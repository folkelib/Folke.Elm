namespace Folke.Orm.Fluent
{
    using System;

    public class FluentWhereSubQueryBuilder<T, TMe> : FluentQueryableBuilder<T, TMe>
    {
        public FluentWhereSubQueryBuilder(BaseQueryBuilder queryBuilder, Action<FluentSelectBuilder<T, TMe>> subQuery, SubQueryType type)
            : base(queryBuilder)
        {
            QueryBuilder.Where();
            switch (type)
            {
                case SubQueryType.Exists:
                    QueryBuilder.Append("EXISTS");
                    break;
                case SubQueryType.NotExists:
                    QueryBuilder.Append("NOT EXISTS");
                    break;
            }
            SubQuery(subQuery);
        }

        private void SubQuery(Action<FluentSelectBuilder<T, TMe>> subQuery)
        {
            var queryBuilder = new BaseQueryBuilder(QueryBuilder);
            var builder = new FluentSelectBuilder<T, TMe>(queryBuilder);
            subQuery(builder);
            QueryBuilder.Append(" (");
            QueryBuilder.Append(queryBuilder.Sql);
            QueryBuilder.Append(")");
        }
    }
}
