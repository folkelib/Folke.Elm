namespace Folke.Orm.Fluent
{
    using System;
    using System.Linq.Expressions;

    /// <summary> The fluent select builder. </summary>
    /// <typeparam name="T">The bean type </typeparam>
    /// <typeparam name="TMe">The parameter type</typeparam>
    public class FluentSelectBuilder<T, TMe> : FluentQueryableBuilder<T, TMe>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentSelectBuilder{T,TMe}"/> class.
        /// </summary>
        /// <param name="baseQueryBuilder">
        /// The base query builder.
        /// </param>
        public FluentSelectBuilder(BaseQueryBuilder baseQueryBuilder)
            : base(baseQueryBuilder)
        {
        }

        public FluentSelectBuilder(IDatabaseDriver driver)
            : base(new BaseQueryBuilder(driver, typeof(T), typeof(TMe)))
        {
        }

        /// <summary>Select several values </summary>
        /// <param name="columns">The expression that returns a value </param> 
        /// <returns> The <see cref="FluentSelectBuilder{T,TMe}"/>. </returns>
        public FluentSelectBuilder<T, TMe> Values(params Expression<Func<T, object>>[] columns)
        {
            foreach (var column in columns)
            {
                QueryBuilder.AppendSelect();
                var tableColumn = QueryBuilder.ExpressionToColumn(column.Body, true);
                if (tableColumn == null)
                {
                    QueryBuilder.AddExpression(column.Body);
                }
                else
                {
                    QueryBuilder.AppendSelectedColumn(tableColumn);
                }
            }

            return this;
        }

        [Obsolete("Use Values")]
        public FluentSelectBuilder<T, TMe> Select(params Expression<Func<T, object>>[] columns)
        {
            return this.Values(columns);
        }

        /// <summary>Select all the columns of a given table </summary>
        /// <param name="tableExpression">The expression that returns a table </param>
        /// <typeparam name="TU">The table type</typeparam>
        /// <returns> The <see cref="FluentSelectBuilder{T,TMe}"/>. </returns>
        public FluentSelectBuilder<T, TMe> All<TU>(Expression<Func<T, TU>> tableExpression)
        {
            QueryBuilder.AppendSelect();
            Expression expressionBody = tableExpression.Body;
            Type tableType = expressionBody.Type;
            QueryBuilder.AppendAllSelects(tableType, QueryBuilder.GetTableAlias(expressionBody));
            return this;
        }

        /// <summary> Select all the field of the bean table </summary>
        /// <returns>The query builder</returns>
        public FluentSelectBuilder<T, TMe> All()
        {
            QueryBuilder.AppendSelect();
            QueryBuilder.AppendAllSelects(typeof(T), null);
            return this;
        }

        [Obsolete("Use All()")]
        public FluentSelectBuilder<T, TMe> SelectAll()
        {
            return this.All();
        }

        [Obsolete("Use All()")]
        public FluentSelectBuilder<T, TMe> SelectAll<TU>(Expression<Func<T, TU>> tableExpression)
        {
            return this.All(tableExpression);
        }

        [Obsolete("Use All()")]
        public FluentSelectBuilder<T, TMe> AndAll()
        {
            return this.All();
        }

        [Obsolete("Use All()")]
        public FluentSelectBuilder<T, TMe> AndAll<TU>(Expression<Func<T, TU>> tableExpression)
        {
            return this.All(tableExpression);
        }

        /// <summary>Chose the bean table as the table to select from</summary>
        /// <returns> The <see cref="FluentFromBuilder{T,TMe,TU}"/>. </returns>
        public FluentFromBuilder<T, TMe, T> From()
        {
            return new FluentFromBuilder<T, TMe, T>(QueryBuilder);
        }

        public FluentFromBuilder<T, TMe, TU> From<TU>(Expression<Func<T, TU>> tableAlias)
        {
            return new FluentFromBuilder<T, TMe, TU>(QueryBuilder, tableAlias);
        }

        [Obsolete("Use From()")]
        public FluentFromBuilder<T, TMe, TU> AndFrom<TU>(Expression<Func<T, TU>> tableAlias)
        {
            return this.From(tableAlias);
        }

        public FluentFromBuilder<T, TMe, T> FromSubQuery(Action<FluentSelectBuilder<T, TMe>> subQuery)
        {
            return new FluentFromBuilder<T, TMe, T>(QueryBuilder, subQuery);
        }

        [Obsolete("Use CountAll()")]
        public FluentSelectBuilder<T, TMe> SelectCountAll()
        {
            return this.CountAll();
        }

        public FluentSelectBuilder<T, TMe> CountAll()
        {
            QueryBuilder.AppendSelect();
            QueryBuilder.Append(" COUNT(*)");
            return this;
        }
    }
}
