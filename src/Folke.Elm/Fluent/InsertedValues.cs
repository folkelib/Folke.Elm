using Folke.Elm.Mapping;
using Newtonsoft.Json;

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
            var type = value.GetType();
            var typeMapping = baseQueryBuilder.Mapper.GetTypeMapping(type);
            AddValues(typeMapping, baseQueryBuilder, null);

            baseQueryBuilder.StringBuilder.Append(") VALUES (");
            AddParameters(value, typeMapping, baseQueryBuilder);

            baseQueryBuilder.StringBuilder.Append(")");
            return (IInsertedValuesResult<T, TMe>) insertedValuesTarget;
        }

        private static void AddValues(TypeMapping typeMapping, BaseQueryBuilder baseQueryBuilder, string baseName)
        {
            bool first = true;
            foreach (var property in typeMapping.Columns.Values)
            {
                if (property.Readonly)
                    continue;
                if (first)
                    first = false;
                else
                    baseQueryBuilder.StringBuilder.Append(",");
                if (property.Reference != null && property.Reference.IsComplexType)
                {
                    AddValues(property.Reference, baseQueryBuilder, property.ComposeName(baseName));
                }
                else
                {
                    baseQueryBuilder.StringBuilder.DuringColumn(null, property.ComposeName(baseName));
                }
            }
        }

        private static void AddParameters(object value, TypeMapping typeMapping, BaseQueryBuilder baseQueryBuilder)
        {
            bool first = true;
            foreach (var property in typeMapping.Columns.Values)
            {
                if (property.Readonly)
                    continue;
                if (first)
                    first = false;
                else
                    baseQueryBuilder.StringBuilder.Append(",");
                var propertyValue = value != null ? property.PropertyInfo.GetValue(value) : null;
                if (property.Reference != null && property.Reference.IsComplexType)
                {
                    AddParameters(propertyValue, property.Reference, baseQueryBuilder);
                }
                else
                {
                    if (property.IsJson) propertyValue = JsonConvert.SerializeObject(propertyValue);
                    var parameterIndex = baseQueryBuilder.AddParameter(propertyValue);
                    baseQueryBuilder.StringBuilder.DuringParameter(parameterIndex);
                }
            }
        }
    }

    public interface IInsertedValuesResult<T, TMe> : IQueryableCommand
    {
        
    }
}
