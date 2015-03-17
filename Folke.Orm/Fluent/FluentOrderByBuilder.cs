namespace Folke.Orm.Fluent
{
    using System;
    using System.Linq.Expressions;

    public class FluentOrderByBuilder<T, TMe> : FluentQueryableBuilder<T, TMe>
    {
        public FluentOrderByBuilder(BaseQueryBuilder queryBuilder, Expression<Func<T, object>> expression)
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

        public FluentOrderByBuilder<T, TMe> Desc()
        {
            QueryBuilder.Append("DESC");
            return this;
        }

        public FluentOrderByBuilder<T, TMe> Asc()
        {
            QueryBuilder.Append("ASC");
            return this;
        }

        public FluentOrderByBuilder<T, TMe> OrderBy<TV>(Expression<Func<T, TV>> column)
        {
            QueryBuilder.AppendOrderBy();
            QueryBuilder.AppendColumn(column.Body);
            return this;
        }
    }
}
