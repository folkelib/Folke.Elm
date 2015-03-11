namespace Folke.Orm.Fluent
{
    using System;
    using System.Linq.Expressions;

    public class FluentFromBuilder<T, TMe, TU> : FluentQueryableBuilder<T, TMe>
    {
        public FluentFromBuilder(BaseQueryBuilder baseQueryBuilder)
            : base(baseQueryBuilder)
        {
            QueryBuilder.AppendFrom();
            QueryBuilder.AppendTable(typeof(T), (string)null);
        }

        public FluentFromBuilder(BaseQueryBuilder baseQueryBuilder, Expression<Func<T, TU>> expression)
            : base(baseQueryBuilder)
        {
            QueryBuilder.AppendFrom();
            QueryBuilder.AppendTable(expression.Body);
        }

        public FluentFromBuilder(BaseQueryBuilder baseQueryBuilder, Action<FluentSelectBuilder<T, TMe>> subQuery)
            : base(baseQueryBuilder)
        {
            QueryBuilder.AppendFrom();
            this.SubQuery(subQuery);
            QueryBuilder.Append("AS");

            var table = QueryBuilder.RegisterTable(typeof(T), null);
            QueryBuilder.Append(table.name);
        }

        /// <summary> Chose the bean table as the table to select from </summary>
        /// <param name="tableAlias"> The table Alias. </param>
        /// <typeparam name="TV">The from table type</typeparam>
        /// <returns> The <see cref="FluentFromBuilder{T,TMe,TU}"/>.  </returns>
        public FluentFromBuilder<T, TMe, TU> From<TV>(Expression<Func<T, TV>> tableAlias)
        {
            QueryBuilder.AppendFrom();
            QueryBuilder.AppendTable(tableAlias.Body);
            return this;
        }

        [Obsolete("Use From()")]
        public FluentFromBuilder<T, TMe, TU> AndFrom<TV>(Expression<Func<T, TV>> tableAlias)
        {
            return From(tableAlias);
        }

        public FluentJoinBuilder<T, TMe, TV> LeftJoin<TV>(Expression<Func<T, TV>> tableExpression)
        {
            return new FluentJoinBuilder<T, TMe, TV>(QueryBuilder, tableExpression, JoinType.LeftOuter);
        }

        public FluentFromBuilder<T, TMe, TU> LeftJoinOnId<TV>(Expression<Func<T, TV>> tableExpression)
        {
            var builder = new FluentJoinBuilder<T, TMe, TV>(QueryBuilder, tableExpression, JoinType.LeftOuter);
            builder.OnId(tableExpression);
            return this;
        }

        public FluentJoinBuilder<T, TMe, TV> RightJoin<TV>(Expression<Func<T, TV>> tableExpression)
        {
            return new FluentJoinBuilder<T, TMe, TV>(QueryBuilder, tableExpression, JoinType.RightOuter);
        }

        public FluentJoinBuilder<T, TMe, TV> InnerJoin<TV>(Expression<Func<T, TV>> tableExpression)
        {
            return new FluentJoinBuilder<T, TMe, TV>(QueryBuilder, tableExpression, JoinType.Inner);
        }

        public FluentJoinBuilder<T, TMe, TV> InnerJoinSubQuery<TV>(Action<FluentSelectBuilder<T, TMe>> subqueryFactory, Expression<Func<TV>> tableAlias)
        {
            return new FluentJoinBuilder<T, TMe, TV>(QueryBuilder, subqueryFactory, tableAlias, JoinType.Inner);
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

        public FluentOrderByBuilder<T, TMe, TV> OrderBy<TV>(Expression<Func<T, TV>> column)
        {
            return new FluentOrderByBuilder<T, TMe, TV>(QueryBuilder, column);
        }
    }
}
