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

        public FluentJoinBuilder<T, TMe, TV> InnerJoin<TV>(Expression<Func<T, TV>> tableExpression)
        {
            return new FluentJoinBuilder<T, TMe, TV>(QueryBuilder, tableExpression, JoinType.Inner);
        }

        public FluentWhereBuilder<T, TMe> Where(Expression<Func<T, bool>> expression)
        {
            return new FluentWhereBuilder<T, TMe>(QueryBuilder, expression);
        }

        public FluentOnBuilder<T, TMe, TU> LeftJoinOnId<TV>(Expression<Func<T, TV>> tableExpression)
        {
            var builder = new FluentJoinBuilder<T, TMe, TV>(QueryBuilder, tableExpression, JoinType.LeftOuter);
            builder.OnId(tableExpression);
            return this;
        }

        public FluentWhereSubQueryBuilder<T, TMe> WhereNotExists(Action<FluentSelectBuilder<T, TMe>> subQuery)
        {
            return new FluentWhereSubQueryBuilder<T, TMe>(QueryBuilder, subQuery, SubQueryType.NotExists);
        }
    }
}
