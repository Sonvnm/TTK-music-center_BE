using HMZ.DTOs.Models;
using HMZ.DTOs.Queries.Base;
using System.Linq.Expressions;
namespace HMZ.SDK.Extensions
{
    public static class QueryFillter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TFilter"></typeparam>
        /// <param name="source"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<T> ApplyFilter<T, TFilter>(this IQueryable<T> source, BaseQuery<TFilter> query)
        where TFilter : class
        {

            if (query.Entity == null)
            {
                return source;
            }
            /// <summary>
            /// <para>var parameter = Expression.Parameter(typeof(T), "e");</para>
            /// <para>var predicate = BinaryExpression.Equal(Expression.Constant(null), Expression.Constant(null));</para>
            /// </summary>
            var parameter = Expression.Parameter(typeof(T), "e");
            var predicate = BinaryExpression.Equal(Expression.Constant(null), Expression.Constant(null));

            foreach (var property in typeof(TFilter).GetProperties())
            {

                var value = property.GetValue(query.Entity);
                if (string.IsNullOrEmpty(value?.ToString()))
                {
                    continue;
                }
                var propertyExpr = Expression.Property(parameter, property.Name);

                Expression conditionExpr = null;
                if (value is string stringValue)
                {
                    var methodInfo = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    conditionExpr = Expression.Call(propertyExpr, methodInfo, Expression.Constant(stringValue));

                }
                else if (value is Guid guidValue)
                {
                    var guidExpr = Expression.Constant(guidValue, typeof(Guid?));
                    conditionExpr = Expression.Equal(propertyExpr, guidExpr);
                }
                else if (value is bool boolValue)
                {
                    var boolExpr = Expression.Constant(boolValue, typeof(bool?));
                    conditionExpr = Expression.Equal(propertyExpr, boolExpr);
                }
                // Type Enum
                else if(value is Enum enumValue){
                    var enumExpr = Expression.Constant(enumValue, propertyExpr.Type);
                    conditionExpr = Expression.Equal(propertyExpr, enumExpr);    
                }

                else if (value is RangeFilter<int?> intRangeValue)
                {
                    var propertyType = Nullable.GetUnderlyingType(propertyExpr.Type)??propertyExpr.Type;
                    
                    // 2 is null
                    if (!intRangeValue.FromValue.HasValue && !intRangeValue.ToValue.HasValue)
                    {
                        continue;
                    }
                    if (intRangeValue.FromValue.HasValue && intRangeValue.ToValue.HasValue)
                    {
                        var fromIntExpr = Expression.Constant(Convert.ChangeType(intRangeValue.FromValue, propertyType));
                        var toIntExpr = Expression.Constant(Convert.ChangeType(intRangeValue.ToValue, propertyType));

                        conditionExpr = Expression.AndAlso(
                        Expression.GreaterThanOrEqual(Expression.Convert(propertyExpr, propertyType), fromIntExpr),
                        Expression.LessThanOrEqual(Expression.Convert(propertyExpr, propertyType), toIntExpr));

                    }
                    if (intRangeValue.FromValue.HasValue && !intRangeValue.ToValue.HasValue)
                    {
                        var fromIntExpr = Expression.Constant(Convert.ChangeType(intRangeValue.FromValue, propertyType));
                        conditionExpr = Expression.GreaterThanOrEqual(propertyExpr, fromIntExpr);
                    }
                    if (!intRangeValue.FromValue.HasValue && intRangeValue.ToValue.HasValue)
                    {
                        var toIntExpr = Expression.Constant(Convert.ChangeType(intRangeValue.ToValue, propertyType));
                        conditionExpr = Expression.LessThanOrEqual(propertyExpr, toIntExpr);
                    }
                }

                else if (value is RangeFilter<decimal?> decimalRangeValue)
                {
                    // 2 is null
                    if (!decimalRangeValue.FromValue.HasValue && !decimalRangeValue.ToValue.HasValue)
                    {
                        continue;
                    }
                    if (decimalRangeValue.FromValue.HasValue && decimalRangeValue.ToValue.HasValue)
                    {
                        var fromDecimal = Expression.Constant(Convert.ChangeType(decimalRangeValue.FromValue, propertyExpr.Type));
                        var toDecimal = Expression.Constant(Convert.ChangeType(decimalRangeValue.ToValue, propertyExpr.Type));

                        conditionExpr = Expression.AndAlso(
                            Expression.GreaterThanOrEqual(propertyExpr, fromDecimal),
                            Expression.LessThanOrEqual(propertyExpr, toDecimal));

                    }
                    if (decimalRangeValue.FromValue.HasValue && !decimalRangeValue.ToValue.HasValue)
                    {
                        var fromDecimalExpr = Expression.Constant(Convert.ChangeType(decimalRangeValue.FromValue, propertyExpr.Type));
                        conditionExpr = Expression.GreaterThanOrEqual(propertyExpr, fromDecimalExpr);
                    }
                    if (!decimalRangeValue.FromValue.HasValue && decimalRangeValue.ToValue.HasValue)
                    {
                        var toDecimalExpr = Expression.Constant(Convert.ChangeType(decimalRangeValue.ToValue, propertyExpr.Type));
                        conditionExpr = Expression.LessThanOrEqual(propertyExpr, toDecimalExpr);
                    }
                    
                }

                else if (value is RangeFilter<DateTime?> dateRangeValue)
                {
                    
                    if (!dateRangeValue.FromValue.HasValue && !dateRangeValue.ToValue.HasValue)
                    {
                        continue;
                    }
                    if (dateRangeValue.FromValue.HasValue && dateRangeValue.ToValue.HasValue)
                    {
                        var fromDateExpr = Expression.Constant(dateRangeValue.FromValue.Value.Date, typeof(DateTime?));
                        var toDateExpr = Expression.Constant(dateRangeValue.ToValue.Value.Date, typeof(DateTime?));
                        var greaterThanExpr = Expression.GreaterThanOrEqual(propertyExpr, fromDateExpr);
                        var lessThanExpr = Expression.LessThanOrEqual(propertyExpr, toDateExpr);

                        conditionExpr = Expression.AndAlso(greaterThanExpr, lessThanExpr);
                    }
                    if (dateRangeValue.FromValue.HasValue && !dateRangeValue.ToValue.HasValue)
                    {
                        var fromDateExpr = Expression.Constant(dateRangeValue.FromValue.Value.Date, typeof(DateTime?));
                        conditionExpr = Expression.GreaterThanOrEqual(propertyExpr, fromDateExpr);
                    }
                    if (!dateRangeValue.FromValue.HasValue && dateRangeValue.ToValue.HasValue)
                    {
                        var toDateExpr = Expression.Constant(dateRangeValue.ToValue.Value.Date, typeof(DateTime?));
                        conditionExpr = Expression.LessThanOrEqual(propertyExpr, toDateExpr);
                    }
                }

                predicate = Expression.AndAlso(predicate, conditionExpr);
            }

            var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);
            return source.Where(lambda);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="columns"></param>
        /// <param name="isDesc"></param>
        /// <returns></returns>
        public static IQueryable<T> OrderByColumns<T>(this IQueryable<T> source, List<string> columns, bool? isDesc= true)
        {
            if (columns == null || columns.Count == 0)
            {
                if (typeof(T).GetProperties().Any(y => y.Name.ToUpper() == "UPDATEDAT"))
                {
                    columns = new List<string> { "createdAt","updatedAt" };
                }
                else
                {
                    columns = new List<string> { "createdAt" };
                }
            }
            bool isExistProperty = columns.Any(x => typeof(T).GetProperties().Any(y => y.Name.ToUpper() == x.ToUpper()));
            if (!isExistProperty)
            {
                return source;
            }
            var parameter = Expression.Parameter(typeof(T), "e");
            var propertyExpr = Expression.Property(parameter, columns[0]);
            var lambda = Expression.Lambda<Func<T, object>>(Expression.Convert(propertyExpr, typeof(object)), parameter);
            var method = isDesc.Value ==true ? "OrderByDescending" : "OrderBy";
            var result = source.Provider.CreateQuery<T>(
                Expression.Call(
                    typeof(Queryable),
                    method,
                    new Type[] { source.ElementType, lambda.Body.Type },
                    source.Expression,
                    lambda));
            for (int i = 1; i < columns.Count; i++)
            {
                propertyExpr = Expression.Property(parameter, columns[i]);
                lambda = Expression.Lambda<Func<T, object>>(Expression.Convert(propertyExpr, typeof(object)), parameter);
                method = isDesc.HasValue ? "ThenByDescending" : "ThenBy";
                result = result.Provider.CreateQuery<T>(
                    Expression.Call(
                        typeof(Queryable),
                        method,
                        new Type[] { source.ElementType, lambda.Body.Type },
                        result.Expression,
                        lambda));
            }
            return result;
        }
    
    
        /// <summary>
        ///  WhereIf(x.Username != null, x => x.Username == "admin")
        /// </summary>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
        {
            if (condition)
            {
                return source.Where(predicate);
            }
            return source;
        }
    
    }
}
