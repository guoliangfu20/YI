﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YI.Core.Extensions
{
    public static class ObjectExtension
    {


        private static Regex GuidRegex = new Regex("(?<info>\\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\\}{0,1})", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// 从object中获取Guid?类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <returns>Guid?。</returns>
        public static Guid? GetGuid(this object o)
        {
            Guid info;
            if (!Guid.TryParse(GuidRegex.Match(o.ToString(string.Empty)).Groups["info"].Value, out info))
            {
                return null;
            }
            return info;
        }


        /// <summary>
        /// 将object转换为string类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>string。</returns>
        public static string ToString(this object o, string t)
        {
            string info = string.Empty;
            if (o == null)
            {
                info = t;
            }
            else
            {
                info = o.ToString();
            }
            return info;
        }

        public static object ChangeType(this object convertibleValue, Type type)
        {
            if (null == convertibleValue) return null;

            try
            {
                if (type == typeof(Guid) || type == typeof(Guid?))
                {
                    string value = convertibleValue.ToString();
                    if (value == "") return null;
                    return Guid.Parse(value);
                }

                if (!type.IsGenericType) return Convert.ChangeType(convertibleValue, type);
                if (type.ToString() == "System.Nullable`1[System.Boolean]" || type.ToString() == "System.Boolean")
                {
                    if (convertibleValue.ToString() == "0")
                        return false;
                    return true;
                }
                Type genericTypeDefinition = type.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(Nullable<>))
                {
                    return Convert.ChangeType(convertibleValue, Nullable.GetUnderlyingType(type));
                }
            }
            catch
            {
                return null;
            }
            return null;
        }
    }
}
