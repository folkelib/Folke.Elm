using Folke.Elm.Mapping;

namespace Folke.Elm.Fluent
{
    public interface IInsertedValuesTarget<T, TMe> : IFluentBuilder
    {
    }

    public static class InsertedValuesTargetExtensions
    {
        public static IInsertedValuesResult<T, TMe> Values<T, TMe>(this IInsertedValuesTarget<T, TMe> insertedValuesTarget, T value)
        {
            var baseQueryBuilder = insertedValuesTarget.QueryBuilder;
            baseQueryBuilder.StringBuilder.Append(" (");
            bool first = true;
            var type = value.GetType();
            var typeMapping = baseQueryBuilder.Mapper.GetTypeMapping(type);
            foreach (var property in typeMapping.Columns.Values)
            {
                if (/*TableHelpers.IsIgnored(property.PropertyType) || */ property.Readonly)
                    continue;
                if (first)
                    first = false;
                else
                    baseQueryBuilder.StringBuilder.Append(",");
                baseQueryBuilder.StringBuilder.DuringColumn(null, property.ColumnName);
            }

            baseQueryBuilder.StringBuilder.Append(") VALUES (");
            first = true;
            foreach (var property in typeMapping.Columns.Values)
            {
                if (property.Readonly)
                    continue;
                if (first)
                    first = false;
                else
                    baseQueryBuilder.StringBuilder.Append(",");
                var parameterIndex = baseQueryBuilder.AddParameter(property.PropertyInfo.GetValue(value));
                baseQueryBuilder.StringBuilder.DuringParameter(parameterIndex);
            }

            baseQueryBuilder.StringBuilder.Append(")");
            return (IInsertedValuesResult<T, TMe>) insertedValuesTarget;
        }
    }

    public interface IInsertedValuesResult<T, TMe> : IQueryableCommand
    {
        
    }
}
