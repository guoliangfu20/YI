using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YI.Core.Configuration;
using YI.Core.Const;
using YI.Core.Dapper;
using YI.Core.Enums;

namespace YI.Core.EFDbContext
{
    public class DBServerProvider
    {
        private static Dictionary<string, string> ConnectionPool = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static readonly string DefaultConnName = "default";


        public DBServerProvider()
        {
            SetConnection(DefaultConnName, AppSetting.DbConnectionString);
        }


        public static void SetConnection(string key, string val)
        {
            if (ConnectionPool.ContainsKey(key))
            {
                ConnectionPool[key] = val;
                return;
            }
            ConnectionPool.Add(key, val);
        }

        /// <summary>
        /// 设置默认数据库连接
        /// </summary>
        /// <param name="val"></param>
        public static void SetDefaultConnection(string val)
        {
            SetConnection(DefaultConnName, val);
        }


        /// <summary>
        /// 获取默认数据库连接
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString()
        {
            return GetConnectionString(DefaultConnName);
        }

        public static string GetConnectionString(string key)
        {
            key = key ?? DefaultConnName;
            if (ConnectionPool.ContainsKey(key))
            {
                return ConnectionPool[key];
            }
            return key;
        }

        public static IDbConnection GetDbConnection(string connString = null)
        {
            connString ??= ConnectionPool[DefaultConnName];
            if (DBType.Name == DbCurrentType.MySql.ToString())
            {
                return new MySqlConnection(connString);
            }
            if (DBType.Name == DbCurrentType.PgSql.ToString())
            {
                return new NpgsqlConnection(connString);
            }
            return new SqlConnection(connString);
        }


        public static YIContext DbContext
        {
            get { return GetEFDbContext(); }
        }
        public static YIContext GetEFDbContext()
        {
            return GetEFDbContext(null);
        }
        public static YIContext GetEFDbContext(string dbName)
        {
            YIContext context = Utilities.HttpContext.Current.RequestServices.GetService(typeof(YIContext)) as YIContext;
            if (dbName != null)
            {
                if (!ConnectionPool.ContainsKey(dbName))
                {
                    throw new Exception("数据库连接名称错误");
                }
                context.Database.GetDbConnection().ConnectionString = ConnectionPool[dbName];
            }
            return context;
        }


        /// <summary>
        /// 扩展dapper 获取MSSQL数据库DbConnection，默认系统获取配置文件的DBType数据库类型，
        /// </summary>
        /// <param name="connString">如果connString为null 执行重载GetDbConnection(string connString = null)</param>
        /// <param name="dapperType">指定连接数据库的类型：MySql/MsSql/PgSql</param>
        /// <returns></returns>
        public static IDbConnection GetDbConnection(string connString = null, DbCurrentType dbCurrentType = DbCurrentType.Default)
        {
            //默认获取DbConnection
            if (connString.IsNullOrEmpty() || DbCurrentType.Default == dbCurrentType)
            {
                return GetDbConnection(connString);
            }
            if (dbCurrentType == DbCurrentType.MySql)
            {
                return new MySqlConnection(connString);
            }
            if (dbCurrentType == DbCurrentType.PgSql)
            {
                return new NpgsqlConnection(connString);
            }
            return new SqlConnection(connString);

        }


        public static ISqlDapper SqlDapper
        {
            get
            {
                return new SqlDapper(DefaultConnName);
            }
        }
        public static ISqlDapper GetSqlDapper(string dbName = null)
        {
            return new SqlDapper(dbName ?? DefaultConnName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbCurrentType">指定数据库类型：MySql/MsSql/PgSql</param>
        /// <param name="dbName">指定数据连串名称</param>
        /// <returns></returns>
        public static ISqlDapper GetSqlDapper(DbCurrentType dbCurrentType, string dbName = null)
        {
            if (dbName.IsNullOrEmpty())
            {
                return new SqlDapper(dbName ?? DefaultConnName);
            }
            return new SqlDapper(dbName, dbCurrentType);
        }
    }
}
