namespace Folke.Orm.Fluent
{
    using System;
    using System.Linq.Expressions;

    public class FluentOrderByBuilder<T, TMe, TU> : FluentQueryableBuilder<T, TMe>
    {
        public FluentOrderByBuilder(BaseQueryBuilder queryBuilder, Expression<Func<T, TU>> expression)
            : base(queryBuilder)
        {
            queryBuilder.AppendOrderBy();
            queryBuilder.AppendColumn(expression.Body);
        }

        public FluentLimitBuilder<T, TMe> Limit(int offset, int count)
        {
            return new FluentLimitBuilder<T, TMe>(QueryBuilder, offset, count);
        }

        public FluentLimitBuilder<T, TMe> Limit(Expression<Func<T, int>> offset, int count)
        {
            return new FluentLimitBuilder<T, TMe>(QueryBuilder, offset, count);
        }

        public FluentLimitBuilder<T, TMe> Limit(Expression<Func<T, TMe, int>> offset, int count)
        {
            return new FluentLimitBuilder<T, TMe>(QueryBuilder, offset, count);
        }

        public FluentOrderByBuilder<T, TMe, TU> Desc()
        {
            QueryBuilder.Append("DESC");
            return this;
        }

        public FluentOrderByBuilder<T, TMe, TU> Asc()
        {
            QueryBuilder.Append("ASC");
            return this;
        }
    }
}
