using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YI.Core.Extensions
{
    public static class GenericExtension
    {

        public static DataTable ToDataTable<T>(this IEnumerable<T> source, Expression<Func<T, object>> columns = null, bool contianKey = true)
        {
            DataTable dtReturn = new DataTable();
            if (source == null) return dtReturn;

            PropertyInfo[] oProps = typeof(T).GetProperties()
                .Where(x => x.PropertyType.Name != "List`1").ToArray();
            if (columns != null)
            {
                string[] columnArray = columns.GetExpressionToArray();
                oProps = oProps.Where(x => columnArray.Contains(x.Name)).ToArray();
            }
            //移除自增主键
            PropertyInfo keyType = oProps.GetKeyProperty();// oProps.GetKeyProperty()?.PropertyType;
            if (!contianKey && keyType != null && (keyType.PropertyType == typeof(int) || keyType.PropertyType == typeof(long)))
            {
                oProps = oProps.Where(x => x.Name != keyType.Name).ToArray();
            }

            foreach (var pi in oProps)
            {
                var colType = pi.PropertyType;

                if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    colType = colType.GetGenericArguments()[0];
                }

                dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
            }
            foreach (var rec in source)
            {
                var dr = dtReturn.NewRow();
                foreach (var pi in oProps)
                {
                    dr[pi.Name] = pi.GetValue(rec, null) == null
                        ? DBNull.Value
                        : pi.GetValue
                            (rec, null);
                }
                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }
    }
}
