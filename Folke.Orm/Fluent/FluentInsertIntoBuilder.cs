namespace Folke.Orm.Fluent
{
    public class FluentInsertIntoBuilder<T, TMe> : FluentQueryableBuilder<T, TMe>
    {
        public FluentInsertIntoBuilder(BaseQueryBuilder baseQueryBuilder) : base(baseQueryBuilder)
        {
            baseQueryBuilder.Append("INSERT INTO");
            baseQueryBuilder.AppendTableName(baseQueryBuilder.Mapper.GetTypeMapping(typeof(T)));
        }

        public FluentInsertIntoBuilder<T, TMe> Values(T value)
        {
            baseQueryBuilder.Append(" (");
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
                    baseQueryBuilder.Append(",");
                baseQueryBuilder.AppendColumn(null, property);
            }

            baseQueryBuilder.Append(") VALUES (");
            first = true;
            foreach (var property in typeMapping.Columns.Values)
            {
                if (property.Readonly)
                    continue;
                if (first)
                    first = false;
                else
                    baseQueryBuilder.Append(",");
                baseQueryBuilder.AppendParameter(property.PropertyInfo.GetValue(value));
            }

            baseQueryBuilder.Append(")");
            return this;
        }
    }
}
