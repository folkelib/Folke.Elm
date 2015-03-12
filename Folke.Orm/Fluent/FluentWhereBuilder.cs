namespace Folke.Orm.Fluent
{
    using System;
    using System.Linq.Expressions;

    public class FluentWhereBuilder<T, TMe> : FluentQueryableBuilder<T, TMe>
    {
        public FluentWhereBuilder(BaseQueryBuilder queryBuilder, Expression<Func<T, bool>> expression)
            : base(queryBuilder)
        {
            queryBuilder.Where();
            queryBuilder.AddExpression(expression.Body);
        }

        public FluentWhereBuilder(BaseQueryBuilder queryBuilder, Expression<Func<T, TMe, bool>> expression)
            : base(queryBuilder)
        {
            queryBuilder.Where();
            queryBuilder.AddExpression(expression.Body);
        }

        public FluentGroupByBuilder<T, TMe, TU> GroupBy<TU>(Expression<Func<T, TU>> column)
        {
            return new FluentGroupByBuilder<T, TMe, TU>(QueryBuilder, column);
        }

        public FluentOrderByBuilder<T, TMe, TV> OrderBy<TV>(Expression<Func<T, TV>> column)
        {
            return new FluentOrderByBuilder<T, TMe, TV>(QueryBuilder, column);
        }

        public FluentLimitBuilder<T, TMe> Limit(int offset, int count)
        {
            return new FluentLimitBuilder<T, TMe>(QueryBuilder, offset, count);
        }

        public FluentWhereBuilder<T, TMe> Where(Expression<Func<T, bool>> expression)
        {
            return new FluentWhereBuilder<T, TMe>(QueryBuilder, expression);
        }
    }
}
