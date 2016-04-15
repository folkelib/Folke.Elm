using System;
using System.Linq.Expressions;
using Folke.Elm.Mapping;

namespace Folke.Elm.Fluent
{
    public interface ISetTarget<T, TMe> : IFluentBuilder
    {
    }

    public static class SetTargetExtensions
    {
        public static ISetResult<T, TMe> Set<T, TMe, TU>(this ISetTarget<T, TMe> target, Expression<Func<T, TU>> column, Expression<Func<T, TU>> value)
        {
            target.AppendSet();

            BaseQueryBuilder queryBuilder = target.QueryBuilder;
            var column1 = queryBuilder.ExpressionToColumn(column.Body, true);
            queryBuilder.StringBuilder.DuringColumn(column1.Table.Alias, column1.Column.ColumnName);
            queryBuilder.StringBuilder.Append("=");
            queryBuilder.AddExpression(value.Body);
            return (ISetResult<T, TMe>) target;
        }

        public static ISetResult<T, TMe> SetAll<T, TMe>(this ISetTarget<T, TMe> target, T value)
        {
            var baseQueryBuilder = target.QueryBuilder;
            var table = baseQueryBuilder.DefaultTable;
            var typeMapping = table.Mapping;
            foreach (var property in typeMapping.Columns.Values)
            {
                if (property.Readonly)
                    continue;

                target.AppendSet();

                string tableName = table.Alias;
                baseQueryBuilder.StringBuilder.DuringColumn(tableName, property.ColumnName);
                baseQueryBuilder.StringBuilder.Append("=");
                var index = baseQueryBuilder.AddParameter(property.PropertyInfo.GetValue(value));
                baseQueryBuilder.StringBuilder.DuringParameter(index);
            }
            return (ISetResult<T, TMe>)target;
        }
    }

    public interface ISetResult<T, TMe> : ISetTarget<T, TMe>, IWhereTarget<T, TMe>, IBaseCommand
    {
    }
}
