using System;
using System.Linq.Expressions;

namespace Folke.Elm.Fluent
{
    public interface IFromTarget<T, TMe>: IFluentBuilder
    {
    }

    public static class FromTargetExtensions
    {
        /// <summary>Chose the bean table as the table to select from</summary>
        /// <returns> The <see cref="IFromResult{T,TMe}"/>. </returns>
        public static IFromResult<T, TMe> From<T, TMe>(this IFromTarget<T, TMe> fromTarget)
        {
            fromTarget.AppendFrom();
            fromTarget.QueryBuilder.StringBuilder.AppendTable(fromTarget.QueryBuilder.RegisterRootTable());
            return (IFromResult<T, TMe>) fromTarget;
        }

        public static IFromResult<T, TMe> From<T, TMe, TU>(this IFromTarget<T, TMe> fromTarget, Expression<Func<T, TU>> expression)
        {
            fromTarget.AppendFrom();
            BaseQueryBuilder queryBuilder = fromTarget.QueryBuilder;
            var selectedTable = queryBuilder.GetTable(expression, register: true);
            queryBuilder.StringBuilder.AppendTable(selectedTable);
            return (IFromResult<T, TMe>)fromTarget;
        }

        public static IFromResult<T, TMe> From<T, TMe>(this IFromTarget<T, TMe> fromTarget, Action<ISelectResult<T, TMe>> subQuery)
        {
            fromTarget.AppendFrom();
            fromTarget.SubQuery(subQuery);
            fromTarget.QueryBuilder.StringBuilder.Append("AS");

            var table = fromTarget.QueryBuilder.RegisterRootTable();
            fromTarget.QueryBuilder.StringBuilder.Append(table.Alias);
            return (IFromResult<T, TMe>)fromTarget;
        }
        
        internal static void SubQuery<T,TMe>(this IFluentBuilder fluentBuilder, Action<ISelectResult<T, TMe>> subQuery)
        {
            var queryBuilder = new BaseQueryBuilder(fluentBuilder.QueryBuilder);
            var builder = FluentBaseBuilder<T, TMe>.Select(queryBuilder);
            subQuery(builder);
            BaseQueryBuilder tempQualifier = fluentBuilder.QueryBuilder;
            tempQualifier.StringBuilder.Append(" (");
            tempQualifier.StringBuilder.Append(queryBuilder.Sql);
            tempQualifier.StringBuilder.Append(')');
        }
    }

    public interface IFromResult<T, TMe> : IQueryableCommand<T>, IJoinTarget<T, TMe>, IWhereTarget<T, TMe>, IOrderByTarget<T, TMe>
    {
    }
}
