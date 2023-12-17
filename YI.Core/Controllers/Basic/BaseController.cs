using Autofac.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YI.Core.Configuration;
using YI.Core.Extensions;
using YI.Core.Services;
using YI.Core.Utilities.Response;
using YI.Entity.DomainModel.Core;

namespace YI.Core.Controllers.Basic
{
    public class BaseController<IServiceBase> : Controller
    {
        protected IServiceBase serviceBase;

        private WebResponseContent responseContent { get; set; }

        public BaseController() { }


        public BaseController(IServiceBase serviceBase)
        {
            this.serviceBase = serviceBase;
        }

        public BaseController(string projectName, string folder, string tablename, IServiceBase service)
        {
            serviceBase = service;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual ActionResult Manager()
        {
            return View();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual async Task<ActionResult> GetPageData(PageDataOptions loadData)
        {
            string pageData = await Task.FromResult(InvokeService("GetPageData", new object[] { loadData }).Serialize());
            return Content(pageData);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual async Task<ActionResult> GetDetailPage(PageDataOptions loadData)
        {
            string pageData = await Task.FromResult(InvokeService("GetDetailPage", new object[] { loadData }).Serialize());
            return Content(pageData);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="fileInput"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual async Task<ActionResult> Upload(List<IFormFile> fileInput)
        {
            object result = await Task.FromResult(InvokeService("Upload", new object[] { fileInput }));
            return Json(result);
        }

        /// <summary>
        /// 导入表数据Excel
        /// </summary>
        /// <param name="fileInput"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<ActionResult> Import(List<IFormFile> fileInput)
        {
            object result = await Task.FromResult(InvokeService("Import", new object[] { fileInput }));
            return Json(result);
        }

        /// <summary>
        /// 导出表格
        /// </summary>
        /// <param name="loadData"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual async Task<ActionResult> Export(PageDataOptions loadData)
        {
            return Json(await Task.FromResult(InvokeService("Export", new object[] { loadData })));
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual ActionResult DownLoadFile(string path)
        {
            if (string.IsNullOrEmpty(path)) return Content("未找到文件");

            try
            {
                if (path.IndexOf("/") == -1 && path.IndexOf("\\") == -1)
                {
                    path = path.DecryptDES(AppSetting.Secret.ExportFile);
                }
                else
                {
                    path = path.MapPath();
                }
                string fileName = Path.GetFileName(path);
                return File(System.IO.File.ReadAllBytes(path), System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception ex)
            {
                Logger.Error($"文件下载出错:{path}{ex.Message}");
            }
            return Content("");
        }


        /// <summary>
        /// 反射调用service方法
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private object InvokeService(string methodName, object[] parameters)
        {
            return serviceBase.GetType().GetMethod(methodName).Invoke(serviceBase, parameters);
        }

        /// <summary>
        /// 反射调用service方法
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="types">为要调用重载的方法参数类型：new Type[] { typeof(SaveDataModel)</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private object InvokeService(string methodName, Type[] types, object[] parameters)
        {
            return serviceBase.GetType().GetMethod(methodName, types).Invoke(serviceBase, parameters);
        }

    }
}
