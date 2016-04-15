using System;
using System.Linq.Expressions;
using Folke.Elm.Visitor;

namespace Folke.Elm.Fluent
{
    public interface IAndWhereTarget<T, TMe> : IFluentBuilder
    {
    }

    public interface IAndWhereResult<T, TMe> : IWhereResult<T, TMe>
    {
    }

    public static class AndWhereTargetExtensions
    {
        public static IAndWhereResult<T, TMe> Or<T, TMe>(this IAndWhereTarget<T, TMe> target, Expression<Func<T, bool>> expression)
        {
            BaseQueryBuilder tempQualifier = target.QueryBuilder;
            if (target.CurrentContext != QueryContext.WhereExpression)
            {
                target.CurrentContext = QueryContext.WhereExpression;
            }
            else
            {
                tempQualifier.StringBuilder.DuringBinaryOperator(BinaryOperatorType.OrElse);
            }
            target.QueryBuilder.AddBooleanExpression(expression.Body);
            return (IAndWhereResult<T, TMe>)target;
        }
    }
}
