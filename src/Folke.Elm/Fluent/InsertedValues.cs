namespace Folke.Elm.Fluent
{
    public interface IInsertedValuesTarget<T, TMe> : IFluentBuilder
    {
    }

    public static class InsertedValuesTargetExtensions
    {
        public static IInsertedValuesResult<T, TMe> Values<T, TMe>(this IInsertedValuesTarget<T, TMe> insertedValuesTarget, T value)
        {
            insertedValuesTarget.QueryBuilder.Append(" (");
            bool first = true;
            var type = value.GetType();
            var typeMapping = insertedValuesTarget.QueryBuilder.Mapper.GetTypeMapping(type);
            foreach (var property in typeMapping.Columns.Values)
            {
                if (/*TableHelpers.IsIgnored(property.PropertyType) || */ property.Readonly)
                    continue;
                if (first)
                    first = false;
                else
                    insertedValuesTarget.QueryBuilder.Append(",");
                insertedValuesTarget.QueryBuilder.AppendColumn(null, property);
            }

            insertedValuesTarget.QueryBuilder.Append(") VALUES (");
            first = true;
            foreach (var property in typeMapping.Columns.Values)
            {
                if (property.Readonly)
                    continue;
                if (first)
                    first = false;
                else
                    insertedValuesTarget.QueryBuilder.Append(",");
                insertedValuesTarget.QueryBuilder.AppendParameter(property.PropertyInfo.GetValue(value));
            }

            insertedValuesTarget.QueryBuilder.Append(")");
            return (IInsertedValuesResult<T, TMe>) insertedValuesTarget;
        }
    }

    public interface IInsertedValuesResult<T, TMe> : IQueryableCommand
    {
        
    }
}
