using System;
using System.Linq.Expressions;

namespace Folke.Elm.Fluent
{
    public interface IGroupByTarget<T, TMe> : IFluentBuilder
    {
        
    }

    public static class GroupByTargetExtensions
    {
        public static IGroupByResult<T, TMe> GroupBy<T, TMe, TU>(this IGroupByTarget<T, TMe> builder, Expression<Func<T, TU>> column)
        {
            BaseQueryBuilder queryBuilder = builder.QueryBuilder;
            if (builder.CurrentContext != QueryContext.GroupBy)
            {
                queryBuilder.StringBuilder.BeforeGroupBy();
                builder.CurrentContext = QueryContext.GroupBy;
            }
            else
            {
                queryBuilder.StringBuilder.DuringGroupBy();
            }
            var column1 = queryBuilder.ExpressionToColumn(column.Body, false);
            queryBuilder.StringBuilder.DuringColumn(column1.Table.Alias, column1.Property.ColumnName);
            return (IGroupByResult<T, TMe>) builder;
        }
    }

    public interface IGroupByResult<out T, TMe> : IQueryableCommand<T>
    {
    }
}
