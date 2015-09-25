using System;
using System.Linq.Expressions;

namespace Folke.Elm.Fluent
{
    public interface IAndOnTarget<T,TMe> : IFluentBuilder
    {
    }

    public static class AndOnTargetExtensions
    {
        public static IAndOnResult<T, TMe> AndOn<T,TMe>(this IAndOnTarget<T,TMe> andOnTarget, Expression<Func<T, bool>> expression)
        {
            andOnTarget.QueryBuilder.Append(" AND ");
            andOnTarget.QueryBuilder.AddExpression(expression.Body);
            return (IAndOnResult<T, TMe>) andOnTarget;
        }

        public static IAndOnResult<T, TMe> AndOn<T, TMe>(this IAndOnTarget<T, TMe> andOnTarget, Expression<Func<T, TMe, bool>> expression)
        {
            andOnTarget.QueryBuilder.Append(" AND ");
            andOnTarget.QueryBuilder.AddExpression(expression.Body);
            return (IAndOnResult<T, TMe>)andOnTarget;
        }
    }

    public interface IAndOnResult<T, TMe> : IFluentBuilder, IAndOnTarget<T,TMe>, IQueryableCommand<T>, ILimitTarget<T, TMe>, IWhereTarget<T, TMe>, IOrderByTarget<T, TMe>
    {
        
    }
}
