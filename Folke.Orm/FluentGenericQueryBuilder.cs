using System;
using System.Linq.Expressions;

namespace Folke.Orm
{
    public class FluentGenericQueryBuilder<T> : FluentGenericQueryBuilder<T, FolkeTuple>
    where T : class, new()
    {
        public FluentGenericQueryBuilder(FolkeConnection connection)
            : base(connection)
        {
        }
    }

    /// <summary>
    /// Used to create a SQL query and create objects with its results
    /// TODO cut in multiple classes for each kind of statement
    /// </summary>
    /// <typeparam name="T">The return type of the SQL query</typeparam>
    /// <typeparam name="TMe">A Tuple with the query commandParameters</typeparam>
    public class FluentGenericQueryBuilder<T, TMe> : BaseQueryBuilder<T>
        where T : class, new()
    {
        public FluentGenericQueryBuilder(FolkeConnection connection):base(connection)
        {
            parametersType = typeof (TMe);
        }

        public FluentGenericQueryBuilder(IDatabaseDriver driver):base(driver)
        {
            parametersType = typeof(TMe);
        }

        public FluentGenericQueryBuilder<T, TMe> Select(params Expression<Func<T, object>>[] columns)
        {
            foreach (var column in columns)
            {
                AppendSelect();
                var tableColumn = ExpressionToColumn(column.Body, true);
                if (tableColumn == null)
                {
                    AddExpression(column.Body);
                }
                else
                {
                    AppendSelectedColumn(tableColumn);
                }
            }
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> From<TU>(Expression<Func<T, TU>> tableAlias)
        {
            AppendFrom();
            AppendTable(tableAlias.Body);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> FromSubQuery(Action<FluentGenericQueryBuilder<T, TMe>> subqueryFactory)
        {
            AppendFrom();
            SubQuery(subqueryFactory);
            query.Append(" AS ");

            var table = RegisterTable(typeof(T), null);
            query.Append(table.name);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> InnerJoinSubQuery<TU>(Action<FluentGenericQueryBuilder<T, TMe>> subqueryFactory, Expression<Func<TU>> tableAlias)
        {
            Append(" INNER JOIN");
            SubQuery(subqueryFactory);
            query.Append(" AS ");
            var table = RegisterTable(typeof(TU), GetTableAlias(tableAlias.Body as MemberExpression));
            query.Append(table.name);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> From()
        {
            AppendFrom();
            AppendTable(typeof(T), (string) null);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> AndFrom<TU>(Expression<Func<T, TU>> tableAlias)
        {
            Append(",");
            AppendTable(tableAlias.Body);
            return this;
        }
        
        public FluentGenericQueryBuilder<T, TMe> LeftJoin<TU>(Expression<Func<T, TU>> tableAlias)
        {
            Append("LEFT JOIN");
            AppendTable(tableAlias.Body);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> LeftJoinOnId<TU>(Expression<Func<T, TU>> column)
        {
            return LeftJoin(column).OnId(column);
        }

        public FluentGenericQueryBuilder<T, TMe> RightJoin<TU>(Expression<Func<T, TU>> tableAlias)
        {
            Append("RIGHT JOIN");
            AppendTable(tableAlias.Body);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> InnerJoin<TU>(Expression<Func<T, TU>> tableAlias)
        {
            Append("INNER JOIN");
            AppendTable(tableAlias.Body);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> SelectAll<TU>(Expression<Func<T, TU>> tableExpression)
        {
            AppendSelect();
            Expression expressionBody = tableExpression.Body;
            Type tableType = expressionBody.Type;
            AppendAllSelects(tableType, GetTableAlias(expressionBody));
            return this;
        }

        /// <summary>
        /// Select all the field of the bean table
        /// </summary>
        /// <returns>The query builder</returns>
        public FluentGenericQueryBuilder<T, TMe> SelectAll()
        {
            AppendSelect();
            AppendAllSelects(typeof(T), null);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> SelectAs<TU>(Expression<Func<T, TU>> valueExpression,
            Expression<Func<T, TU>> targetExpression)
        {
            AppendSelect();
            AddExpression(valueExpression.Body);
            SelectField(targetExpression.Body);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> AndAll<TU>(Expression<Func<T, TU>> tableAlias)
        {
            return SelectAll(tableAlias);
        }

        public FluentGenericQueryBuilder<T, TMe> SelectCountAll()
        {
            AppendSelect();
            Append(" COUNT(*)");
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> SubQuery(Action<FluentGenericQueryBuilder<T, TMe>> subQuery)
        {
            var builder = new FluentGenericQueryBuilder<T, TMe>(driver) { parameters = parameters };
            foreach (var tableAliase in tables)
            {
                builder.tables.Add(tableAliase);
            }
            builder.defaultTable = defaultTable;
            subQuery(builder);
            parameters = builder.parameters;
            query.Append(" (");
            query.Append(builder.Sql);
            query.Append(')');
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> OnId<TU>(Expression<Func<T, TU>> rightColumn)
        {
            currentContext = ContextEnum.Join;
            Append("ON ");
            var memberExpression = (MemberExpression) rightColumn.Body;
            AppendColumn(ExpressionToColumn(memberExpression));
            query.Append("=");
            AppendColumn(GetTableKey(memberExpression));
            return this;
        }
        
        public FluentGenericQueryBuilder<T, TMe> On<TU>(Expression<Func<T, TU>> expression)
        {
            currentContext = ContextEnum.Join;
            Append("ON ");
            AddExpression(expression.Body);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> OrWhere<TU>(Expression<Func<T, TU>> expression)
        {
            Append(currentContext == ContextEnum.Where ? "OR" : "WHERE");
            currentContext = ContextEnum.Where;
            AddExpression(expression.Body);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> WhereExists(Action<FluentGenericQueryBuilder<T, TMe>> subQuery)
        {
            Where();
            Append("EXISTS");
            SubQuery(subQuery);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> WhereNotExists(Action<FluentGenericQueryBuilder<T, TMe>> subQuery)
        {
            Where();
            Append("NOT EXISTS");
            SubQuery(subQuery);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> Where(Expression<Func<T, TMe, bool>> expression)
        {
            Where();
            AddExpression(expression.Body);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> Where(Expression<Func<T, bool>> expression)
        {
            Where();
            AddExpression(expression.Body);
            return this;
        }

        /*public FluentGenericQueryBuilder<T, TMe> Where(Expression<Func<T, SubQueryHelper<T, TMe>, bool>> expression)
        {
            Where();
            AddExpression(expression);
            return this;
        }*/

        public FluentGenericQueryBuilder<T, TMe> AndOn(Expression<Func<T, bool>> expression)
        {
            Append("AND ");
            AddExpression(expression.Body);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> Update()
        {
            Append("UPDATE ");
            AppendTable(typeof(T), (string) null);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> InsertInto()
        {
            Append("INSERT INTO");
            AppendTableName(typeof(T));
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> GroupBy<TU>(Expression<Func<T, TU>> column)
        {
            if (currentContext != ContextEnum.GroupBy)
                Append("GROUP BY ");
            else
                query.Append(',');
            currentContext = ContextEnum.GroupBy;
            AppendColumn(column.Body);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> OrderBy<TU>(Expression<Func<T, TU>> column)
        {
            if (currentContext != ContextEnum.OrderBy)
                Append("ORDER BY ");
            else
                query.Append(',');
            currentContext = ContextEnum.OrderBy;
            AppendColumn(column.Body);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> Desc()
        {
            Append("DESC");
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> Asc()
        {
            Append("ASC");
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> Limit(int offset, int count)
        {
            query.Append(" LIMIT ").Append(offset).Append(",").Append(count);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> Limit(Expression<Func<T, int>> offset, int count)
        {
            query.Append(" LIMIT ");
            AddExpression(offset.Body);
            query.Append(",").Append(count);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> Limit(Expression<Func<T, TMe, int>> offset, int count)
        {
            query.Append(" LIMIT ");
            AddExpression(offset.Body);
            query.Append(",").Append(count);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> Values(T value)
        {
            currentContext = ContextEnum.Values;
            query.Append(" (");
            bool first = true;
            var type = value.GetType();
            foreach (var property in type.GetProperties())
            {
                if (TableHelpers.IsIgnored(property.PropertyType) || TableHelpers.IsReadOnly(property))
                    continue;
                if (first)
                    first = false;
                else
                    query.Append(",");
                AppendColumn(null, property);
            }
            query.Append(") VALUES (");
            first = true;
            foreach (var property in type.GetProperties())
            {
                if (TableHelpers.IsIgnored(property.PropertyType) || TableHelpers.IsReadOnly(property))
                    continue;
                if (first)
                    first = false;
                else
                    query.Append(",");
                AppendParameter(property.GetValue(value));
            }
            query.Append(")");
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> SetAll(T value)
        {
            Append("SET ");
            currentContext = ContextEnum.Set;
            var type = value.GetType();
            bool first = true;
            var table = defaultTable;
            foreach (var property in type.GetProperties())
            {
                if (TableHelpers.IsIgnored(property.PropertyType) || TableHelpers.IsReadOnly(property))
                    continue;

                if (first)
                    first = false;
                else
                    query.Append(",");
                AppendColumn(table.name, property);
                query.Append("=");
                AppendParameter(property.GetValue(value));
            }
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> Set<TU>(Expression<Func<T, TU>> column, Expression<Func<T, TU>> value)
        {
            if (currentContext == ContextEnum.Set)
            {
                Append(", ");
            }
            else
            {
                Append("SET ");
                currentContext = ContextEnum.Set;
            }

            AppendColumn(column.Body, registerTable: true);
            Append("=");
            AddExpression(value.Body);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> Delete()
        {
            currentContext = ContextEnum.Delete;
            noAlias = true;
            Append("DELETE");
            return this;
        }
    }
}