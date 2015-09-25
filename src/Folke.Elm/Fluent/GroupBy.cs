using System;
using System.Linq.Expressions;

namespace Folke.Elm.Fluent
{
    public interface IGroupByTarget<T, TMe> : IFluentBuilder
    {
        
    }

    public static class GroupByTargetExtensions
    {
        public static IGroupByResult<T, TMe> GroupBy<T, TMe, TU>(this IGroupByTarget<T, TMe> queryBuilder, Expression<Func<T, TU>> column)
        {
            queryBuilder.QueryBuilder.AppendGroupBy();
            queryBuilder.QueryBuilder.AppendColumn(column.Body);
            return (IGroupByResult<T, TMe>) queryBuilder;
        }
    }

    public interface IGroupByResult<out T, TMe> : IQueryableCommand<T>
    {
    }
}
