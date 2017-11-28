using System;
using System.Linq.Expressions;
using Folke.Elm.Mapping;
using Folke.Elm.Visitor;

namespace Folke.Elm.Fluent
{
    public interface ISetTarget<T, TMe> : IFluentBuilder
    {
    }

    public static class SetTargetExtensions
    {
        public static ISetResult<T, TMe> Set<T, TMe, TU>(this ISetTarget<T, TMe> target, Expression<Func<T, TU>> column, Expression<Func<T, TU>> value)
        {
            target.AppendSet();

            BaseQueryBuilder queryBuilder = target.QueryBuilder;
            var column1 = queryBuilder.ExpressionToColumn(column.Body, ParseOptions.RegisterTables);
            column1.Accept(queryBuilder.StringBuilder);
            queryBuilder.StringBuilder.Append("=");
            queryBuilder.AddExpression(value.Body);
            return (ISetResult<T, TMe>) target;
        }

        public static ISetResult<T, TMe> SetAll<T, TMe>(this ISetTarget<T, TMe> target, T value)
        {
            var baseQueryBuilder = target.QueryBuilder;
            var table = baseQueryBuilder.DefaultTable;
            var typeMapping = table.Mapping;
            AddParameters(target, value, typeMapping, table, baseQueryBuilder, null);
            return (ISetResult<T, TMe>)target;
        }

        private static void AddParameters<T, TMe>(ISetTarget<T, TMe> target, object value, TypeMapping typeMapping, SelectedTable table,
            BaseQueryBuilder baseQueryBuilder, string baseName)
        {
            foreach (var property in typeMapping.Columns.Values)
            {
                if (property.Readonly)
                    continue;

                var parameter = property.PropertyInfo.GetValue(value);
                if (property.Reference != null && property.Reference.IsComplexType)
                {
                    AddParameters(target, parameter, property.Reference, table, baseQueryBuilder, property.ComposeName(baseName));
                }
                else
                {
                    target.AppendSet();
                    string tableName = table.Alias;
                    baseQueryBuilder.StringBuilder.DuringColumn(tableName, property.ComposeName(baseName));
                    baseQueryBuilder.StringBuilder.Append("=");
                    var index = baseQueryBuilder.AddParameter(parameter);
                    baseQueryBuilder.StringBuilder.DuringParameter(index);
                }
            }
        }
    }

    public interface ISetResult<T, TMe> : ISetTarget<T, TMe>, IWhereTarget<T, TMe>, IBaseCommand
    {
    }
}
