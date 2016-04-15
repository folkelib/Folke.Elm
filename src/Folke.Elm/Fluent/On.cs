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
            BaseQueryBuilder.TableColumn tableColumn = onTarget.QueryBuilder.ExpressionToColumn(expression.Body);
            onTarget.QueryBuilder.StringBuilder.DuringColumn(tableColumn.Table.Alias, tableColumn.Column.ColumnName);
            onTarget.QueryBuilder.StringBuilder.Append("=");
            BaseQueryBuilder.TableColumn tableColumn1 = onTarget.QueryBuilder.GetTableKey(expression.Body);
            onTarget.QueryBuilder.StringBuilder.DuringColumn(tableColumn1.Table.Alias, tableColumn1.Column.ColumnName);
            return (IOnResult<T, TMe>)onTarget;
        }
    }

    public interface IOnResult<T, TMe> : IFluentBuilder, IJoinTarget<T, TMe>, IAndOnTarget<T,TMe>, IWhereTarget<T, TMe>, IQueryableCommand<T>, ILimitTarget<T, TMe>, IOrderByTarget<T, TMe>
    {
        
    }
}
