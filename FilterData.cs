using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.FilterExtensions
{
    public class FilterModel
    {
        public string ColumnName { get; set; }
        public string Value { get; set; }
        public FilterType FilterType { get; set; } = FilterType.Contain;
        public FilterModel(string columnName, string value, FilterType filterType)
        {
            ColumnName = columnName;
            Value = value;
            FilterType = filterType;
        }
    }

    public enum FilterType
    {
        Equal = 1,
        NotEqual,
        Contain,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        StartWith,
        EndWith,
    }

    public static class FilterData
    {
        public static IQueryable<T> Filter<T>(this IQueryable<T> data, List<List<FilterModel>> filters)
        {
            foreach (var check in filters)
            {
                if (check.Count == 0)
                    return Enumerable.Empty<T>().AsQueryable();
            }
            Expression expression = null;

            var parameter = Expression.Parameter(typeof(T), "x");

            foreach (var item in filters)
            {
                List<Expression> expressions = new List<Expression>();
                foreach (var filter in item)
                {
                    var filterColumn = Expression.Property(parameter, filter.ColumnName);

                    if (filterColumn.Type == typeof(string))
                    {
                        var filterValue = Expression.Constant(filter.Value);
                        var filterNameValue = Expression.Convert(filterValue, filterColumn.Type);
                        var filterData = GetFilterType<T>(filter, filterColumn, filterNameValue);
                        expressions.Add(filterData);
                    }
                    else if (filterColumn.Type == typeof(bool))
                    {
                        var filterValueBool = bool.Parse(filter.Value);
                        var filterColumnBool = Expression.Convert(Expression.Property(parameter, filter.ColumnName), typeof(bool));
                        var filterValue = Expression.Constant(filterValueBool);
                        var filterNameValue = Expression.Convert(filterValue, filterColumnBool.Type);
                        var filterData = GetFilterType<T>(filter, filterColumn, filterNameValue);
                        expressions.Add(filterData);
                    }
                    else if (filterColumn.Type == typeof(DateTime) || filterColumn.Type == typeof(Nullable<DateTime>))
                    {
                        var filterValueDate = DateTime.Parse(filter.Value);
                        var filterColumnDate = Expression.Convert(Expression.Property(parameter, filter.ColumnName), filterColumn.Type);
                        var filterValue = Expression.Constant(filterValueDate);
                        var filterNameValue = Expression.Convert(filterValue, filterColumnDate.Type);
                        var filterData = GetFilterType<T>(filter, filterColumn, filterNameValue);
                        expressions.Add(filterData);
                    }
                    else if (filterColumn.Type == typeof(decimal))
                    {
                        var filterValueBool = decimal.Parse(filter.Value);
                        var filterColumnBool = Expression.Convert(Expression.Property(parameter, filter.ColumnName), typeof(decimal));
                        var filterValue = Expression.Constant(filterValueBool);
                        var filterNameValue = Expression.Convert(filterValue, filterColumnBool.Type);
                        var filterData = GetFilterType<T>(filter, filterColumn, filterNameValue);
                        expressions.Add(filterData);
                    }
                    else if (filterColumn.Type == typeof(int) || filterColumn.Type == typeof(Nullable<int>))
                    {
                        var filterValueInt = int.Parse(filter.Value);
                        var filterColumnInt = Expression.Convert(Expression.Property(parameter, filter.ColumnName), filterColumn.Type);
                        var filterValue = Expression.Constant(filterValueInt);
                        var filterNameValue = Expression.Convert(filterValue, filterColumnInt.Type);
                        var filterData = GetFilterType<T>(filter, filterColumn, filterNameValue);
                        expressions.Add(filterData);
                    }

                    //in progress
                    else {
                        var genericParamater = Expression.Parameter(filterColumn.Type, "y");
                        var genericColumn = filterColumn.Type.GetProperty("Name");                      
                        var filterGenericColumn = Expression.Property(filterColumn, genericColumn);
                        var nameValue = Expression.Constant(filter.Value);
                        var filterNameValue = Expression.Convert(nameValue, nameValue.Type);
                        var nameEquality = GetFilterType<T>(filter, filterGenericColumn, filterNameValue);
                        expressions.Add(nameEquality);                        
                                               
                    }
                }

                if (expression is null)
                {
                    expression = expressions.Select(filter => filter).Aggregate(Expression.Or);
                }
                else
                {
                    expression = Expression.And(expression, expressions.Select(filter => filter).Aggregate(Expression.Or));
                }
            }

            return data.Where(Expression.Lambda<Func<T, bool>>(expression, parameter));
        }

        private static Expression GetFilterType<T>(FilterModel filterValue, MemberExpression memberExpression, UnaryExpression unaryExpression)
        {
            var Contains = memberExpression.Type.GetMethod("Contains", new Type[] { typeof(string) });
            var StartsWith = memberExpression.Type.GetMethod("StartsWith", new Type[] { typeof(string) });
            var EndsWith = memberExpression.Type.GetMethod("EndsWith", new Type[] { typeof(string) });


            if (unaryExpression.Type == typeof(string))
            {
                switch (filterValue.FilterType)
                {
                    case FilterType.Contain:
                        return Expression.Call(memberExpression, Contains, unaryExpression);
                    case FilterType.Equal:
                        return Expression.Equal(memberExpression, unaryExpression);
                    case FilterType.NotEqual:
                        return Expression.NotEqual(memberExpression, unaryExpression);
                    case FilterType.StartWith:
                        return Expression.Call(memberExpression, StartsWith, unaryExpression);
                    case FilterType.EndWith:
                        return Expression.Call(memberExpression, EndsWith, unaryExpression);
                }
            }
            if (unaryExpression.Type == typeof(int) || unaryExpression.Type == typeof(Nullable<int>) || unaryExpression.Type == typeof(decimal))
            {
                switch (filterValue.FilterType)
                {
                    case FilterType.Equal:
                        return Expression.Equal(memberExpression, unaryExpression);
                    case FilterType.NotEqual:
                        return Expression.NotEqual(memberExpression, unaryExpression);
                    case FilterType.GreaterThan:
                        return Expression.GreaterThan(memberExpression, unaryExpression);
                    case FilterType.GreaterThanOrEqual:
                        return Expression.GreaterThanOrEqual(memberExpression, unaryExpression);
                    case FilterType.LessThan:
                        return Expression.LessThan(memberExpression, unaryExpression);
                    case FilterType.LessThanOrEqual:
                        return Expression.LessThanOrEqual(memberExpression, unaryExpression);
                    default:
                        break;
                }
            }

            if (unaryExpression.Type == typeof(bool))
            {
                return Expression.Equal(memberExpression, unaryExpression);
            }

            if (unaryExpression.Type == typeof(DateTime) || unaryExpression.Type == typeof(Nullable<DateTime>))
            {
                switch (filterValue.FilterType)
                {
                    case FilterType.Equal:
                        return Expression.Equal(memberExpression, unaryExpression);
                    case FilterType.NotEqual:
                        return Expression.NotEqual(memberExpression, unaryExpression);
                    case FilterType.GreaterThan:
                        return Expression.GreaterThan(memberExpression, unaryExpression);
                    case FilterType.GreaterThanOrEqual:
                        return Expression.GreaterThanOrEqual(memberExpression, unaryExpression);
                    case FilterType.LessThan:
                        return Expression.LessThan(memberExpression, unaryExpression);
                    case FilterType.LessThanOrEqual:
                        return Expression.LessThanOrEqual(memberExpression, unaryExpression);
                    default:
                        break;
                }
            }

            return Expression.Call(memberExpression, Contains, unaryExpression);
        }
    }
}
