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
        /// <returns> The <see cref="FluentFromBuilder{T,TMe}"/>. </returns>
        public static IFromResult<T, TMe> From<T, TMe>(this IFromTarget<T, TMe> fromTarget)
        {
            fromTarget.QueryBuilder.AppendFrom();
            fromTarget.QueryBuilder.AppendTable(typeof(T), (string)null);
            return (IFromResult<T, TMe>) fromTarget;
        }

        public static IFromResult<T, TMe> From<T, TMe, TU>(this IFromTarget<T, TMe> fromTarget, Expression<Func<T, TU>> expression)
        {
            fromTarget.QueryBuilder.AppendFrom();
            fromTarget.QueryBuilder.AppendTable(expression.Body);
            return (IFromResult<T, TMe>)fromTarget;
        }

        public static IFromResult<T, TMe> From<T, TMe>(this IFromTarget<T, TMe> fromTarget, Action<ISelectResult<T, TMe>> subQuery)
        {
            fromTarget.QueryBuilder.AppendFrom();
            fromTarget.SubQuery(subQuery);
            fromTarget.QueryBuilder.Append("AS");

            var table = fromTarget.QueryBuilder.RegisterTable(typeof(T), null);
            fromTarget.QueryBuilder.Append(table.name);
            return (IFromResult<T, TMe>)fromTarget;
        }
        
        internal static void SubQuery<T,TMe>(this IFluentBuilder fluentBuilder, Action<ISelectResult<T, TMe>> subQuery)
        {
            var queryBuilder = new BaseQueryBuilder(fluentBuilder.QueryBuilder);
            var builder = FluentBaseBuilder<T, TMe>.Select(queryBuilder);
            subQuery(builder);
            fluentBuilder.QueryBuilder.AppendInParenthesis(queryBuilder.Sql);
        }
    }

    public interface IFromResult<T, TMe> : IFromTarget<T, TMe>, IQueryableCommand<T>, IJoinTarget<T, TMe>, IWhereTarget<T, TMe>, IOrderByTarget<T, TMe>
    {
    }
}
