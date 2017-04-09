using System;
using System.Linq.Expressions;
using Folke.Elm.Visitor;

namespace Folke.Elm.Fluent
{
    public interface IOnTarget<T,TMe> : IFluentBuilder
    {
    }

    public static class OnTargetExtensions
    {
        public static IOnResult<T, TMe> On<T,TMe>(this IOnTarget<T, TMe> onTarget, Expression<Func<T, bool>> expression)
        {
            onTarget.QueryBuilder.StringBuilder.AppendAfterSpace("ON ");
            onTarget.QueryBuilder.AddBooleanExpression(expression.Body);
            return (IOnResult<T, TMe>) onTarget;
        }

        public static IOnResult<T, TMe> On<T, TMe>(this IOnTarget<T, TMe> onTarget, Expression<Func<T, TMe, bool>> expression)
        {
            onTarget.QueryBuilder.StringBuilder.AppendAfterSpace("ON ");
            onTarget.QueryBuilder.AddBooleanExpression(expression.Body);
            return (IOnResult<T, TMe>)onTarget;
        }

        public static IOnResult<T, TMe> OnId<T, TMe, TKey>(this IOnTarget<T, TMe> onTarget, Expression<Func<T, TKey>> expression)
        {
            onTarget.QueryBuilder.StringBuilder.AppendAfterSpace("ON ");
            var table = onTarget.QueryBuilder.GetTable(expression.Body, false);
            Field tableColumn = new Field(table.Parent, table.ParentMember);
            tableColumn.Accept(onTarget.QueryBuilder.StringBuilder);
            onTarget.QueryBuilder.StringBuilder.Append("=");
            Field tableColumn1 = new Field(table, table.Mapping.Key);
            tableColumn1.Accept(onTarget.QueryBuilder.StringBuilder);
            return (IOnResult<T, TMe>)onTarget;
        }
    }

    public interface IOnResult<T, TMe> : IFluentBuilder, IJoinTarget<T, TMe>, IAndOnTarget<T,TMe>, IWhereTarget<T, TMe>, IQueryableCommand<T>, ILimitTarget<T, TMe>, IOrderByTarget<T, TMe>
    {
        
    }
}
