namespace Folke.Orm.Fluent
{
    using System;
    using System.Linq.Expressions;

    public class FluentJoinBuilder<T, TMe> : FluentBaseBuilder<T, TMe>
    {
        public FluentJoinBuilder(BaseQueryBuilder queryBuilder, Expression<Func<T, object>> tableExpression, JoinType type) : base(queryBuilder)
        {
            AppendJoin(type);

            QueryBuilder.AppendTable(tableExpression.Body);
        }

        public FluentJoinBuilder(BaseQueryBuilder queryBuilder, Action<FluentSelectBuilder<T, TMe>> subQuery, Expression<Func<object>> tableAlias, JoinType type) : base(queryBuilder)
        {
            AppendJoin(type);
            SubQuery(subQuery);
            QueryBuilder.Append("AS");
            var table = QueryBuilder.RegisterTable(tableAlias.Body.Type, QueryBuilder.GetTableAlias(tableAlias.Body as MemberExpression));
            QueryBuilder.Append(table.name);
        }

        private void AppendJoin(JoinType type)
        {
            switch (type)
            {
                case JoinType.LeftOuter:
                    QueryBuilder.Append("LEFT JOIN");
                    break;
                case JoinType.RightOuter:
                    QueryBuilder.Append("RIGHT JOIN");
                    break;
                case JoinType.Inner:
                    QueryBuilder.Append("INNER JOIN");
                    break;
            }
        }

        public FluentOnBuilder<T, TMe> On(Expression<Func<T, bool>> expression)
        {
            return new FluentOnBuilder<T, TMe>(QueryBuilder, expression);
        }

        public FluentOnBuilder<T, TMe> On(Expression<Func<T, TMe, bool>> expression)
        {
            return new FluentOnBuilder<T, TMe>(QueryBuilder, expression);
        }

        public FluentOnBuilder<T, TMe> OnId(Expression<Func<T, object>> expression)
        {
            return new FluentOnBuilder<T, TMe>(QueryBuilder, expression);
        }
    }
}
