using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Folke.Orm
{
    /// <summary>
    /// Used to create a SQL query and create objects with its results
    /// TODO cut in multiple classes for each kind of statement
    /// </summary>
    /// <typeparam name="T">The return type of the SQL query</typeparam>
    /// <typeparam name="TMe">A Tuple with the query commandParameters</typeparam>
    public class FluentGenericQueryBuilder<T, TMe> : BaseQueryBuilder<T>
        where T : class, new()
    {
        private Stack<ContextEnum> queryStack;

        public FluentGenericQueryBuilder(FolkeConnection connection):base(connection)
        {
            parametersType = typeof (TMe);
        }

        public FluentGenericQueryBuilder(IDatabaseDriver driver):base(driver)
        {
            parametersType = typeof(TMe);
        }

        public FluentGenericQueryBuilder<T, TMe> Append(string sql)
        {
            if (query.Length != 0)
                query.Append(' ');
            query.Append(sql);
            return this;
        }

        protected TableAlias RegisterTable<TU>(Expression<Func<T, TU>> alias)
        {
            return RegisterTable(typeof(TU), GetTableAlias(alias.Body as MemberExpression));
        }

        public TableAlias RegisterTable()
        {
            return RegisterTable(null, null);
        }

        internal TableAlias RegisterTable(Type type, string tableAlias)
        {
            if (tableAlias == null)
            {
                if (parameterTable == null)
                {
                    parameterTable = new TableAlias { name = "t", alias = null, type = defaultType };
                    tables.Add(parameterTable);
                }
                return parameterTable;
            }

            var table = tables.SingleOrDefault(t => t.alias == tableAlias);
            if (table == null)
            {
                table = new TableAlias { name = "t" + tables.Count, alias = tableAlias, type = type };
                tables.Add(table);
            }
            return table;
        }

        /*private void AppendField<U>(Expression<Func<T, U>> expression)
        {
            // TODO sans doute qu'on devrait gérer x.Toto.Tutu si x.Toto est une table
            var memberExpression = expression.Body as MemberExpression;
            var table = parameterTable;
            AppendField(table.name, typeof(U), memberExpression.Member.Name);
        }*/

        protected FluentGenericQueryBuilder<T, TMe> Column(Type tableType, MemberExpression tableAlias, PropertyInfo column)
        {
            return Column(tableType, GetTableAlias(tableAlias), column);
        }

        /// <summary>
        /// Append a column name to the query. If the context is Select, the column is added
        /// to the list of selected columns.
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="tableAlias"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        protected FluentGenericQueryBuilder<T, TMe> Column(Type tableType, string tableAlias, PropertyInfo column)
        {
            TableAlias table;
            if (currentContext == ContextEnum.Select)
            {
                table = RegisterTable(tableType, tableAlias);
                if (selectedFields == null)
                    selectedFields = new List<FieldAlias>();
                selectedFields.Add(new FieldAlias { propertyInfo = column, alias = table.alias, index = selectedFields.Count });
            }
            else
            {
                table = GetTable(tableAlias);
                if (table == null)
                    throw new Exception("Table " + tableAlias + " not found");

                if (table.type != tableType)
                    throw new Exception("Internal error, table type " + tableType + " does not match table alias " + tableAlias + ", which had a type of " + table.type);
            }
            AppendField(table.name, column);
            return this;
        }

        /// <summary>
        /// Append a column name to the query. The table is always T. If the context is Select, the column is added
        /// to the list of selected columns.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public FluentGenericQueryBuilder<T, TMe> Column(PropertyInfo column)
        {
            return Column(typeof(T), (string) null, column);
        }

        /// <summary>
        /// Append a column to the query using a lambda expression. If the context is Select, the column is added
        /// to the list of selected columns.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public FluentGenericQueryBuilder<T, TMe> Column(Expression column)
        {
            if (column.NodeType == ExpressionType.Parameter)
            {
                return Column(typeof(T), (string)null, typeof(T).GetProperty("Id"));
            }
            var columnMember = (MemberExpression) column;
            
            if (columnMember.Expression is MemberExpression)
            {
                return Column(columnMember.Expression.Type, GetTableAlias(columnMember.Expression as MemberExpression), (PropertyInfo) columnMember.Member);
            }
            return Column(typeof(T), (string)null, (PropertyInfo)columnMember.Member);
        }

        /// <summary>
        /// Add a column name to the query using a lambda expression. If the context is Select, the column is added
        /// to the list of selected columns.
        /// </summary>
        /// <typeparam name="TU">The column type</typeparam>
        /// <param name="column">An expression that returns the column</param>
        /// <returns>The query itself</returns>
        public FluentGenericQueryBuilder<T, TMe> Column<TU>(Expression<Func<T, TU>> column)
        {
            return Column(column.Body);
        }

        /// <summary>
        /// TODO nécessaire ?
        /// </summary>
        /// <param name="expression"></param>
        private void OnField(MemberExpression expression)
        {
            if (expression.Expression is ParameterExpression)
            {
                var table = GetTable(expression);
                if (table != null)
                    AppendField(table.name, TableHelpers.GetKey(table.type));
                else
                    AppendField(GetTable((string)null).name, expression.Member);
            }
            else if (expression.Expression is MemberExpression)
            {
                var accessTo = expression.Expression as MemberExpression;
                var table = GetTable(accessTo);
                AppendField(table.name, expression.Member);
            }
            else
                throw new Exception("Must be a x.Member or x.Member.Submember");
        }

        /// <summary>
        /// Append all the columns of the table to a select expression
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="tableAlias"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        protected FluentGenericQueryBuilder<T, TMe> Columns(Type tableType, MemberExpression tableAlias, IEnumerable<PropertyInfo> columns)
        {
            return Columns(tableType, GetTableAlias(tableAlias), columns);
        }

        /// <summary>
        /// Append all the columns of a table to a select expression
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="tableAlias"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        protected FluentGenericQueryBuilder<T, TMe> Columns(Type tableType, string tableAlias, IEnumerable<PropertyInfo> columns)
        {
            var table = RegisterTable(tableType, tableAlias);
            if (query.Length != 0)
                query.Append(' ');
            bool first = true;
            if (selectedFields == null)
                selectedFields = new List<FieldAlias>();

            foreach (var column in columns)
            {
                if (TableHelpers.IsIgnored(column.PropertyType))
                    continue;

                selectedFields.Add(new FieldAlias { propertyInfo = column, alias = table.alias, index = selectedFields.Count });

                if (first)
                    first = false;
                else
                    query.Append(',');
                AppendField(table.name, column);
            }
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> Columns(params Expression<Func<T, object>>[] column)
        {
            // TODO gérer le cas où le retour est casté depuis une value
            return Columns(typeof(T), (string) null, column.Select(TableHelpers.GetExpressionPropertyInfo));
        }

        /// <summary>
        /// Select all the table columns 
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="tableAlias"></param>
        /// <returns></returns>
        public FluentGenericQueryBuilder<T, TMe> All(Type tableType, MemberExpression tableAlias)
        {
            return All(tableType, GetTableAlias(tableAlias));
        }

        public FluentGenericQueryBuilder<T, TMe> All(Type tableType, string tableAlias)
        {
            return Columns(tableType, tableAlias, tableType.GetProperties());
        }

        public FluentGenericQueryBuilder<T, TMe> All<TU>(Expression<Func<T, TU>> tableAlias)
        {
            return All(typeof(TU), (MemberExpression) tableAlias.Body);
        }

        protected FluentGenericQueryBuilder<T, TMe> Table<TU>(Expression<Func<T, TU>> tableAlias)
        {
            return Table(tableAlias.Body.Type, tableAlias.Body as MemberExpression);
        }

        protected FluentGenericQueryBuilder<T, TMe> Table(Type tableType, MemberExpression tableAlias)
        {
            return Table(tableType, GetTableAlias(tableAlias));
        }

        protected FluentGenericQueryBuilder<T, TMe> Table(Type tableType, string tableAlias)
        {
            var table = RegisterTable(tableType, tableAlias);
            
            AppendTableName(tableType);
            if (!noAlias)
            {
                query.Append(" as ");
                query.Append(table.name);
            }
            return this;
        }

        private void AppendFrom()
        {
            if (currentContext == ContextEnum.Select || currentContext == ContextEnum.Delete)
            {
                baseMappedClass = MappedClass.MapClass(selectedFields, typeof(T));
                Append("FROM");
            }
            else if (currentContext == ContextEnum.From)
                Append(",");
            currentContext = ContextEnum.From;
        }

        public FluentGenericQueryBuilder<T, TMe> From<TU>(Expression<Func<TU>> tableAlias)
        {
            AppendFrom();
            return Table(typeof(TU), (MemberExpression) tableAlias.Body);
        }

        public FluentGenericQueryBuilder<T, TMe> From<TU>(Expression<Func<T, TU>> tableAlias)
        {
            AppendFrom();
            return Table(tableAlias);
        }

        public FluentGenericQueryBuilder<T, TMe> FromSubQuery(Func<FluentGenericQueryBuilder<T, TMe>, FluentGenericQueryBuilder<T, TMe>> subqueryFactory)
        {
            var builder = new FluentGenericQueryBuilder<T, TMe>(driver) {parameters = parameters};
            var subquery = subqueryFactory.Invoke(builder);
            parameters = builder.parameters;
            AppendFrom();
            query.Append(" (");
            query.Append(subquery.query);
            query.Append(") AS ");

            var table = RegisterTable(typeof(T), null);
            query.Append(table.name);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> InnerJoinSubQuery<TU>(Func<FluentGenericQueryBuilder<T, TMe>, FluentGenericQueryBuilder<T, TMe>> subqueryFactory, Expression<Func<TU>> tableAlias)
        {
            var builder = new FluentGenericQueryBuilder<T, TMe>(driver) {parameters = parameters};
            var subquery = subqueryFactory.Invoke(builder);
            parameters = builder.parameters;
            Append(" INNER JOIN (");
            query.Append(subquery.Sql);
            query.Append(") AS ");
            var table = RegisterTable(typeof(TU), GetTableAlias(tableAlias.Body as MemberExpression));
            query.Append(table.name);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> From()
        {
            AppendFrom();
            return Table(typeof(T), (string) null);
        }

        public FluentGenericQueryBuilder<T, TMe> AndFrom<TU>(Expression<Func<T, TU>> tableAlias)
        {
            Append(",");
            return Table(tableAlias);
        }

        public FluentGenericQueryBuilder<T, TMe> LeftJoin(Type tableType, string tableAlias)
        {
            Append("LEFT JOIN");
            return Table(tableType, tableAlias);
        }

        public FluentGenericQueryBuilder<T, TMe> LeftJoin<TU>(Expression<Func<T, TU>> tableAlias)
        {
            Append("LEFT JOIN");
            return Table(tableAlias);
        }

        public FluentGenericQueryBuilder<T, TMe> LeftJoinOnId<TU>(Expression<Func<T, TU>> column)
        {
            return LeftJoin(column).OnId(column);
        }

        public FluentGenericQueryBuilder<T, TMe> RightJoin<TU>(Expression<Func<T, TU>> tableAlias)
        {
            Append("RIGHT JOIN");
            return Table(tableAlias);
        }

        public FluentGenericQueryBuilder<T, TMe> InnerJoin<TU>(Expression<Func<T, TU>> tableAlias)
        {
            Append("INNER JOIN");
            return Table(tableAlias);
        }
        
        internal void AddExpression<TU>(Expression<Func<T, TU>> expression)
        {
            AddExpression(expression.Body);
        }

        internal void AddExpression(Expression expression)
        {
            if (expression is UnaryExpression)
            {
                var unary = expression as UnaryExpression;
                switch (unary.NodeType)
                {
                    case ExpressionType.Negate:
                        query.Append('-');
                        break;
                    case ExpressionType.Not:
                        query.Append(" NOT ");
                        break;
                    case ExpressionType.Convert:
                        break;
                    default:
                        throw new Exception("ExpressionType in UnaryExpression not supported");
                }
                AddExpression(unary.Operand);
                return;
            }
            
            if (expression is BinaryExpression)
            {
                var binary = expression as BinaryExpression;
                query.Append('(');
                
                AddExpression(binary.Left);

                if (binary.Right.NodeType == ExpressionType.Constant && ((ConstantExpression) binary.Right).Value == null)
                {
                    if (binary.NodeType == ExpressionType.Equal)
                        query.Append(" IS NULL");
                    else if (binary.NodeType == ExpressionType.NotEqual)
                        query.Append(" IS NOT NULL");
                    else
                        throw new Exception("Operator not supported with null right member in " + binary);
                    query.Append(")");
                    return;
                }

                switch (binary.NodeType)
                {
                    case ExpressionType.Add:
                        query.Append("+");
                        break;
                    case ExpressionType.AndAlso:
                        query.Append(" AND ");
                        break;
                    case ExpressionType.Divide:
                        query.Append("/");
                        break;
                    case ExpressionType.Equal:
                        query.Append('=');
                        break;
                    case ExpressionType.GreaterThan:
                        query.Append(">");
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        query.Append(">=");
                        break;
                    case ExpressionType.LessThan:
                        query.Append("<");
                        break;
                    case ExpressionType.LessThanOrEqual:
                        query.Append("<=");
                        break;
                    case ExpressionType.Modulo:
                        query.Append("%");
                        break;
                    case ExpressionType.Multiply:
                        query.Append('*');
                        break;
                    case ExpressionType.OrElse:
                        query.Append(" OR ");
                        break;
                    case ExpressionType.Subtract:
                        query.Append('-');
                        break;
                    default:
                        throw new Exception("Expression type not supported");
                }

                if (binary.Right.NodeType == ExpressionType.Constant && binary.Left.NodeType == ExpressionType.Convert
                    && ((UnaryExpression)binary.Left).Operand.Type.IsEnum)
                {
                    var enumType = ((UnaryExpression)binary.Left).Operand.Type;
                    var enumIndex = (int) ((ConstantExpression)binary.Right).Value;
                    Parameter(enumType.GetEnumValues().GetValue(enumIndex));
                }
                else
                {
                    AddExpression(binary.Right);
                }
                query.Append(')');
                return;
            }

            if (expression is ConstantExpression)
            {
                var constant = expression as ConstantExpression;
                Parameter(constant.Value);
                return;
            }

            if (expression is MemberExpression)
            {
                if (TryColumn(expression as MemberExpression))
                    return;
            }

            if (expression.NodeType == ExpressionType.Parameter)
            {
                AppendField(parameterTable.name, TableHelpers.GetKey(parameterTable.type));
                return;
            }

            if (expression.NodeType == ExpressionType.Call)
            {
                var call = (MethodCallExpression)expression;
                
                if (call.Method.DeclaringType == typeof (ExpressionHelpers))
                {
                    switch (call.Method.Name)
                    {
                        case "Property":
                            var propertyInfo = (PropertyInfo)Expression.Lambda(call.Arguments[1]).Compile().DynamicInvoke();
                            TryColumn(call.Arguments[0], propertyInfo);
                            break;
                        case "Like":
                            AddExpression(call.Arguments[0]);
                            query.Append(" LIKE ");
                            AddExpression(call.Arguments[1]);
                            break;
                        default:
                            throw new Exception("Unsupported expression helper");
                    }
                    return;
                }

                if (call.Method.DeclaringType == typeof (string))
                {
                    switch (call.Method.Name)
                    {
                        case "StartsWith":
                            AddExpression(call.Object);
                            query.Append(" LIKE ");
                            var text = (string) Expression.Lambda(call.Arguments[0]).Compile().DynamicInvoke();
                            text = text.Replace("\\", "\\\\").Replace("%","\\%") + "%";
                            Parameter(text);
                            break;
                        default:
                            throw new Exception("Unsupported string method");
                    }
                    return;
                }

                if (call.Method.Name == "Equals")
                {
                    query.Append('(');
                    AddExpression(call.Object);
                    query.Append('=');
                    AddExpression(call.Arguments[0]);
                    query.Append(')');
                    return;
                }
            }

            var value = Expression.Lambda(expression).Compile().DynamicInvoke();
            Parameter(value);
        }
        
        public FluentGenericQueryBuilder<T, TMe> SelectAll<TU>(Expression<Func<T, TU>> tableAlias)
        {
            Select();
            return All(tableAlias);
        }

        /// <summary>
        /// Select all the field of the selected table
        /// </summary>
        /// <typeparam name="TU">The table type</typeparam>
        /// <param name="tableAlias">The table</param>
        /// <returns>The query builder</returns>
        public FluentGenericQueryBuilder<T, TMe> SelectAll<TU>(Expression<Func<TU>> tableAlias)
        {
            Select();
            return All(typeof(TU), (MemberExpression) tableAlias.Body);
        }

        /// <summary>
        /// Select all the field of the bean table
        /// </summary>
        /// <returns>The query builder</returns>
        public FluentGenericQueryBuilder<T, TMe> SelectAll()
        {
            Select();
            return All(typeof(T), (string) null);
        }

        /// <summary>
        /// Begins a select command
        /// </summary>
        /// <returns>The query builder</returns>
        public FluentGenericQueryBuilder<T, TMe> Select()
        {
            if (currentContext == ContextEnum.Select)
            {
                Append(",");
            }
            else
            {
                currentContext = ContextEnum.Select;
                Append("SELECT");
            }
            return this;
        }
        
        /// <summary>
        /// Select several columns
        /// </summary>
        /// <param name="column">A column to select</param>
        /// <returns></returns>
        public FluentGenericQueryBuilder<T, TMe> Select(params Expression<Func<T, object>>[] column)
        {
            Select();
            return Columns(typeof(T), (string) null, column.Select(TableHelpers.GetExpressionPropertyInfo));
        }

        public FluentGenericQueryBuilder<T, TMe> Select<TU, TV>(Expression<Func<TU>> tableAlias, Expression<Func<TV>> column)
        {
            Select();
            return Column(typeof(TU), (MemberExpression) tableAlias.Body, ((MemberExpression) column.Body).Member as PropertyInfo);
        }
        
        public FluentGenericQueryBuilder<T, TMe> AndAll(Type tableType, MemberExpression tableAlias)
        {
            return AndAll(tableType, GetTableAlias(tableAlias));
        }

        public FluentGenericQueryBuilder<T, TMe> AndAll(Type tableType, string tableAlias)
        {
            Append(",");
            return All(tableType, tableAlias);
        }

        public FluentGenericQueryBuilder<T, TMe> AndAll<TU>(Expression<Func<T, TU>> tableAlias)
        {
            return AndAll(tableAlias.Body.Type, (MemberExpression) tableAlias.Body);
        }

        public FluentGenericQueryBuilder<T, TMe> SelectCountAll()
        {
            Select();
            Append(" COUNT(*)");
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> On<TU>(Expression<Func<T, TU>> leftColumn, Expression<Func<T, TU>> rightColumn)
        {
            currentContext = ContextEnum.Join;
            Append("ON ");
            OnField(leftColumn.Body as MemberExpression);
            query.Append("=");
            OnField(rightColumn.Body as MemberExpression);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> On(PropertyInfo leftColumn, string leftTableAlias, PropertyInfo rightColumn, string rightTableAlias)
        {
            currentContext = ContextEnum.Join;
            Append("ON ");
            AppendField(GetTable(leftTableAlias).name, leftColumn);
            query.Append("=");
            AppendField(GetTable(rightTableAlias).name, rightColumn);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> OnId<TU>(Expression<Func<T, TU>> rightColumn)
        {
            currentContext = ContextEnum.Join;
            Append("ON ");
            var memberExpression = (MemberExpression) rightColumn.Body;
            Column(memberExpression);
            query.Append("=");
            Column(memberExpression.Type, GetTableAlias(memberExpression), memberExpression.Type.GetProperty("Id"));
            return this;
        }
        
        public FluentGenericQueryBuilder<T, TMe> On<TU>(Expression<Func<T, TU>> expression)
        {
            currentContext = ContextEnum.Join;
            Append("ON ");
            AddExpression(expression.Body);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> Max<TU>(Expression<Func<T, TU>> column)
        {
            return Append("MAX(").Column(column).Append(")");
        }

        public FluentGenericQueryBuilder<T, TMe> BeginMax()
        {
            return Append("MAX(");
        }

        public FluentGenericQueryBuilder<T, TMe> EndMax()
        {
            query.Append(')');
            return this;
        }

        /// <summary>
        /// Start a sub-select
        /// </summary>
        /// <returns></returns>
        public FluentGenericQueryBuilder<T, TMe> BeginSub()
        {
            if (queryStack == null)
                queryStack = new Stack<ContextEnum>();
            queryStack.Push(currentContext);
            currentContext = ContextEnum.Unknown;
            query.Append('(');
            return this;
        }

        /// <summary>
        /// End a sub-select
        /// </summary>
        /// <returns></returns>
        public FluentGenericQueryBuilder<T, TMe> EndSub()
        {
            currentContext = queryStack.Pop();
            query.Append(')');
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> Where()
        {
            if (currentContext == ContextEnum.Unknown)
                SelectAll().From();
            Append(currentContext == ContextEnum.Where ? "AND" : "WHERE");
            currentContext = ContextEnum.Where;
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> OrWhere()
        {
            Append(currentContext == ContextEnum.Where ? "OR" : "WHERE");
            currentContext = ContextEnum.Where;
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> OrWhere<TU>(Expression<Func<T, TU>> expression)
        {
            OrWhere().AddExpression(expression.Body);
            return this;
        }

        /// <summary>
        /// Begins a sub-expression in a where expression (open a parenthesis)
        /// </summary>
        /// <returns></returns>
        public FluentGenericQueryBuilder<T, TMe> BeginWhereSubExpression<TU>(Expression<Func<T, TU>> expression)
        {
            Where();
            query.Append('(');
            AddExpression(expression.Body);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> EndWhereSubExpression()
        {
            query.Append(')');
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> Exists()
        {
            return Append("EXISTS");
        }

        public FluentGenericQueryBuilder<T, TMe> NotExists()
        {
            return Append("NOT EXISTS");
        }
        
        public FluentGenericQueryBuilder<T, TMe> Equals()
        {
            query.Append('=');
            return this;
        }

        /// <summary>
        /// Add a IN operator. 
        /// Example: Where().Column(x => x.Value).In(new[]{12, 13, 14})
        /// </summary>
        /// <typeparam name="TU"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public FluentGenericQueryBuilder<T, TMe> In<TU>(IEnumerable<TU> values)
        {
            query.Append(" IN (");
            bool first = true;
            foreach (var value in values)
            {
                if (first)
                    first = false;
                else
                    query.Append(',');
                Parameter(value);
            }
            query.Append(')');
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> WhereIn<TU>(Expression<Func<T, TU>> column, IEnumerable<TU> values)
        {
            return Where().Column(column).In(values);
        }

        public FluentGenericQueryBuilder<T, TMe> Fetch(params Expression<Func<T, object>>[] fetches)
        {
            SelectAll();
            foreach (var fetch in fetches)
                AndAll(fetch);
            From();
            foreach (var fetch in fetches)
                LeftJoinOnId(fetch);
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

        public FluentGenericQueryBuilder<T, TMe> AndOn(Expression<Func<T, bool>> expression)
        {
            Append("AND ");
            AddExpression(expression.Body);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> Update()
        {
            Append("UPDATE ");
            Table(typeof(T), (string) null);
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
            if (!TryColumn(column.Body as MemberExpression))
                throw new Exception(column + " is not a valid column");
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> OrderBy<TU>(Expression<Func<T, TU>> column)
        {
            if (currentContext != ContextEnum.OrderBy)
                Append("ORDER BY ");
            else
                query.Append(',');
            currentContext = ContextEnum.OrderBy;
            if (!TryColumn(column.Body as MemberExpression))
                throw new Exception(column + " is not a valid column");
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> Desc()
        {
            return Append("DESC");
        }

        public FluentGenericQueryBuilder<T, TMe> Asc()
        {
            return Append("ASC");
        }

        public FluentGenericQueryBuilder<T, TMe> Limit(int offset, int count)
        {
            query.Append(" LIMIT ").Append(offset).Append(",").Append(count);
            return this;
        }

        public FluentGenericQueryBuilder<T, TMe> Limit<TU>(Expression<Func<TMe, TU>> offset, int count)
        {
            var expression = (MemberExpression)offset.Body;
            query.Append(" LIMIT @").Append(expression.Member.Name).Append(",").Append(count);
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
                AppendField(null, property);
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
                Parameter(property.GetValue(value));
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
            var table = parameterTable;
            foreach (var property in type.GetProperties())
            {
                if (TableHelpers.IsIgnored(property.PropertyType) || TableHelpers.IsReadOnly(property))
                    continue;

                if (first)
                    first = false;
                else
                    query.Append(",");
                AppendField(table.name, property);
                query.Append("=");
                Parameter(property.GetValue(value));
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

            Column(column);
            Append("=");
            AddExpression(value.Body);
            return this;
        }


        public FluentGenericQueryBuilder<T, TMe> Delete()
        {
            currentContext = ContextEnum.Delete;
            noAlias = true;
            return Append("DELETE");
        }
    }
}