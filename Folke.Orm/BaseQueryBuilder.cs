using System.Globalization;

namespace Folke.Orm
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Used to create a SQL query and create objects with its results
    /// </summary>
    /// <typeparam name="T">The return type of the SQL query</typeparam>
    /// <typeparam name="TMe">A Tuple with the query commandParameters</typeparam>
    public class BaseQueryBuilder<T, TMe>
        where T : class, new()
    {
        private enum ContextEnum
        {
            /// <summary>
            /// Context is unknown.
            /// </summary>
            Unknown,

            /// <summary>
            /// Currently in a SELECT statement.
            /// </summary>
            Select,

            /// <summary>
            /// In a WHERE part.
            /// </summary>
            Where,

            /// <summary>
            /// In the ORDER BY part.
            /// </summary>
            OrderBy,

            /// <summary>
            /// In a SET statment.
            /// </summary>
            Set,

            /// <summary>
            /// In any JOIN part.
            /// </summary>
            Join,

            /// <summary>
            /// In the VALUES() part of an INSERT statment.
            /// </summary>
            Values,

            /// <summary>
            /// In the FROM part of a SELECT statment.
            /// </summary>
            From,

            /// <summary>
            /// In a DELETE statement.
            /// </summary>
            Delete,

            /// <summary>
            /// In a GROUP BY part.
            /// </summary>
            GroupBy
        }

        public class TableAlias
        {
            public Type type;
            public string name;
            public string alias;
        }

        public class FieldAlias
        {
            public PropertyInfo propertyInfo;
            public string alias;
            public int index;
        }

        protected StringBuilder query = new StringBuilder();
        private IList<FieldAlias> selectedFields;
        private readonly IList<TableAlias> tables;
        protected TableAlias parameterTable;

        private ContextEnum currentContext = ContextEnum.Unknown;
        private Stack<ContextEnum> queryStack;
        private bool noAlias;
        private IList<object> parameters;

        protected readonly FolkeConnection connection;
        protected readonly IDatabaseDriver driver;
        protected readonly char beginSymbol;
        protected readonly char endSymbol;

        public BaseQueryBuilder(FolkeConnection connection)
        {
            this.connection = connection;
            driver = connection.Driver;
            beginSymbol = driver.BeginSymbol;
            endSymbol = driver.EndSymbol;
            tables = new List<TableAlias>();
        }

        public BaseQueryBuilder(IDatabaseDriver driver)
        {
            this.driver = driver;
            beginSymbol = driver.BeginSymbol;
            endSymbol = driver.EndSymbol;
            tables = new List<TableAlias>();
        }

        public string Sql
        {
            get
            {
                return query.ToString();
            }
        }

        public BaseQueryBuilder<T, TMe> Append(string sql)
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

        private string GetTableAlias(MemberExpression tableAlias)
        {
            if (tableAlias == null || tableAlias.Expression == null)
                return null;

            string aliasValue = null;
            while (true)
            {
                if (aliasValue != null)
                    aliasValue = tableAlias.Member.Name + "." + aliasValue;
                else
                    aliasValue = tableAlias.Member.Name;

                if (tableAlias.Expression is ParameterExpression)
                    break;

                if (tableAlias.Expression.NodeType != ExpressionType.MemberAccess)
                {
                    aliasValue = tableAlias.Expression + "." + aliasValue;
                    break;
                }
                tableAlias = (MemberExpression)tableAlias.Expression;
            }
            return aliasValue;
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
                    parameterTable = new TableAlias { name = "t", alias = null, type = typeof(T) };
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

        protected TableAlias GetTable(string tableAlias)
        {
            return tables.SingleOrDefault(t => t.alias == tableAlias);
        }

        protected TableAlias GetTable(MemberExpression alias)
        {
            return GetTable(GetTableAlias(alias));
        }
        
        /// <summary>
        /// Add a field name to the query. Very low level.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="propertyInfo"></param>
        private void AppendField(string tableName, MemberInfo propertyInfo)
        {
            query.Append(' ');
            if (tableName != null && !noAlias)
            {
                query.Append(tableName);
                query.Append(".");
            }
            query.Append(beginSymbol);
            query.Append(TableHelpers.GetColumnName(propertyInfo));
            query.Append(endSymbol);
        }

        /*private void AppendField<U>(Expression<Func<T, U>> expression)
        {
            // TODO sans doute qu'on devrait gérer x.Toto.Tutu si x.Toto est une table
            var memberExpression = expression.Body as MemberExpression;
            var table = parameterTable;
            AppendField(table.name, typeof(U), memberExpression.Member.Name);
        }*/

        protected BaseQueryBuilder<T, TMe> Column(Type tableType, MemberExpression tableAlias, PropertyInfo column)
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
        protected BaseQueryBuilder<T, TMe> Column(Type tableType, string tableAlias, PropertyInfo column)
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
        public BaseQueryBuilder<T, TMe> Column(PropertyInfo column)
        {
            return Column(typeof(T), (string) null, column);
        }

        /// <summary>
        /// Append a column to the query using a lambda expression. If the context is Select, the column is added
        /// to the list of selected columns.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public BaseQueryBuilder<T, TMe> Column(Expression column)
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
        public BaseQueryBuilder<T, TMe> Column<TU>(Expression<Func<T, TU>> column)
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

        protected bool TryColumn(Expression tableExpression, MemberInfo propertyInfo)
        {
            if (currentContext == ContextEnum.Select)
                throw new Exception("Do not call TryColumn in a Select context");

            if (tableExpression is MemberExpression)
            {
                var accessTo = tableExpression as MemberExpression;
                var table = GetTable(accessTo);
                if (table != null)
                {
                    AppendField(table.name, propertyInfo);
                    return true;
                }
                if (propertyInfo.Name == "Id" && TryColumn(accessTo))
                    return true;
            }
            else if (tableExpression is ParameterExpression && tableExpression.Type == typeof(T) && parameterTable != null)
            {
                AppendField(parameterTable.name, propertyInfo);
                return true;
            }
            else if (tableExpression is ParameterExpression && tableExpression.Type == typeof(TMe))
            {
                query.Append("@" + propertyInfo.Name);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Try to append a column name to the query. If the table is not selected, returns false.
        /// Do not call it from the select part of a query.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        protected bool TryColumn(MemberExpression column)
        {
            if (currentContext == ContextEnum.Select)
                throw new Exception("Do not call TryColumn in a Select context");

            if (TryColumn(column.Expression, column.Member))
            {
                return true;
            }
            var table = GetTable(column);
            if (table != null)
            {
                AppendField(table.name, TableHelpers.GetKey(table.type));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Append all the columns of the table to a select expression
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="tableAlias"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        protected BaseQueryBuilder<T, TMe> Columns(Type tableType, MemberExpression tableAlias, IEnumerable<PropertyInfo> columns)
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
        protected BaseQueryBuilder<T, TMe> Columns(Type tableType, string tableAlias, IEnumerable<PropertyInfo> columns)
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

        private static PropertyInfo GetExpressionPropertyInfo(Expression<Func<T, object>> column)
        {
            MemberExpression member;
            if (column.Body.NodeType == ExpressionType.Convert)
                member = (MemberExpression)((UnaryExpression)column.Body).Operand;
            else
                member = (MemberExpression)column.Body;
            return member.Member as PropertyInfo;                
        }

        public BaseQueryBuilder<T, TMe> Columns(params Expression<Func<T, object>>[] column)
        {
            // TODO gérer le cas où le retour est casté depuis une value
            return Columns(typeof(T), (string) null, column.Select(GetExpressionPropertyInfo));
        }

        /// <summary>
        /// Select all the table columns 
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="tableAlias"></param>
        /// <returns></returns>
        public BaseQueryBuilder<T, TMe> All(Type tableType, MemberExpression tableAlias)
        {
            return All(tableType, GetTableAlias(tableAlias));
        }

        public BaseQueryBuilder<T, TMe> All(Type tableType, string tableAlias)
        {
            return Columns(tableType, tableAlias, tableType.GetProperties());
        }

        public BaseQueryBuilder<T, TMe> All<TU>(Expression<Func<T, TU>> tableAlias)
        {
            return All(typeof(TU), (MemberExpression) tableAlias.Body);
        }

        protected void AppendTableName(Type type)
        {
            if (query.Length != 0)
                query.Append(' ');
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            if (tableAttribute != null)
            {
                if (tableAttribute.Schema != null)
                {
                    query.Append(beginSymbol);
                    query.Append(tableAttribute.Schema);
                    query.Append(endSymbol);
                    query.Append('.');
                }

                query.Append(beginSymbol);
                query.Append(tableAttribute.Name);
                query.Append(endSymbol);
            }
            else
            {
                query.Append(beginSymbol);
                query.Append(type.Name);
                query.Append(endSymbol);
            }
        }

        protected BaseQueryBuilder<T, TMe> Table<TU>(Expression<Func<T, TU>> tableAlias)
        {
            return Table(tableAlias.Body.Type, tableAlias.Body as MemberExpression);
        }

        protected BaseQueryBuilder<T, TMe> Table(Type tableType, MemberExpression tableAlias)
        {
            return Table(tableType, GetTableAlias(tableAlias));
        }

        protected BaseQueryBuilder<T, TMe> Table(Type tableType, string tableAlias)
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
                baseMappedClass = MappedClass<T, TMe>.MapClass(selectedFields, typeof(T));
                Append("FROM");
            }
            else if (currentContext == ContextEnum.From)
                Append(",");
            currentContext = ContextEnum.From;
        }

        public BaseQueryBuilder<T, TMe> From<TU>(Expression<Func<TU>> tableAlias)
        {
            AppendFrom();
            return Table(typeof(TU), (MemberExpression) tableAlias.Body);
        }

        public BaseQueryBuilder<T, TMe> From<TU>(Expression<Func<T, TU>> tableAlias)
        {
            AppendFrom();
            return Table(tableAlias);
        }

        public BaseQueryBuilder<T, TMe> FromSubQuery(Func<BaseQueryBuilder<T, TMe>, BaseQueryBuilder<T, TMe>> subqueryFactory)
        {
            var builder = new BaseQueryBuilder<T, TMe>(driver) {parameters = parameters};
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

        public BaseQueryBuilder<T, TMe> InnerJoinSubQuery<TU>(Func<BaseQueryBuilder<T, TMe>, BaseQueryBuilder<T, TMe>> subqueryFactory, Expression<Func<TU>> tableAlias)
        {
            var builder = new BaseQueryBuilder<T, TMe>(driver) {parameters = parameters};
            var subquery = subqueryFactory.Invoke(builder);
            parameters = builder.parameters;
            Append(" INNER JOIN (");
            query.Append(subquery.Sql);
            query.Append(") AS ");
            var table = RegisterTable(typeof(TU), GetTableAlias(tableAlias.Body as MemberExpression));
            query.Append(table.name);
            return this;
        }

        public BaseQueryBuilder<T, TMe> From()
        {
            AppendFrom();
            return Table(typeof(T), (string) null);
        }

        public BaseQueryBuilder<T, TMe> AndFrom<TU>(Expression<Func<T, TU>> tableAlias)
        {
            Append(",");
            return Table(tableAlias);
        }

        public BaseQueryBuilder<T, TMe> LeftJoin(Type tableType, string tableAlias)
        {
            Append("LEFT JOIN");
            return Table(tableType, tableAlias);
        }

        public BaseQueryBuilder<T, TMe> LeftJoin<TU>(Expression<Func<T, TU>> tableAlias)
        {
            Append("LEFT JOIN");
            return Table(tableAlias);
        }

        public BaseQueryBuilder<T, TMe> LeftJoinOnId<TU>(Expression<Func<T, TU>> column)
        {
            return LeftJoin(column).OnId(column);
        }

        public BaseQueryBuilder<T, TMe> RightJoin<TU>(Expression<Func<T, TU>> tableAlias)
        {
            Append("RIGHT JOIN");
            return Table(tableAlias);
        }

        public BaseQueryBuilder<T, TMe> InnerJoin<TU>(Expression<Func<T, TU>> tableAlias)
        {
            Append("INNER JOIN");
            return Table(tableAlias);
        }

        public string Result
        {
            get
            {
                return query.ToString();
            }
        }

        public void Parameter(object parameter)
        {
            if (parameter == null && currentContext != ContextEnum.Set && currentContext != ContextEnum.Values)
            {
                var toChange = query.ToString().TrimEnd(' ');
                if (toChange.LastIndexOf('=') != toChange.Length - 1)
                    throw new Exception("Compare to null must be =");
                query.Clear();
                query.Append(toChange.Substring(0, toChange.Length - 1));
                query.Append(" IS NULL");
                return;
            }

            if (parameters == null)
                parameters = new List<object>();

            var parameterIndex = parameters.Count;
            if (parameter != null)
            {
                var parameterType = parameter.GetType();
                if (parameterType == typeof(TimeSpan))
                {
                    parameter = ((TimeSpan)parameter).TotalSeconds;
                }
                else if (TableHelpers.IsForeign(parameterType))
                {
                    parameter = TableHelpers.GetKey(parameterType).GetValue(parameter);
                    if (parameter.Equals(0))
                        throw new Exception("Id should not be 0");
                }
            }

            parameters.Add(parameter);
            
            query.Append("@Item" + parameterIndex);
        }

        protected static FolkeCommand CreateCommand(StringBuilder stringBuilder, IFolkeConnection folkeConnection, object[] commandParameters)
        {
            var command = folkeConnection.OpenCommand();
            if (commandParameters != null)
            {
                for (var i = 0; i < commandParameters.Length; i++)
                {
                    var parameterName = "Item" + i.ToString(CultureInfo.InvariantCulture);
                    var parameter = commandParameters[i];
                    var commandParameter = command.CreateParameter();
                    commandParameter.ParameterName = parameterName;
                    if (parameter == null)
                        commandParameter.Value = null;
                    else
                    {
                        var parameterType = parameter.GetType();
                        if (parameterType.IsEnum)
                            commandParameter.Value = parameterType.GetEnumName(parameter);
                        else
                        {
                            var table = parameter as IFolkeTable;
                            commandParameter.Value = table != null ? table.Id : parameter;
                        }
                    }
                    command.Parameters.Add(commandParameter);
                }
            }
            command.CommandText = stringBuilder.ToString();
            return command;
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
        
        public BaseQueryBuilder<T, TMe> SelectAll<TU>(Expression<Func<T, TU>> tableAlias)
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
        public BaseQueryBuilder<T, TMe> SelectAll<TU>(Expression<Func<TU>> tableAlias)
        {
            Select();
            return All(typeof(TU), (MemberExpression) tableAlias.Body);
        }

        /// <summary>
        /// Select all the field of the bean table
        /// </summary>
        /// <returns>The query builder</returns>
        public BaseQueryBuilder<T, TMe> SelectAll()
        {
            Select();
            return All(typeof(T), (string) null);
        }

        /// <summary>
        /// Begins a select command
        /// </summary>
        /// <returns>The query builder</returns>
        public BaseQueryBuilder<T, TMe> Select()
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
        public BaseQueryBuilder<T, TMe> Select(params Expression<Func<T, object>>[] column)
        {
            Select();
            return Columns(typeof(T), (string) null, column.Select(GetExpressionPropertyInfo));
        }

        public BaseQueryBuilder<T, TMe> Select<TU, TV>(Expression<Func<TU>> tableAlias, Expression<Func<TV>> column)
        {
            Select();
            return Column(typeof(TU), (MemberExpression) tableAlias.Body, ((MemberExpression) column.Body).Member as PropertyInfo);
        }
        
        public BaseQueryBuilder<T, TMe> AndAll(Type tableType, MemberExpression tableAlias)
        {
            return AndAll(tableType, GetTableAlias(tableAlias));
        }

        public BaseQueryBuilder<T, TMe> AndAll(Type tableType, string tableAlias)
        {
            Append(",");
            return All(tableType, tableAlias);
        }

        public BaseQueryBuilder<T, TMe> AndAll<TU>(Expression<Func<T, TU>> tableAlias)
        {
            return AndAll(tableAlias.Body.Type, (MemberExpression) tableAlias.Body);
        }

        public BaseQueryBuilder<T, TMe> SelectCountAll()
        {
            Select();
            Append(" COUNT(*)");
            return this;
        }

        public BaseQueryBuilder<T, TMe> On<TU>(Expression<Func<T, TU>> leftColumn, Expression<Func<T, TU>> rightColumn)
        {
            currentContext = ContextEnum.Join;
            Append("ON ");
            OnField(leftColumn.Body as MemberExpression);
            query.Append("=");
            OnField(rightColumn.Body as MemberExpression);
            return this;
        }

        public BaseQueryBuilder<T, TMe> On(PropertyInfo leftColumn, string leftTableAlias, PropertyInfo rightColumn, string rightTableAlias)
        {
            currentContext = ContextEnum.Join;
            Append("ON ");
            AppendField(GetTable(leftTableAlias).name, leftColumn);
            query.Append("=");
            AppendField(GetTable(rightTableAlias).name, rightColumn);
            return this;
        }

        public BaseQueryBuilder<T, TMe> OnId<TU>(Expression<Func<T, TU>> rightColumn)
        {
            currentContext = ContextEnum.Join;
            Append("ON ");
            var memberExpression = (MemberExpression) rightColumn.Body;
            Column(memberExpression);
            query.Append("=");
            Column(memberExpression.Type, GetTableAlias(memberExpression), memberExpression.Type.GetProperty("Id"));
            return this;
        }
        
        public BaseQueryBuilder<T, TMe> On<TU>(Expression<Func<T, TU>> expression)
        {
            currentContext = ContextEnum.Join;
            Append("ON ");
            AddExpression(expression.Body);
            return this;
        }

        public BaseQueryBuilder<T, TMe> Max<TU>(Expression<Func<T, TU>> column)
        {
            return Append("MAX(").Column(column).Append(")");
        }

        public BaseQueryBuilder<T, TMe> BeginMax()
        {
            return Append("MAX(");
        }

        public BaseQueryBuilder<T, TMe> EndMax()
        {
            query.Append(')');
            return this;
        }

        /// <summary>
        /// Start a sub-select
        /// </summary>
        /// <returns></returns>
        public BaseQueryBuilder<T, TMe> BeginSub()
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
        public BaseQueryBuilder<T, TMe> EndSub()
        {
            currentContext = queryStack.Pop();
            query.Append(')');
            return this;
        }

        public BaseQueryBuilder<T, TMe> Where()
        {
            if (currentContext == ContextEnum.Unknown)
                SelectAll().From();
            Append(currentContext == ContextEnum.Where ? "AND" : "WHERE");
            currentContext = ContextEnum.Where;
            return this;
        }

        public BaseQueryBuilder<T, TMe> OrWhere()
        {
            Append(currentContext == ContextEnum.Where ? "OR" : "WHERE");
            currentContext = ContextEnum.Where;
            return this;
        }

        public BaseQueryBuilder<T, TMe> OrWhere<TU>(Expression<Func<T, TU>> expression)
        {
            OrWhere().AddExpression(expression.Body);
            return this;
        }

        /// <summary>
        /// Begins a sub-expression in a where expression (open a parenthesis)
        /// </summary>
        /// <returns></returns>
        public BaseQueryBuilder<T, TMe> BeginWhereSubExpression<TU>(Expression<Func<T, TU>> expression)
        {
            Where();
            query.Append('(');
            AddExpression(expression.Body);
            return this;
        }

        public BaseQueryBuilder<T, TMe> EndWhereSubExpression()
        {
            query.Append(')');
            return this;
        }

        public BaseQueryBuilder<T, TMe> Exists()
        {
            return Append("EXISTS");
        }

        public BaseQueryBuilder<T, TMe> NotExists()
        {
            return Append("NOT EXISTS");
        }
        
        public BaseQueryBuilder<T, TMe> Equals()
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
        public BaseQueryBuilder<T, TMe> In<TU>(IEnumerable<TU> values)
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

        public BaseQueryBuilder<T, TMe> WhereIn<TU>(Expression<Func<T, TU>> column, IEnumerable<TU> values)
        {
            return Where().Column(column).In(values);
        }

        public BaseQueryBuilder<T, TMe> Fetch(params Expression<Func<T, object>>[] fetches)
        {
            SelectAll();
            foreach (var fetch in fetches)
                AndAll(fetch);
            From();
            foreach (var fetch in fetches)
                LeftJoinOnId(fetch);
            return this;
        }

        public BaseQueryBuilder<T, TMe> Where(Expression<Func<T, TMe, bool>> expression)
        {
            Where();
            AddExpression(expression.Body);
            return this;
        }

        public BaseQueryBuilder<T, TMe> Where(Expression<Func<T, bool>> expression)
        {
            Where();
            AddExpression(expression.Body);
            return this;
        }

        public BaseQueryBuilder<T, TMe> AndOn(Expression<Func<T, bool>> expression)
        {
            Append("AND ");
            AddExpression(expression.Body);
            return this;
        }

        public BaseQueryBuilder<T, TMe> Update()
        {
            Append("UPDATE ");
            Table(typeof(T), (string) null);
            return this;
        }

        public BaseQueryBuilder<T, TMe> InsertInto()
        {
            Append("INSERT INTO");
            AppendTableName(typeof(T));
            return this;
        }

        public BaseQueryBuilder<T, TMe> GroupBy<TU>(Expression<Func<T, TU>> column)
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

        public BaseQueryBuilder<T, TMe> OrderBy<TU>(Expression<Func<T, TU>> column)
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

        public BaseQueryBuilder<T, TMe> Desc()
        {
            return Append("DESC");
        }

        public BaseQueryBuilder<T, TMe> Asc()
        {
            return Append("ASC");
        }

        public BaseQueryBuilder<T, TMe> Limit(int offset, int count)
        {
            query.Append(" LIMIT ").Append(offset).Append(",").Append(count);
            return this;
        }

        public BaseQueryBuilder<T, TMe> Limit<TU>(Expression<Func<TMe, TU>> offset, int count)
        {
            var expression = (MemberExpression)offset.Body;
            query.Append(" LIMIT @").Append(expression.Member.Name).Append(",").Append(count);
            return this;
        }

        public BaseQueryBuilder<T, TMe> Values(T value)
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

        public BaseQueryBuilder<T, TMe> SetAll(T value)
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

        public BaseQueryBuilder<T, TMe> Set<TU>(Expression<Func<T, TU>> column, Expression<Func<T, TU>> value)
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

        private MappedClass<T, TMe> baseMappedClass;


        public T Single(IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = CreateCommand(query, folkeConnection, commandParameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new Exception("No result found");
                    var value = baseMappedClass.Read(folkeConnection, typeof(T), reader);
                    reader.Close();
                    return (T)value;
                }
            }
        }

        public T SingleOrDefault(IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = CreateCommand(query, folkeConnection, commandParameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return default(T);
                    var value = baseMappedClass.Read(folkeConnection, typeof(T), reader);
                    reader.Close();
                    return (T)value;
                }
            }
        }

        public bool TryExecute(IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            try
            {
                Execute(folkeConnection, commandParameters);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public void Execute(IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = CreateCommand(query, folkeConnection, commandParameters))
            {
                command.ExecuteNonQuery();
            }
        }

        public IList<T> List(IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = CreateCommand(query, folkeConnection, commandParameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    var ret = new List<T>();
                    while (reader.Read())
                    {
                        var value = baseMappedClass.Read(folkeConnection, typeof(T), reader);
                        ret.Add((T)value);
                    }
                    reader.Close();
                    return ret;
                }
            }
        }

        /// <summary>
        /// Assumes that the result is one scalar value and nothing else, get this
        /// </summary>
        /// <typeparam name="TU">The scalar value type</typeparam>
        /// <param name="folkeConnection">A folkeConnection</param>
        /// <param name="commandParameters">Optional commandParameters if the query had commandParameters</param>
        /// <returns>A single value</returns>
        public TU Scalar<TU>(FolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = CreateCommand(query, folkeConnection, commandParameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read() || reader.IsDBNull(0))
                    {
                        return default(TU);
                    }
                    var ret = reader.GetTypedValue<TU>(0);
                    reader.Close();
                    return ret;
                }
            }
        }

        public object Scalar(FolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = CreateCommand(query, folkeConnection, commandParameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read() || reader.IsDBNull(0))
                    {
                        return null;
                    }
                    var ret = reader.GetValue(0);
                    reader.Close();
                    return ret;
                }
            }
        }

        public BaseQueryBuilder<T, TMe> Delete()
        {
            currentContext = ContextEnum.Delete;
            noAlias = true;
            return Append("DELETE");
        }

        public TU Scalar<TU>()
        {
            return Scalar<TU>(connection, parameters == null ? null : parameters.ToArray());
        }

        public object Scalar()
        {
            return Scalar(connection, parameters == null ? null : parameters.ToArray());
        }
        
        public IList<T> List()
        {
            return List(connection, parameters == null ? null : parameters.ToArray());
        }

        public void Execute()
        {
            Execute(connection, parameters == null ? null : parameters.ToArray());
        }

        public bool TryExecute()
        {
            return TryExecute(connection, parameters == null ? null : parameters.ToArray());
        }

        public T SingleOrDefault()
        {
            return SingleOrDefault(connection, parameters == null ? null : parameters.ToArray());
        }

        /// <summary>
        /// A single result
        /// </summary>
        /// <returns></returns>
        public T Single()
        {
            return Single(connection, parameters == null ? null : parameters.ToArray());
        }
    }
}