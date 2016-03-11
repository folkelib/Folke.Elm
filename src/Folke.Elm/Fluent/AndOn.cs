using System;
using System.Linq.Expressions;

namespace Folke.Elm.Fluent
{
    public interface IAndOnTarget<T, TMe> : IFluentBuilder
    {
    }

    /// <summary>A result of an AND statement in a ON clause.</summary>
    /// <typeparam name="T">The return type</typeparam>
    /// <typeparam name="TMe">The parameter type</typeparam>
    public interface IAndOnResult<T, TMe> : IFluentBuilder, IAndOnTarget<T, TMe>, IQueryableCommand<T>, ILimitTarget<T, TMe>, IWhereTarget<T, TMe>, IOrderByTarget<T, TMe>, IJoinTarget<T, TMe>
    {
    }

    public static class AndOnTargetExtensions
    {
        public static IAndOnResult<T, TMe> AndOn<T, TMe>(this IAndOnTarget<T, TMe> andOnTarget, Expression<Func<T, bool>> expression)
        {
            andOnTarget.QueryBuilder.Append(" AND ");
            andOnTarget.QueryBuilder.AddBooleanExpression(expression.Body);
            return (IAndOnResult<T, TMe>) andOnTarget;
        }

        public static IAndOnResult<T, TMe> AndOn<T, TMe>(this IAndOnTarget<T, TMe> andOnTarget, Expression<Func<T, TMe, bool>> expression)
        {
            andOnTarget.QueryBuilder.Append(" AND ");
            andOnTarget.QueryBuilder.AddBooleanExpression(expression.Body);
            return (IAndOnResult<T, TMe>)andOnTarget;
        }
    }
}
