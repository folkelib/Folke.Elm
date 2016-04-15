using System;
using System.Linq.Expressions;

namespace Folke.Elm.Fluent
{
    public interface IOrderByTarget<T, TMe> : IFluentBuilder
    {
    }

    public static class OrderByTargetExtensions
    {
        public static IOrderByResult<T, TMe> OrderBy<T, TMe, TV>(this IOrderByTarget<T, TMe> fluentBuilder, Expression<Func<T, TV>> expression)
        {
            BaseQueryBuilder tempQualifier = fluentBuilder.QueryBuilder;
            if (fluentBuilder.CurrentContext != QueryContext.OrderBy)
            {
                tempQualifier.StringBuilder.BeforeOrderBy();
                fluentBuilder.CurrentContext = QueryContext.OrderBy;
            }
            else
            {
                tempQualifier.StringBuilder.DuringOrderBy();
            }
            fluentBuilder.QueryBuilder.AddExpression(expression.Body);
            return (IOrderByResult<T, TMe>)fluentBuilder;
        }
    }

    public interface IOrderByResult<T, TMe> : ILimitTarget<T, TMe>, IAscTarget<T, TMe>, IQueryableCommand<T>
    {
    }
}
