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
    }
}
