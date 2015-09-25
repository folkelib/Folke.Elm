using System;
using System.Linq.Expressions;

namespace Folke.Elm.Fluent
{
    public interface ISelectedValuesTarget<T, TMe> : IFluentBuilder
    {
    }

    public static class SelectedValuesTargetExtensions
    {
        /// <summary>Select several values </summary>
        /// <param name="target"></param>
        /// <param name="columns">The expression that returns a value </param> 
        /// <returns> The <see cref="FluentSelectBuilder{T,TMe}"/>. </returns>
        public static ISelectedValuesResult<T, TMe> Values<T, TMe>(this ISelectedValuesTarget<T, TMe> target, params Expression<Func<T, object>>[] columns)
        {
            foreach (var column in columns)
            {
                target.QueryBuilder.AppendSelect();
                var tableColumn = target.QueryBuilder.ExpressionToColumn(column.Body, true);
                if (tableColumn == null)
                {
                    target.QueryBuilder.AddExpression(column.Body, true);
                }
                else
                {
                    target.QueryBuilder.AppendSelectedColumn(tableColumn);
                }
            }

            return (ISelectedValuesResult<T, TMe>)target;
        }

        /// <summary>Select all the columns of a given table </summary>
        /// <param name="target"></param>
        /// <param name="tableExpression">The expression that returns a table </param>
        /// <typeparam name="TU">The table type</typeparam>
        /// <typeparam name="TMe">The parameter type</typeparam>
        /// <typeparam name="T">The main table type</typeparam>
        /// <returns> The <see cref="FluentSelectBuilder{T,TMe}"/>. </returns>
        public static ISelectedValuesResult<T, TMe> All<T, TMe, TU>(this ISelectedValuesTarget<T, TMe> target, Expression<Func<T, TU>> tableExpression)
        {
            target.QueryBuilder.AppendSelect();
            Expression expressionBody = tableExpression.Body;
            Type tableType = expressionBody.Type;
            target.QueryBuilder.AppendAllSelects(tableType, target.QueryBuilder.GetTableAlias(expressionBody));
            return (ISelectedValuesResult<T, TMe>)target;
        }

        /// <summary> Select all the field of the bean table </summary>
        /// <returns>The query builder</returns>
        public static ISelectedValuesResult<T, TMe> All<T, TMe>(this ISelectedValuesTarget<T, TMe> target)
        {
            target.QueryBuilder.AppendSelect();
            target.QueryBuilder.AppendAllSelects(typeof(T), null);
            return (ISelectedValuesResult<T, TMe>)target;
        }

        public static ISelectedValuesResult<T, TMe> CountAll<T, TMe>(this ISelectedValuesTarget<T, TMe> target)
        {
            target.QueryBuilder.AppendSelect();
            target.QueryBuilder.Append(" COUNT(*)");
            return (ISelectedValuesResult<T, TMe>)target;
        }

        public static ISelectedValuesResult<T, TMe> Count<T, TMe, TU>(this ISelectedValuesTarget<T, TMe> target, Expression<Func<T, TU>> valueExpression)
        {
            target.QueryBuilder.AppendSelect();
            target.QueryBuilder.Append("COUNT(");
            target.QueryBuilder.AddExpression(valueExpression.Body, true);
            target.QueryBuilder.Append(")");
            return (ISelectedValuesResult<T, TMe>)target;
        }

        public static ISelectedValuesResult<T, TMe> Sum<T, TMe, TU>(this ISelectedValuesTarget<T, TMe> target, Expression<Func<T, TU>> valueExpression)
        {
            target.QueryBuilder.AppendSelect();
            target.QueryBuilder.Append("SUM(");
            target.QueryBuilder.AddExpression(valueExpression.Body, true);
            target.QueryBuilder.Append(")");
            return (ISelectedValuesResult<T, TMe>)target;
        }

        public static ISelectedValuesResult<T, TMe> Max<T, TMe, TU>(this ISelectedValuesTarget<T, TMe> target, Expression<Func<T, TU>> valueExpression)
        {
            target.QueryBuilder.AppendSelect();
            target.QueryBuilder.Append("MAX(");
            target.QueryBuilder.AddExpression(valueExpression.Body, true);
            target.QueryBuilder.Append(")");
            return (ISelectedValuesResult<T, TMe>)target;
        }

        public static ISelectedValuesResult<T, TMe> Value<T, TMe, TU>(this ISelectedValuesTarget<T, TMe> target, Expression<Func<T, TU>> valueExpression,
           Expression<Func<T, TU>> targetExpression)
        {
            target.QueryBuilder.AppendSelect();
            target.QueryBuilder.AddExpression(valueExpression.Body);
            target.QueryBuilder.SelectField(targetExpression.Body);
            return (ISelectedValuesResult<T, TMe>)target;
        }

        public static ISelectedValuesResult<T, TMe> Value<T, TMe, TU>(this ISelectedValuesTarget<T, TMe> target, Expression<Func<T, TU>> column)
        {
            target.QueryBuilder.AppendSelect();
            var tableColumn = target.QueryBuilder.ExpressionToColumn(column.Body, true);
            if (tableColumn == null)
            {
                target.QueryBuilder.AddExpression(column.Body, true);
            }
            else
            {
                target.QueryBuilder.AppendSelectedColumn(tableColumn);
            }
            return (ISelectedValuesResult<T, TMe>)target;
        }
    }

    public interface ISelectedValuesResult<T, TMe> : ISelectedValuesTarget<T, TMe>, IFromTarget<T, TMe>, IQueryableCommand<T>
    {
    }
}
