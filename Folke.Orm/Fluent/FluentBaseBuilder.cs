namespace Folke.Orm.Fluent
{
    using System;

    public abstract class FluentBaseBuilder<T, TMe>
    {
        protected BaseQueryBuilder baseQueryBuilder;

        public BaseQueryBuilder QueryBuilder
        {
            get
            {
                return baseQueryBuilder;
            }
        }

        protected FluentBaseBuilder(BaseQueryBuilder queryBuilder)
        {
            baseQueryBuilder = queryBuilder;
        }

        protected void SubQuery(Action<FluentSelectBuilder<T, TMe>> subQuery)
        {
            var queryBuilder = new BaseQueryBuilder(this.QueryBuilder);
            var builder = new FluentSelectBuilder<T, TMe>(queryBuilder);
            subQuery(builder);
            this.QueryBuilder.AppendInParenthesis(queryBuilder.Sql);
        }
    }
}
