namespace Folke.Orm.Fluent
{
    using System;
    using System.Linq.Expressions;

    public class FluentFromBuilder<T, TMe> : FluentQueryableBuilder<T, TMe>
    {
        public FluentFromBuilder(BaseQueryBuilder baseQueryBuilder)
            : base(baseQueryBuilder)
        {
            QueryBuilder.AppendFrom();
            QueryBuilder.AppendTable(typeof(T), (string)null);
        }

        public FluentFromBuilder(BaseQueryBuilder baseQueryBuilder, Expression<Func<T, object>> expression)
            : base(baseQueryBuilder)
        {
            QueryBuilder.AppendFrom();
            QueryBuilder.AppendTable(expression.Body);
        }

        public FluentFromBuilder(BaseQueryBuilder baseQueryBuilder, Action<FluentSelectBuilder<T, TMe>> subQuery)
            : base(baseQueryBuilder)
        {
            QueryBuilder.AppendFrom();
            SubQuery(subQuery);
            QueryBuilder.Append("AS");

            var table = QueryBuilder.RegisterTable(typeof(T), null);
            QueryBuilder.Append(table.name);
        }

        /// <summary> Chose the bean table as the table to select from </summary>
        /// <param name="tableAlias"> The table Alias. </param>
        /// <returns> The <see cref="FluentFromBuilder{T,TMe}"/>.  </returns>
        public FluentFromBuilder<T, TMe> From(Expression<Func<T, object>> tableAlias)
        {
            QueryBuilder.AppendFrom();
            QueryBuilder.AppendTable(tableAlias.Body);
            return this;
        }

        [Obsolete("Use From()")]
        public FluentFromBuilder<T, TMe> AndFrom(Expression<Func<T, object>> tableAlias)
        {
            return From(tableAlias);
        }

        public FluentJoinBuilder<T, TMe> LeftJoin(Expression<Func<T, object>> tableExpression)
        {
            return new FluentJoinBuilder<T, TMe>(QueryBuilder, tableExpression, JoinType.LeftOuter);
        }

        public FluentFromBuilder<T, TMe> LeftJoinOnId(Expression<Func<T, object>> tableExpression)
        {
            var builder = new FluentJoinBuilder<T, TMe>(QueryBuilder, tableExpression, JoinType.LeftOuter);
            builder.OnId(tableExpression);
            return this;
        }

        public FluentJoinBuilder<T, TMe> RightJoin(Expression<Func<T, object>> tableExpression)
        {
            return new FluentJoinBuilder<T, TMe>(QueryBuilder, tableExpression, JoinType.RightOuter);
        }

        public FluentJoinBuilder<T, TMe> InnerJoin(Expression<Func<T, object>> tableExpression)
        {
            return new FluentJoinBuilder<T, TMe>(QueryBuilder, tableExpression, JoinType.Inner);
        }

        public FluentJoinBuilder<T, TMe> InnerJoinSubQuery(Action<FluentSelectBuilder<T, TMe>> subqueryFactory, Expression<Func<object>> tableAlias)
        {
            return new FluentJoinBuilder<T, TMe>(QueryBuilder, subqueryFactory, tableAlias, JoinType.Inner);
        }

        public FluentWhereBuilder<T, TMe> Where(Expression<Func<T, bool>> expression)
        {
            return new FluentWhereBuilder<T, TMe>(QueryBuilder, expression);
        }

        public FluentWhereBuilder<T, TMe> Where(Expression<Func<T, TMe, bool>> expression)
        {
            return new FluentWhereBuilder<T, TMe>(QueryBuilder, expression);
        }

        public FluentWhereSubQueryBuilder<T, TMe> WhereExists(Action<FluentSelectBuilder<T, TMe>> subQuery)
        {
            return new FluentWhereSubQueryBuilder<T, TMe>(QueryBuilder, subQuery, SubQueryType.Exists);
        }

        public FluentWhereSubQueryBuilder<T, TMe> WhereNotExists(Action<FluentSelectBuilder<T, TMe>> subQuery)
        {
            return new FluentWhereSubQueryBuilder<T, TMe>(QueryBuilder, subQuery, SubQueryType.NotExists);
        }

        public FluentOrderByBuilder<T, TMe> OrderBy(Expression<Func<T, object>> column)
        {
            return new FluentOrderByBuilder<T, TMe>(QueryBuilder, column);
        }
    }
}
