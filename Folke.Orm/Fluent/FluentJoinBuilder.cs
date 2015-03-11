namespace Folke.Orm.Fluent
{
    using System;
    using System.Linq.Expressions;

    public class FluentJoinBuilder<T, TMe, TU> : FluentBaseBuilder<T, TMe>
    {
        public FluentJoinBuilder(BaseQueryBuilder queryBuilder, Expression<Func<T, TU>> tableExpression, JoinType type) : base(queryBuilder)
        {
            this.AppendJoin(type);

            QueryBuilder.AppendTable(tableExpression.Body);
        }

        public FluentJoinBuilder(BaseQueryBuilder queryBuilder, Action<FluentSelectBuilder<T, TMe>> subQuery, Expression<Func<TU>> tableAlias, JoinType type) : base(queryBuilder)
        {
            this.AppendJoin(type);
            SubQuery(subQuery);
            QueryBuilder.Append("AS");
            var table = QueryBuilder.RegisterTable(typeof(TU), QueryBuilder.GetTableAlias(tableAlias.Body as MemberExpression));
            QueryBuilder.Append(table.name);
        }

        private void AppendJoin(JoinType type)
        {
            switch (type)
            {
                case JoinType.LeftOuter:
                    this.QueryBuilder.Append("LEFT JOIN");
                    break;
                case JoinType.RightOuter:
                    this.QueryBuilder.Append("RIGHT JOIN");
                    break;
                case JoinType.Inner:
                    this.QueryBuilder.Append("INNER JOIN");
                    break;
            }
        }

        public FluentOnBuilder<T, TMe, TU> On(Expression<Func<T, bool>> expression)
        {
            return new FluentOnBuilder<T, TMe, TU>(QueryBuilder, expression);
        }

        public FluentOnBuilder<T, TMe, TV> OnId<TV>(Expression<Func<T, TV>> expression)
        {
            return new FluentOnBuilder<T, TMe, TV>(QueryBuilder, expression);
        }
    }
}
