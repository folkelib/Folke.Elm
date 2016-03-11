using System;
using System.Linq.Expressions;

namespace Folke.Elm.Fluent
{
    public interface ISetTarget<T, TMe> : IFluentBuilder
    {
    }

    public static class SetTargetExtensions
    {
        public static ISetResult<T, TMe> Set<T, TMe, TU>(this ISetTarget<T, TMe> target, Expression<Func<T, TU>> column, Expression<Func<T, TU>> value)
        {
            target.QueryBuilder.AppendSet();

            target.QueryBuilder.AppendColumn(column.Body, registerTable: true);
            target.QueryBuilder.Append("=");
            target.QueryBuilder.AddExpression(value.Body);
            return (ISetResult<T, TMe>) target;
        }

        public static ISetResult<T, TMe> SetAll<T, TMe>(this ISetTarget<T, TMe> target, T value)
        {
            var table = target.QueryBuilder.DefaultTable;
            var typeMapping = table.Mapping;
            foreach (var property in typeMapping.Columns.Values)
            {
                if (property.Readonly)
                    continue;

                target.QueryBuilder.AppendSet();

                target.QueryBuilder.AppendColumn(table.Name, property);
                target.QueryBuilder.Append("=");
                target.QueryBuilder.AppendParameter(property.PropertyInfo.GetValue(value));
            }
            return (ISetResult<T, TMe>)target;
        }
    }

    public interface ISetResult<T, TMe> : ISetTarget<T, TMe>, IWhereTarget<T, TMe>, IBaseCommand
    {
    }
}
