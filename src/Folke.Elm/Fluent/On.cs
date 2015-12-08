using System;
using System.Linq.Expressions;

namespace Folke.Elm.Fluent
{
    public interface IOnTarget<T,TMe> : IFluentBuilder
    {
    }

    public static class OnTargetExtensions
    {
        public static IOnResult<T, TMe> On<T,TMe>(this IOnTarget<T, TMe> onTarget, Expression<Func<T, bool>> expression)
        {
            onTarget.QueryBuilder.Append("ON ");
            onTarget.QueryBuilder.AddBooleanExpression(expression.Body);
            return (IOnResult<T, TMe>) onTarget;
        }

        public static IOnResult<T, TMe> On<T, TMe>(this IOnTarget<T, TMe> onTarget, Expression<Func<T, TMe, bool>> expression)
        {
            onTarget.QueryBuilder.Append("ON ");
            onTarget.QueryBuilder.AddBooleanExpression(expression.Body);
            return (IOnResult<T, TMe>)onTarget;
        }

        public static IOnResult<T, TMe> OnId<T, TMe>(this IOnTarget<T, TMe> onTarget, Expression<Func<T, object>> expression)
        {
            onTarget.QueryBuilder.Append("ON ");
            onTarget.QueryBuilder.AppendColumn(onTarget.QueryBuilder.ExpressionToColumn(expression.Body));
            onTarget.QueryBuilder.Append("=");
            onTarget.QueryBuilder.AppendColumn(onTarget.QueryBuilder.GetTableKey(expression.Body));
            return (IOnResult<T, TMe>)onTarget;
        }
    }

    public interface IOnResult<T, TMe> : IFluentBuilder, IJoinTarget<T, TMe>, IAndOnTarget<T,TMe>, IWhereTarget<T, TMe>, IQueryableCommand<T>, ILimitTarget<T, TMe>
    {
        
    }
}
