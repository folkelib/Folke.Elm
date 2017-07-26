using System;
using System.Linq.Expressions;
using Folke.Elm.Visitor;

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
        /// <returns> The <see cref="ISelectedValuesResult{T,TMe}"/>. </returns>
        public static ISelectedValuesResult<T, TMe> Values<T, TMe>(this ISelectedValuesTarget<T, TMe> target, params Expression<Func<T, object>>[] columns)
        {
            foreach (var column in columns)
            {
                AppendValue(target, column);
            }

            return (ISelectedValuesResult<T, TMe>)target;
        }

        /// <summary>Select all the columns of a given table </summary>
        /// <param name="target"></param>
        /// <param name="tableExpression">The expression that returns a table </param>
        /// <typeparam name="TU">The table type</typeparam>
        /// <typeparam name="TMe">The parameter type</typeparam>
        /// <typeparam name="T">The main table type</typeparam>
        /// <returns> The <see cref="ISelectedValuesResult{T,TMe}"/>. </returns>
        public static ISelectedValuesResult<T, TMe> All<T, TMe, TU>(this ISelectedValuesTarget<T, TMe> target, Expression<Func<T, TU>> tableExpression)
        {
            target.AppendSelect();
            var table = target.QueryBuilder.GetTable(tableExpression, true);
            target.QueryBuilder.AppendAllSelects(table);
            return (ISelectedValuesResult<T, TMe>)target;
        }

        /// <summary> Select all the field of the bean table </summary>
        /// <returns>The query builder</returns>
        public static ISelectedValuesResult<T, TMe> All<T, TMe>(this ISelectedValuesTarget<T, TMe> target)
        {
            target.AppendSelect();
            target.QueryBuilder.AppendAllSelects(target.QueryBuilder.RegisterRootTable());
            return (ISelectedValuesResult<T, TMe>)target;
        }

        public static ISelectedValuesResult<T, TMe> CountAll<T, TMe>(this ISelectedValuesTarget<T, TMe> target)
        {
            target.AppendSelect();
            target.QueryBuilder.StringBuilder.AppendAfterSpace("COUNT(*)");
            return (ISelectedValuesResult<T, TMe>)target;
        }

        public static ISelectedValuesResult<T, TMe> Count<T, TMe, TU>(this ISelectedValuesTarget<T, TMe> target, Expression<Func<T, TU>> valueExpression)
        {
            target.AppendSelect();
            target.QueryBuilder.StringBuilder.AppendAfterSpace("COUNT(");
            target.QueryBuilder.AddExpression(valueExpression.Body, ParseOptions.RegisterTables);
            target.QueryBuilder.StringBuilder.Append(")");
            return (ISelectedValuesResult<T, TMe>)target;
        }

        public static ISelectedValuesResult<T, TMe> Sum<T, TMe, TU>(this ISelectedValuesTarget<T, TMe> target, Expression<Func<T, TU>> valueExpression)
        {
            target.AppendSelect();
            target.QueryBuilder.StringBuilder.AppendAfterSpace("SUM(");
            target.QueryBuilder.AddExpression(valueExpression.Body, ParseOptions.RegisterTables);
            target.QueryBuilder.StringBuilder.Append(")");
            return (ISelectedValuesResult<T, TMe>)target;
        }

        public static ISelectedValuesResult<T, TMe> Max<T, TMe, TU>(this ISelectedValuesTarget<T, TMe> target, Expression<Func<T, TU>> valueExpression)
        {
            target.AppendSelect();
            target.QueryBuilder.StringBuilder.AppendAfterSpace("MAX(");
            target.QueryBuilder.AddExpression(valueExpression.Body, ParseOptions.RegisterTables);
            target.QueryBuilder.StringBuilder.Append(")");
            return (ISelectedValuesResult<T, TMe>)target;
        }

        public static ISelectedValuesResult<T, TMe> Value<T, TMe, TU>(this ISelectedValuesTarget<T, TMe> target, Expression<Func<T, TU>> valueExpression,
           Expression<Func<T, TU>> targetExpression)
        {
            target.AppendSelect();
            target.QueryBuilder.AddExpression(valueExpression.Body);
            target.QueryBuilder.SelectField(targetExpression.Body);
            return (ISelectedValuesResult<T, TMe>)target;
        }

        public static ISelectedValuesResult<T, TMe> Value<T, TMe, TU>(this ISelectedValuesTarget<T, TMe> target, Expression<Func<T, TU>> column)
        {
            AppendValue(target, column);
            return (ISelectedValuesResult<T, TMe>)target;
        }

        private static void AppendValue<T, TMe, TU>(ISelectedValuesTarget<T, TMe> target, Expression<Func<T, TU>> column)
        {
            target.AppendSelect();
            var tableColumn = target.QueryBuilder.ExpressionToColumn(column.Body, ParseOptions.RegisterTables);
            if (tableColumn == null)
            {
                target.QueryBuilder.AddExpression(column.Body, ParseOptions.RegisterTables);
            }
            else
            {
                if (tableColumn is Field)
                {
                    var field = target.QueryBuilder.SelectField((Field) tableColumn);
                    field.Accept(target.QueryBuilder.StringBuilder);
                }
                else
                {
                    target.QueryBuilder.AppendAllSelects((SelectedTable) tableColumn);
                }
            }
        }
    }

    public interface ISelectedValuesResult<T, TMe> : ISelectedValuesTarget<T, TMe>, IFromTarget<T, TMe>, IQueryableCommand<T>
    {
    }
}
