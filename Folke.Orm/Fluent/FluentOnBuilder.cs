namespace Folke.Orm.Fluent
{
    using System;
    using System.Linq.Expressions;

    public class FluentOnBuilder<T, TMe, TU> : FluentQueryableBuilder<T, TMe>
    {
        public FluentOnBuilder(BaseQueryBuilder queryBuilder, Expression<Func<T, bool>> expression)
            : base(queryBuilder)
        {
            queryBuilder.Append("ON ");
            queryBuilder.AddExpression(expression.Body);
        }

        public FluentOnBuilder(BaseQueryBuilder queryBuilder, Expression<Func<T, TU>> expression)
            : base(queryBuilder)
        {
            queryBuilder.Append("ON ");
            queryBuilder.AppendColumn(queryBuilder.ExpressionToColumn(expression.Body));
            queryBuilder.Append("=");
            queryBuilder.AppendColumn(queryBuilder.GetTableKey(expression.Body));
        }

        public FluentOnBuilder<T, TMe, TU> AndOn(Expression<Func<T, bool>> expression)
        {
            QueryBuilder.Append(" AND ");
            QueryBuilder.AddExpression(expression.Body);
            return this;
        }

        public FluentOrderByBuilder<T, TMe, TV> OrderBy<TV>(Expression<Func<T, TV>> expression)
        {
            return new FluentOrderByBuilder<T, TMe, TV>(QueryBuilder, expression);
        }
    }
}
