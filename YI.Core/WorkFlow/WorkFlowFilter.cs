using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using YI.Core.Enums;
using YI.Core.Extensions;

namespace YI.Core.WorkFlow
{
    public static class WorkFlowFilter
    {
        public static Expression<Func<T, bool>>? Create<T>(List<FieldFilter> filters)
            where T : class
        {
            if (filters == null) return null;

            filters = filters.Where(x => !string.IsNullOrEmpty(x.Field)).ToList();

            if (filters.Any(x => !string.IsNullOrEmpty(x.Value)) == false)   // 所有条件都是空
                return null;

            List<string> fields = typeof(T).GetProperties().Select(p => p.Name).ToList();

            Expression<Func<T, bool>> orFilter = null;
            Expression<Func<T, bool>> expression = x => true;

            foreach (var item in filters)
            {
                if (string.IsNullOrEmpty(item.Value)) continue;
                if (!fields.Contains(item.Field))
                {
                    string msg = $"表【{typeof(T).GetEntityTableName()}】不存在字段【{item.Field}】";
                    Console.WriteLine(msg);
                    throw new Exception(msg);
                }
                item.Value = item.Value.Trim();

                LinqExpressionType type = LinqExpressionType.Equal;
                switch (item.FilterType)
                {
                    case "!=":
                        type = LinqExpressionType.NotEqual;
                        break;
                    case ">":
                        type = LinqExpressionType.GreaterThan;
                        break;
                    case ">=":
                        type = LinqExpressionType.ThanOrEqual;
                        break;
                    case "<":
                        type = LinqExpressionType.LessThan;
                        break;
                    case "<=":
                        type = LinqExpressionType.LessThanOrEqual;
                        break;
                    case "in":
                        type = LinqExpressionType.In;
                        break;
                    case "like":
                        type = LinqExpressionType.Contains;
                        break;
                    default:
                        break;
                }

                if (type == LinqExpressionType.In)
                {
                    var values = item.Value.Split(",").Where(x => !string.IsNullOrEmpty(x)).ToList();
                    if (values.Count > 0)
                    {
                        expression = expression.And(item.Field.CreateExpression<T>(values, type));
                    }
                }
                else if (item.FilterType == "or")
                {
                    if (orFilter == null)
                    {
                        orFilter = x => false;
                    }
                    orFilter = orFilter.Or(item.Field.CreateExpression<T>(item.Value, LinqExpressionType.Equal));
                }
                else
                {
                    expression = expression.And(item.Field.CreateExpression<T>(item.Value, type));
                }

            }
            if (orFilter != null)
            {
                expression = expression.And(orFilter);
            }
            return expression;
        }
    }
}
