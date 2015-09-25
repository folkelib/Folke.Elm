using System;
using System.Linq.Expressions;

namespace Folke.Elm.Fluent
{
    public interface IAndWhereTarget<T, TMe> : IFluentBuilder
    {
    }

    public static class AndWhereTargetExtensions
    {
        public static IAndWhereResult<T, TMe> Or<T,TMe>(this IAndWhereTarget<T, TMe> target, Expression<Func<T, bool>> expression)
        {
            target.QueryBuilder.AppendOr();
            target.QueryBuilder.AddExpression(expression.Body);
            return (IAndWhereResult<T, TMe>) target;
        }
    }

    public interface IAndWhereResult<T, TMe> : IWhereResult<T, TMe>
    {
        
    }
}
