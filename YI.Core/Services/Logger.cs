using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using YI.Core.Configuration;
using YI.Core.Const;
using YI.Core.EFDbContext;
using YI.Core.Enums;
using YI.Core.Extensions;
using YI.Core.UserManager;
using YI.Entity.DomainModel.System;

namespace YI.Core.Services
{
    public static class Logger
    {
        public static ConcurrentQueue<Sys_Log> loggerQueueData = new ConcurrentQueue<Sys_Log>();
        private static DateTime lastClearFileDT = DateTime.Now.AddDays(-1);
        private static string _loggerPath = AppSetting.DownLoadPath + "Logs\\Queue\\";

        static Logger()
        {
            Task.Run(() =>
            {
                Start();
                if (!"MySql".Equals(DBType.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                try
                {
                    DBServerProvider.SqlDapper.ExcuteNonQuery(" set global local_infile = 'ON'; ", null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"日志启动调用mysql数据库异常：{ex.Message},{ex.StackTrace}");
                }
            });
        }

        public static void Info(string message)
        {
            Info(LoggerType.Info, message);
        }
        public static void Info(LoggerType loggerType, string message = null)
        {
            Info(loggerType, message, null, null);
        }
        public static void Info(LoggerType loggerType, string requestParam, string resposeParam, string ex = null)
        {
            Add(loggerType, requestParam, resposeParam, ex, LoggerStatus.Info);
        }



        public static void Error(string message)
        {
            Error(LoggerType.Error, message);
        }
        public static void Error(LoggerType loggerType, string message)
        {
            Error(loggerType, message, null, null);
        }
        public static void Error(LoggerType loggerType, string requestParam, string resposeParam, string ex = null)
        {
            Add(loggerType, requestParam, resposeParam, ex, LoggerStatus.Error);
        }


        public static void Success(string message)
        {
            Success(LoggerType.Success, message);
        }
        public static void Success(LoggerType loggerType, string message = null)
        {
            Success(loggerType, message, null, null);
        }
        public static void Success(LoggerType loggerType, string requestParam, string resposeParam, string ex = null)
        {
            Add(loggerType, requestParam, resposeParam, ex, LoggerStatus.Success);
        }


        /// <summary>
        /// 多线程调用日志
        /// </summary>
        /// <param name="message"></param>
        public static void AddAsync(string message, string ex = null)
        {
            AddAsync(LoggerType.Info, null, message, ex, ex != null ? LoggerStatus.Error : LoggerStatus.Info);
        }
        public static void AddAsync(LoggerType loggerType, string requestParameter, string responseParameter, string ex, LoggerStatus status)
        {
            var log = new Sys_Log()
            {
                BeginDate = DateTime.Now,
                EndDate = DateTime.Now,
                User_Id = 0,
                UserName = "",
                //  Role_Id = ,
                LogType = loggerType.ToString(),
                ExceptionInfo = ex,
                RequestParameter = requestParameter,
                ResponseParameter = responseParameter,
                Success = (int)status
            };
            loggerQueueData.Enqueue(log);
        }


        private static void Add(LoggerType loggerType, string requestParameter, string responseParameter, string ex, LoggerStatus status)
        {
            Sys_Log log = null;
            try
            {
                HttpContext context = Utilities.HttpContext.Current;
                if (context.Request.Method == "OPTIONS") return;
                ActionObserver cctionObserver = context.RequestServices.GetService(typeof(ActionObserver)) as ActionObserver;
                if (context == null)
                {
                    WriteText($"未获取到httpcontext信息,type:{loggerType.ToString()},reqParam:{requestParameter},respParam:{responseParameter},ex:{ex},success:{status.ToString()}");
                    return;
                }
                UserInfo userInfo = UserContext.Current.UserInfo;
                log = new Sys_Log()
                {
                    BeginDate = cctionObserver.RequestDate,
                    EndDate = DateTime.Now,
                    User_Id = userInfo.User_Id,
                    UserName = userInfo.UserTrueName,
                    Role_Id = userInfo.Role_Id,
                    LogType = loggerType.ToString(),
                    ExceptionInfo = ex,
                    RequestParameter = requestParameter,
                    ResponseParameter = responseParameter,
                    Success = (int)status
                };
                SetServicesInfo(log, context);
            }
            catch (Exception exception)
            {
                log = log ?? new Sys_Log()
                {
                    BeginDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    LogType = loggerType.ToString(),
                    RequestParameter = requestParameter,
                    ResponseParameter = responseParameter,
                    Success = (int)status,
                    ExceptionInfo = ex + exception.Message
                };
            }
            loggerQueueData.Enqueue(log);
        }

        public static void SetServicesInfo(Sys_Log log, HttpContext context)
        {
            log.Url = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase +
                context.Request.Path;

            log.UserIP = context.GetUserIp()?.Replace("::ffff:", "");
            log.ServiceIP = context.Connection.LocalIpAddress.MapToIPv4().ToString() + ":" + context.Connection.LocalPort;

            log.BrowserType = context.Request.Headers["User-Agent"];
            if (log.BrowserType != null && log.BrowserType.Length > 190)
            {
                log.BrowserType = log.BrowserType.Substring(0, 190);
            }
            if (string.IsNullOrEmpty(log.RequestParameter))
            {
                try
                {
                    log.RequestParameter = context.GetRequestParameters();
                    if (log.RequestParameter != null)
                    {
                        log.RequestParameter = HttpUtility.UrlDecode(log.RequestParameter, Encoding.UTF8);
                    }
                }
                catch (Exception ex)
                {
                    log.ExceptionInfo += $"日志读取参数出错:{ex.Message}";
                    Console.WriteLine($"日志读取参数出错:{ex.Message}");
                }
            }
        }

        private static void Start()
        {
            DataTable queueTable = CreateEmptyTable();
            while (true)
            {
                try
                {
                    if (loggerQueueData.Count() > 0 && queueTable.Rows.Count < 500)
                    {
                        DequeueToTable(queueTable);
                        continue;
                    }

                    //每5秒写一次数据
                    Thread.Sleep(5000);
                    if (queueTable.Rows.Count == 0) continue;

                    DBServerProvider.SqlDapper.BulkInsert(queueTable, "Sys_Log", SqlBulkCopyOptions.KeepIdentity, null, _loggerPath);

                    queueTable.Clear();

                    if ((DateTime.Now - lastClearFileDT).TotalDays > 1)
                    {
                        Utilities.FileHelper.DeleteFolder(_loggerPath);
                        lastClearFileDT = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"日志批量写入数据时出错:{ex.Message}");
                    WriteText(ex.Message + ex.StackTrace + ex.Source);
                    queueTable.Clear();
                }
            }
        }

        private static void WriteText(string message)
        {
            try
            {
                Utilities.FileHelper.WriteFile(
                    _loggerPath + "Error\\",  // path
                    $"{DateTime.Now.ToString("yyyyMMdd")}.txt", // filename
                    message + "\r\n");  // content
            }
            catch (Exception ex)
            {
                Console.WriteLine($"日志写入文件时出错:{ex.Message}");
            }
        }


        private static DataTable CreateEmptyTable()
        {
            DataTable queueTable = new DataTable();
            queueTable.Columns.Add("LogType", typeof(string));
            queueTable.Columns.Add("RequestParameter", typeof(string));
            queueTable.Columns.Add("ResponseParameter", typeof(string));
            queueTable.Columns.Add("ExceptionInfo", typeof(string));
            queueTable.Columns.Add("Success", Type.GetType("System.Int32"));
            queueTable.Columns.Add("BeginDate", Type.GetType("System.DateTime"));
            queueTable.Columns.Add("EndDate", Type.GetType("System.DateTime"));
            queueTable.Columns.Add("ElapsedTime", Type.GetType("System.Int32"));
            queueTable.Columns.Add("UserIP", typeof(string));
            queueTable.Columns.Add("ServiceIP", typeof(string));
            queueTable.Columns.Add("BrowserType", typeof(string));
            queueTable.Columns.Add("Url", typeof(string));
            queueTable.Columns.Add("User_Id", Type.GetType("System.Int32"));
            queueTable.Columns.Add("UserName", typeof(string));
            queueTable.Columns.Add("Role_Id", Type.GetType("System.Int32"));
            return queueTable;
        }


        private static void DequeueToTable(DataTable queueTable)
        {
            loggerQueueData.TryDequeue(out Sys_Log log);
            DataRow row = queueTable.NewRow();

            if (log.BeginDate == null || log.BeginDate?.Year < 2010)
            {
                log.BeginDate = DateTime.Now;
            }
            if (log.EndDate == null)
            {
                log.EndDate = DateTime.Now;
            }

            row["LogType"] = log.LogType;
            row["RequestParameter"] = log.RequestParameter?.Replace("\r\n", "");
            row["ResponseParameter"] = log.ResponseParameter?.Replace("\r\n", "");
            row["ExceptionInfo"] = log.ExceptionInfo;
            row["Success"] = log.Success ?? -1;
            row["BeginDate"] = log.BeginDate;
            row["EndDate"] = log.EndDate;
            row["ElapsedTime"] = ((DateTime)log.EndDate - (DateTime)log.BeginDate).TotalMilliseconds;
            row["UserIP"] = log.UserIP;
            row["ServiceIP"] = log.ServiceIP;
            row["BrowserType"] = log.BrowserType;
            row["Url"] = log.Url;
            row["User_Id"] = log.User_Id ?? -1;
            row["UserName"] = log.UserName;
            row["Role_Id"] = log.Role_Id ?? -1;
            queueTable.Rows.Add(row);
        }

    }
    public enum LoggerStatus
    {
        Success = 1,
        Error = 2,
        Info = 3
    }
}
