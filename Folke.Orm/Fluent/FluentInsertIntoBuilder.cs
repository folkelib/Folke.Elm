namespace Folke.Orm.Fluent
{
    public class FluentInsertIntoBuilder<T, TMe> : FluentQueryableBuilder<T, TMe>
    {
        public FluentInsertIntoBuilder(BaseQueryBuilder baseQueryBuilder) : base(baseQueryBuilder)
        {
            baseQueryBuilder.Append("INSERT INTO");
            baseQueryBuilder.AppendTableName(typeof(T));
        }

        public FluentInsertIntoBuilder<T, TMe> Values(T value)
        {
            baseQueryBuilder.Append(" (");
            bool first = true;
            var type = value.GetType();
            foreach (var property in type.GetProperties())
            {
                if (TableHelpers.IsIgnored(property.PropertyType) || TableHelpers.IsReadOnly(property))
                    continue;
                if (first)
                    first = false;
                else
                    baseQueryBuilder.Append(",");
                baseQueryBuilder.AppendColumn(null, property);
            }

            baseQueryBuilder.Append(") VALUES (");
            first = true;
            foreach (var property in type.GetProperties())
            {
                if (TableHelpers.IsIgnored(property.PropertyType) || TableHelpers.IsReadOnly(property))
                    continue;
                if (first)
                    first = false;
                else
                    baseQueryBuilder.Append(",");
                baseQueryBuilder.AppendParameter(property.GetValue(value));
            }

            baseQueryBuilder.Append(")");
            return this;
        }
    }
}
