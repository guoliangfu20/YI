using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using YI.Core.Extensions;

namespace YI.Core.WorkFlow
{
    public class WorkFlowContainer
    {
        private static WorkFlowContainer _instance;

        private static Dictionary<string, string> _container = [];
        private static Dictionary<string, string[]> _filterFields = [];
        private static Dictionary<string, string[]> _formFields = [];
        private static List<Type> _types = [];


        public static WorkFlowContainer Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;

                }
                _instance = new WorkFlowContainer();
                return _instance;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">流程实例名称</param>
        /// <param name="filterFields">流程配置可筛选条件字段</param>
        ///<param name="formFields">审批界面要显示字段</param>
        /// <returns></returns>
        public WorkFlowContainer Use<T>(string name = null, Expression<Func<T, object>> filterFields = null, Expression<Func<T, object>> formFields = null)
        {
            Type type = typeof(T);
            if (_types.Contains(type))
            {
                return _instance;
            }

            _container[type.Name] = name ?? typeof(T).GetEntityTableCnName();
            if (filterFields != null)
            {
                _filterFields[type.Name] = filterFields.GetExpressionToArray();
            }
            if (formFields != null)
            {
                _formFields[type.Name] = formFields.GetExpressionToArray();
            }
            _types.Add(type);
            return _instance;
        }

        public static bool Exists<T>()
        {
            return Exists(typeof(T).GetEntityTableName());
        }

        public static bool Exists(string table)
        {
            return _container.ContainsKey(table);
        }

        public static Type GetType(string tableName)
        {
            return _types.Where(c => c.Name == tableName).FirstOrDefault();
        }

        public static string[] GetFilterFields(string tableName)
        {
            _filterFields.TryGetValue(tableName, out string[] fields);
            return fields;
        }

        public static string[] GetFormFields(string tableName)
        {
            _formFields.TryGetValue(tableName, out string[] fields);
            return fields;
        }
    }
}
