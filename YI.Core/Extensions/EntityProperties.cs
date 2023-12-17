using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YI.Core.Configuration;
using YI.Core.Const;
using YI.Entity.AttributeManager;

namespace YI.Core.Extensions
{
    public static class EntityProperties
    {


        /// <summary>
        /// 获取主键字段
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static PropertyInfo GetKeyProperty(this Type entity)
        {
            return entity.GetProperties().GetKeyProperty();
        }
        public static PropertyInfo GetKeyProperty(this PropertyInfo[] properties)
        {
            return properties.Where(c => c.IsKey()).FirstOrDefault();
        }

        public static bool IsKey(this PropertyInfo propertyInfo)
        {
            object[] keyAttributes = propertyInfo.GetCustomAttributes(typeof(KeyAttribute), false);
            if (keyAttributes.Length > 0)
                return true;
            return false;
        }

        /// <summary>
        /// 获取对象里指定成员名称
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="properties"> 格式 Expression<Func<entityt, object>> exp = x => new { x.字段1, x.字段2 };或x=>x.Name</param>
        /// <returns></returns>
        public static string[] GetExpressionProperty<TEntity>(this Expression<Func<TEntity, object>> properties)
        {
            if (properties == null)
                return new string[] { };
            if (properties.Body is NewExpression)
                return ((NewExpression)properties.Body).Members.Select(x => x.Name).ToArray();
            if (properties.Body is MemberExpression)
                return new string[] { ((MemberExpression)properties.Body).Member.Name };
            if (properties.Body is UnaryExpression)
                return new string[] { ((properties.Body as UnaryExpression).Operand as MemberExpression).Member.Name };
            throw new Exception("未实现的表达式");
        }

        /// <summary>
        /// 获取表带有EntityAttribute属性的真实表名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetEntityTableName(this Type type)
        {
            Attribute attribute = type.GetCustomAttribute(typeof(EntityAttribute));
            if (attribute != null && attribute is EntityAttribute)
            {
                return (attribute as EntityAttribute).TableName ?? type.Name;
            }
            return type.Name;
        }

        public static Dictionary<string, string> GetColumType(this PropertyInfo[] properties, bool containsKey)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (PropertyInfo property in properties)
            {
                if (!containsKey && property.IsKey())
                {
                    continue;
                }
                var keyVal = GetColumnType(property, true);
                dictionary.Add(keyVal.Key, keyVal.Value);
            }
            return dictionary;
        }


        private static readonly Dictionary<Type, string> entityMapDbColumnType = new Dictionary<Type, string>() {
                    {typeof(int),SqlDbTypeName.Int },
                    {typeof(int?),SqlDbTypeName.Int },
                    {typeof(long),SqlDbTypeName.BigInt },
                    {typeof(long?),SqlDbTypeName.BigInt },
                    {typeof(decimal),"decimal(18, 5)" },
                    {typeof(decimal?),"decimal(18, 5)"  },
                    {typeof(double),"decimal(18, 5)" },
                    {typeof(double?),"decimal(18, 5)" },
                    {typeof(float),"decimal(18, 5)" },
                    {typeof(float?),"decimal(18, 5)" },
                    {typeof(Guid),"UniqueIdentifier" },
                    {typeof(Guid?),"UniqueIdentifier" },
                    {typeof(byte),"tinyint" },
                    {typeof(byte?),"tinyint" },
                    {typeof(string),"nvarchar" }
        };


        /// <summary>
        /// 返回属性的字段及数据库类型
        /// </summary>
        /// <param name="property"></param>
        /// <param name="lenght">是否包括后字段具体长度:nvarchar(100)</param>
        /// <returns></returns>
        public static KeyValuePair<string, string> GetColumnType(this PropertyInfo property, bool lenght = false)
        {
            string colType = "";
            object objAtrr = property.GetTypeCustomAttributes(typeof(ColumnAttribute), out bool asType);
            if (asType)
            {
                colType = ((ColumnAttribute)objAtrr).TypeName.ToLower();
                if (!string.IsNullOrEmpty(colType))
                {
                    //不需要具体长度直接返回
                    if (!lenght)
                    {
                        return new KeyValuePair<string, string>(property.Name, colType);
                    }
                    if (colType == "decimal" || colType == "double" || colType == "float")
                    {
                        objAtrr = property.GetTypeCustomAttributes(typeof(DisplayFormatAttribute), out asType);
                        colType += "(" + (asType ? ((DisplayFormatAttribute)objAtrr).DataFormatString : "18,5") + ")";

                    }
                    ///如果是string,根据 varchar或nvarchar判断最大长度
                    if (property.PropertyType.ToString() == "System.String")
                    {
                        colType = colType.Split("(")[0];
                        objAtrr = property.GetTypeCustomAttributes(typeof(MaxLengthAttribute), out asType);
                        if (asType)
                        {
                            int length = ((MaxLengthAttribute)objAtrr).Length;
                            colType += "(" + (length < 1 || length > (colType.StartsWith("n") ? 8000 : 4000) ? "max" : length.ToString()) + ")";
                        }
                        else
                        {
                            colType += "(max)";
                        }
                    }
                    return new KeyValuePair<string, string>(property.Name, colType);
                }
            }
            if (entityMapDbColumnType.TryGetValue(property.PropertyType, out string value))
            {
                colType = value;
            }
            else
            {
                colType = SqlDbTypeName.NVarChar;
            }
            if (lenght && colType == SqlDbTypeName.NVarChar)
            {
                colType = "nvarchar(max)";
            }
            return new KeyValuePair<string, string>(property.Name, colType);
        }

        /// <summary>
        /// 获取PropertyInfo指定属性
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetTypeCustomAttributes(this PropertyInfo propertyInfo, Type type, out bool asType)
        {
            object[] attributes = propertyInfo.GetCustomAttributes(type, false);
            if (attributes.Length == 0)
            {
                asType = false;
                return new string[0];
            }
            asType = true;
            return attributes[0];
        }


        public static string GetEntitySql<T>(this IEnumerable<T> entityList,
          bool containsKey = false,
          string sql = null,
          Expression<Func<T, object>> ignoreFileds = null,
          Expression<Func<T, object>> fixedColumns = null,
          FieldType? fieldType = null
          )
        {

            if (entityList == null || entityList.Count() == 0) return "";
            PropertyInfo[] propertyInfo = typeof(T).GetProperties().ToArray();
            if (propertyInfo.Count() == 0)
            {
                propertyInfo = entityList.ToArray()[0].GetType().GetGenericProperties().ToArray();
            }
            propertyInfo = propertyInfo.GetGenericProperties().ToArray();

            string[] arr = null;
            if (fixedColumns != null)
            {
                arr = fixedColumns.GetExpressionToArray();
                PropertyInfo keyProperty = typeof(T).GetKeyProperty();
                propertyInfo = propertyInfo.Where(x => (containsKey && x.Name == keyProperty.Name) || arr.Contains(x.Name)).ToArray();
            }
            if (ignoreFileds != null)
            {
                arr = ignoreFileds.GetExpressionToArray();
                propertyInfo = propertyInfo.Where(x => !arr.Contains(x.Name)).ToArray();
            }

            Dictionary<string, string> dictProperties = propertyInfo.GetColumType(containsKey);
            if (fieldType != null)
            {
                string realType = fieldType.ToString();
                if ((int)fieldType == 0 || (int)fieldType == 1)
                {
                    realType += "(max)";
                }
                dictProperties = new Dictionary<string, string> { { dictProperties.Select(x => x.Key).ToList()[0], realType } };
            }
            if (dictProperties.Keys.Count * entityList.Count() > 50 * 3000)
            {
                throw new Exception("写入数据太多,请分开写入。");
            }

            string cols = string.Join(",", dictProperties.Select(c => "[" + c.Key + "]" + " " + c.Value));
            StringBuilder declareTable = new StringBuilder();

            string tempTablbe = "#" + EntityToSqlTempName.TempInsert.ToString();

            declareTable.Append("CREATE TABLE " + tempTablbe + " (" + cols + ")");
            declareTable.Append("\r\n");

            //参数总数量
            int parCount = (dictProperties.Count) * (entityList.Count());
            int takeCount = 0;
            int maxParsCount = 2050;
            if (parCount > maxParsCount)
            {
                //如果参数总数量超过2100，设置每次分批循环写入表的大小
                takeCount = maxParsCount / dictProperties.Count;
            }

            int count = 0;
            StringBuilder stringLeft = new StringBuilder();
            StringBuilder stringCenter = new StringBuilder();
            StringBuilder stringRight = new StringBuilder();

            int index = 0;
            foreach (T entity in entityList)
            {
                //每1000行需要分批写入(数据库限制每批至多写入1000行数据)
                if (index == 0 || index >= 1000 || takeCount - index == 0)
                {
                    if (stringLeft.Length > 0)
                    {
                        declareTable.AppendLine(
                            stringLeft.Remove(stringLeft.Length - 2, 2).Append("',").ToString() +
                            stringCenter.Remove(stringCenter.Length - 1, 1).Append("',").ToString() +
                            stringRight.Remove(stringRight.Length - 1, 1).ToString());

                        stringLeft.Clear(); stringCenter.Clear(); stringRight.Clear();
                    }

                    stringLeft.AppendLine("exec sp_executesql N'SET NOCOUNT ON;");
                    stringCenter.Append("N'");

                    index = 0; count = 0;
                }
                stringLeft.Append(index == 0 ? "; INSERT INTO  " + tempTablbe + "  values (" : " ");
                index++;
                foreach (PropertyInfo property in propertyInfo)
                {
                    if (!containsKey && property.IsKey()) { continue; }
                    string par = "@v" + count;
                    stringLeft.Append(par + ",");
                    stringCenter.Append(par + " " + dictProperties[property.Name] + ",");
                    object val = property.GetValue(entity);
                    if (val == null)
                    {
                        stringRight.Append(par + "=NUll,");
                    }
                    else
                    {
                        stringRight.Append(par + "='" + val.ToString().Replace("'", "''''") + "',");
                    }
                    count++;
                }
                stringLeft.Remove(stringLeft.Length - 1, 1);
                stringLeft.Append("),(");
            }

            if (stringLeft.Length > 0)
            {
                declareTable.AppendLine(
                    stringLeft.Remove(stringLeft.Length - 2, 2).Append("',").ToString() +
                    stringCenter.Remove(stringCenter.Length - 1, 1).Append("',").ToString() +
                    stringRight.Remove(stringRight.Length - 1, 1).ToString());

                stringLeft.Clear(); stringCenter.Clear(); stringRight.Clear();
            }
            if (!string.IsNullOrEmpty(sql))
            {
                sql = sql.Replace(EntityToSqlTempName.TempInsert.ToString(), tempTablbe);
                declareTable.AppendLine(sql);
            }
            else
            {
                declareTable.AppendLine(" SELECT " + (string.Join(",", fixedColumns?.GetExpressionToArray() ?? new string[] { "*" })) + " FROM " + tempTablbe);
            }


            if (tempTablbe.Substring(0, 1) == "#")
            {
                declareTable.AppendLine("; drop table " + tempTablbe);
            }
            return declareTable.ToString();
        }

        public static string GetKeyName(this Type typeinfo)
        {
            return typeinfo.GetProperties().GetKeyName();
        }

        public static string GetKeyName(this PropertyInfo[] properties)
        {
            return properties.GetKeyName(false);
        }
        /// <summary>
        /// 获取key列名
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="keyType">true获取key对应类型,false返回对象Key的名称</param>
        /// <returns></returns>
        public static string GetKeyName(this PropertyInfo[] properties, bool keyType)
        {
            string keyName = string.Empty;
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (!propertyInfo.IsKey())
                    continue;
                if (!keyType)
                    return propertyInfo.Name;
                var attributes = propertyInfo.GetCustomAttributes(typeof(ColumnAttribute), false);
                //如果没有ColumnAttribute的需要单独再验证，下面只验证有属性的
                if (attributes.Length > 0)
                    return ((ColumnAttribute)attributes[0]).TypeName.ToLower();
                else
                    return GetColumType(new PropertyInfo[] { propertyInfo }, true)[propertyInfo.Name];
            }
            return keyName;
        }

    }

    public enum EntityToSqlTempName
    {
        TempInsert = 0
    }
}
