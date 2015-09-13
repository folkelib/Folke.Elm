namespace Folke.Orm.Fluent
{
    using System;
    using System.Linq.Expressions;

    public class FluentUpdateBuilder<T, TMe> : FluentQueryableBuilder<T, TMe>
    {
        public FluentUpdateBuilder(BaseQueryBuilder queryBuilder)
            : base(queryBuilder)
        {
            queryBuilder.Append("UPDATE ");
            queryBuilder.AppendTable(typeof(T), (string)null);
        }

        public FluentUpdateBuilder<T, TMe> Set<TU>(Expression<Func<T, TU>> column, Expression<Func<T, TU>> value)
        {
            QueryBuilder.AppendSet();

            QueryBuilder.AppendColumn(column.Body, registerTable: true);
            QueryBuilder.Append("=");
            QueryBuilder.AddExpression(value.Body);
            return this;
        }

        public FluentUpdateBuilder<T, TMe> SetAll(T value)
        {
            var table = QueryBuilder.DefaultTable;
            var typeMapping = table.Mapping;
            foreach (var property in typeMapping.Columns.Values)
            {
                if (property.Readonly)
                    continue;

                QueryBuilder.AppendSet();
            
                QueryBuilder.AppendColumn(table.name, property);
                QueryBuilder.Append("=");
                QueryBuilder.AppendParameter(property.PropertyInfo.GetValue(value));
            }

            return this;
        }

        public FluentWhereBuilder<T, TMe> Where(Expression<Func<T, bool>> expression)
        {
            return new FluentWhereBuilder<T, TMe>(QueryBuilder, expression);
        }

    }
}
