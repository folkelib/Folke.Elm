namespace Folke.Orm
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Used to create a SQL query and create objects with its results
    /// </summary>
    /// <typeparam name="T">The return type of the SQL query</typeparam>
    /// <typeparam name="TMe">A Tuple with the query parameters</typeparam>
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

        protected class TableAlias
        {
            public Type type;
            public string name;
            public string alias;
        }

        protected class FieldAlias
        {
            public PropertyInfo propertyInfo;
            public string alias;
            public int index;
        }

        protected StringBuilder query = new StringBuilder();
        private IList<FieldAlias> selectedFields;
        private IList<TableAlias> tables;
        protected TableAlias parameterTable;

        private ContextEnum currentContext = ContextEnum.Unknown;
        private Stack<ContextEnum> queryStack;
        private bool noAlias = false;
        private IList<object> parameters;

        protected readonly FolkeConnection connection;
        protected readonly IDatabaseDriver driver;
        protected readonly char beginSymbol;
        protected readonly char endSymbol;

        public BaseQueryBuilder(FolkeConnection connection)
        {
            this.connection = connection;
            this.driver = connection.Driver;
            this.beginSymbol = this.driver.BeginSymbol;
            this.endSymbol = this.driver.EndSymbol;
            this.tables = new List<TableAlias>();
        }

        public BaseQueryBuilder(IDatabaseDriver driver)
        {
            this.driver = driver;
            this.beginSymbol = driver.BeginSymbol;
            this.endSymbol = driver.EndSymbol;
            this.tables = new List<TableAlias>();
        }

        public string SQL
        {
            get
            {
                return this.query.ToString();
            }
        }

        public BaseQueryBuilder<T, TMe> Append(string sql)
        {
            if (this.query.Length != 0)
                this.query.Append(' ');
            this.query.Append(sql);
            return this;
        }

        protected TableAlias RegisterTable<U>(Expression<Func<T, U>> alias)
        {
            return this.RegisterTable(typeof(U), this.GetTableAlias(alias.Body as MemberExpression));
        }

        private string GetTableAlias(MemberExpression tableAlias)
        {
            if (tableAlias == null)
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
                else
                    tableAlias = (MemberExpression)tableAlias.Expression;
            }
            return aliasValue;
        }

        protected TableAlias RegisterTable(Type type, string tableAlias)
        {
            if (tableAlias == null)
            {
                if (this.parameterTable == null)
                {
                    this.parameterTable = new TableAlias { name = "t", alias = null, type = typeof(T) };
                    this.tables.Add(this.parameterTable);
                }
                return this.parameterTable;
            }

            var table = this.tables.SingleOrDefault(t => t.alias == tableAlias);
            if (table == null)
            {
                table = new TableAlias { name = "t" + this.tables.Count, alias = tableAlias, type = type };
                this.tables.Add(table);
            }
            return table;
        }

        protected TableAlias GetTable(string tableAlias)
        {
            return this.tables.SingleOrDefault(t => t.alias == tableAlias);
        }

        protected TableAlias GetTable(MemberExpression alias)
        {
            return this.GetTable(this.GetTableAlias(alias));
        }
        
        /// <summary>
        /// Add a field name to the query. Very low level.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnType"></param>
        /// <param name="name"></param>
        private void AppendField(string tableName, Type columnType, string name)
        {
            this.query.Append(' ');
            if (tableName != null && !this.noAlias)
            {
                this.query.Append(tableName);
                this.query.Append(".");
            }
            this.query.Append(this.beginSymbol);
            this.query.Append(name);
            if (IsForeign(columnType))
                this.query.Append("_id");
            this.query.Append(this.endSymbol);
        }

        /*private void AppendField<U>(Expression<Func<T, U>> expression)
        {
            // TODO sans doute qu'on devrait gérer x.Toto.Tutu si x.Toto est une table
            var memberExpression = expression.Body as MemberExpression;
            var table = parameterTable;
            AppendField(table.name, typeof(U), memberExpression.Member.Name);
        }*/

        /// <summary>
        /// Get a column name from a property. Allow the column name
        /// to be overloaded by an attribute
        /// </summary>
        /// <param name="property">The property</param>
        /// <returns>The column name</returns>
        private string GetColumnName(MemberInfo property)
        {
            var column = property.GetCustomAttribute<ColumnAttribute>();
            if (column != null && column.Name != null)
                return column.Name;
            return property.Name;
        }

        protected BaseQueryBuilder<T, TMe> Column(Type tableType, MemberExpression tableAlias, PropertyInfo column)
        {
            return this.Column(tableType, this.GetTableAlias(tableAlias), column);
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
            if (this.currentContext == ContextEnum.Select)
            {
                table = this.RegisterTable(tableType, tableAlias);
                if (this.selectedFields == null)
                    this.selectedFields = new List<FieldAlias>();
                this.selectedFields.Add(new FieldAlias { propertyInfo = column, alias = table.alias, index = this.selectedFields.Count });
            }
            else
            {
                table = this.GetTable(tableAlias);
                if (table == null)
                    throw new Exception("Table " + tableAlias + " not found");

                if (table.type != tableType)
                    throw new Exception("Internal error, table type " + tableType + " does not match table alias " + tableAlias + ", which had a type of " + table.type);
            }
            this.AppendField(table.name, column.PropertyType, this.GetColumnName(column));
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
            return this.Column(typeof(T), (string) null, column);
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
                return this.Column(typeof(T), (string)null, typeof(T).GetProperty("Id"));
            }
            var columnMember = (MemberExpression) column;
            
            if (columnMember.Expression is MemberExpression)
            {
                return this.Column(columnMember.Expression.Type, this.GetTableAlias(columnMember.Expression as MemberExpression), (PropertyInfo) columnMember.Member);
            }
            else
            {
                return this.Column(typeof(T), (string)null, (PropertyInfo)columnMember.Member);
            }
        }

        /// <summary>
        /// Add a column name to the query using a lambda expression. If the context is Select, the column is added
        /// to the list of selected columns.
        /// </summary>
        /// <typeparam name="U">The column type</typeparam>
        /// <param name="column">An expression that returns the column</param>
        /// <returns>The query itself</returns>
        public BaseQueryBuilder<T, TMe> Column<U>(Expression<Func<T, U>> column)
        {
            return this.Column((MemberExpression)column.Body);
        }

        /// <summary>
        /// TODO nécessaire ?
        /// </summary>
        /// <param name="expression"></param>
        private void OnField(MemberExpression expression)
        {
            if (expression.Expression is ParameterExpression)
            {
                var table = this.GetTable(expression);
                if (table != null)
                    this.AppendField(table.name, typeof(int), "Id");
                else
                    this.AppendField(this.GetTable((string)null).name, expression.Type, expression.Member.Name);
            }
            else if (expression.Expression is MemberExpression)
            {
                var accessTo = expression.Expression as MemberExpression;
                var table = this.GetTable(accessTo);
                this.AppendField(table.name, expression.Type, expression.Member.Name);
            }
            else
                throw new Exception("Must be a x.Member or x.Member.Submember");
        }

        /// <summary>
        /// Try to append a column name to the query. If the table is not selected, returns false.
        /// Do not call it from the select part of a query.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        protected bool TryColumn(MemberExpression column)
        {
            if (this.currentContext == ContextEnum.Select)
                throw new Exception("Do not call TryColumn in a Select context");

            if (column.Expression is MemberExpression)
            {
                var accessTo = column.Expression as MemberExpression;
                var table = this.GetTable(accessTo);
                if (table != null)
                {
                    this.AppendField(table.name, column.Type, this.GetColumnName(column.Member));
                    return true;
                }
                else if (column.Member.Name == "Id" && this.TryColumn(accessTo))
                    return true;
            }
            else if (column.Expression is ParameterExpression && column.Expression.Type == typeof(T) && this.parameterTable != null)
            {
                this.AppendField(this.parameterTable.name, column.Type, this.GetColumnName(column.Member));
                return true;
            }
            else if (column.Expression is ParameterExpression && column.Expression.Type == typeof(TMe))
            {
                this.query.Append("@" + column.Member.Name);
                return true;
            }
            else
            {
                var table = this.GetTable(column);
                if (table != null)
                {
                    this.AppendField(table.name, typeof(int), "Id");
                    return true;
                }
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
            return this.Columns(tableType, this.GetTableAlias(tableAlias), columns);
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
            var table = this.RegisterTable(tableType, tableAlias);
            if (this.query.Length != 0)
                this.query.Append(' ');
            bool first = true;
            if (this.selectedFields == null)
                this.selectedFields = new List<FieldAlias>();

            foreach (var column in columns)
            {
                if (IsIgnored(column.PropertyType))
                    continue;

                this.selectedFields.Add(new FieldAlias { propertyInfo = column, alias = table.alias, index = this.selectedFields.Count });

                if (first)
                    first = false;
                else
                    this.query.Append(',');
                this.AppendField(table.name, column.PropertyType, this.GetColumnName(column));
            }
            return this;
        }

        private static PropertyInfo GetExpressionPropertyInfo(Expression<Func<T, object>> column)
        {
            MemberExpression member;
            if (column.Body.NodeType == ExpressionType.Convert)
                member = ((UnaryExpression)column.Body).Operand as MemberExpression;
            else
                member = column.Body as MemberExpression;
            return member.Member as PropertyInfo;                
        }

        public BaseQueryBuilder<T, TMe> Columns(params Expression<Func<T, object>>[] column)
        {
            // TODO gérer le cas où le retour est casté depuis une value
            return this.Columns(typeof(T), (string) null, column.Select(x => GetExpressionPropertyInfo(x)));
        }

        /// <summary>
        /// Select all the table columns 
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="tableAlias"></param>
        /// <returns></returns>
        public BaseQueryBuilder<T, TMe> All(Type tableType, MemberExpression tableAlias)
        {
            return this.All(tableType, this.GetTableAlias(tableAlias));
        }

        public BaseQueryBuilder<T, TMe> All(Type tableType, string tableAlias)
        {
            return this.Columns(tableType, tableAlias, tableType.GetProperties());
        }

        public BaseQueryBuilder<T, TMe> All<U>(Expression<Func<T, U>> tableAlias)
        {
            return this.All(typeof(U), (MemberExpression) tableAlias.Body);
        }

        protected void AppendTableName(Type type)
        {
            if (this.query.Length != 0)
                this.query.Append(' ');
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            if (tableAttribute != null)
            {
                if (tableAttribute.Schema != null)
                {
                    this.query.Append(this.beginSymbol);
                    this.query.Append(tableAttribute.Schema);
                    this.query.Append(this.endSymbol);
                    this.query.Append('.');
                }

                this.query.Append(this.beginSymbol);
                this.query.Append(tableAttribute.Name);
                this.query.Append(this.endSymbol);
            }
            else
            {
                this.query.Append(this.beginSymbol);
                this.query.Append(type.Name);
                this.query.Append(this.endSymbol);
            }
        }

        protected BaseQueryBuilder<T, TMe> Table<U>(Expression<Func<T, U>> tableAlias)
        {
            return this.Table(tableAlias.Body.Type, tableAlias.Body as MemberExpression);
        }

        protected BaseQueryBuilder<T, TMe> Table(Type tableType, MemberExpression tableAlias)
        {
            return this.Table(tableType, this.GetTableAlias(tableAlias));
        }

        protected BaseQueryBuilder<T, TMe> Table(Type tableType, string tableAlias)
        {
            var table = this.RegisterTable(tableType, tableAlias);
            
            this.AppendTableName(tableType);
            if (!this.noAlias)
            {
                this.query.Append(" as ");
                this.query.Append(table.name);
            }
            return this;
        }

        private void AppendFrom()
        {
            if (this.currentContext == ContextEnum.Select || this.currentContext == ContextEnum.Delete)
            {
                this.baseMappedClass = this.MapClass(typeof(T));
                this.Append("FROM");
            }
            else if (this.currentContext == ContextEnum.From)
                this.Append(",");
            this.currentContext = ContextEnum.From;
        }

        public BaseQueryBuilder<T, TMe> From<U>(Expression<Func<U>> tableAlias)
        {
            this.AppendFrom();
            return this.Table(typeof(U), (MemberExpression) tableAlias.Body);
        }

        public BaseQueryBuilder<T, TMe> From<U>(Expression<Func<T, U>> tableAlias)
        {
            this.AppendFrom();
            return this.Table(tableAlias);
        }

        public BaseQueryBuilder<T, TMe> FromSubQuery(Func<BaseQueryBuilder<T, TMe>, BaseQueryBuilder<T, TMe>> subqueryFactory)
        {
            var builder = new BaseQueryBuilder<T, TMe>(this.driver);
            builder.parameters = this.parameters;
            var subquery = subqueryFactory.Invoke(builder);
            this.parameters = builder.parameters;
            this.AppendFrom();
            this.query.Append(" (");
            this.query.Append(subquery.query);
            this.query.Append(") AS ");

            var table = this.RegisterTable(typeof(T), null);
            this.query.Append(table.name);
            return this;
        }

        public BaseQueryBuilder<T, TMe> InnerJoinSubQuery<U>(Func<BaseQueryBuilder<T, TMe>, BaseQueryBuilder<T, TMe>> subqueryFactory, Expression<Func<U>> tableAlias)
        {
            var builder = new BaseQueryBuilder<T, TMe>(this.driver);
            builder.parameters = this.parameters;
            var subquery = subqueryFactory.Invoke(builder);
            this.parameters = builder.parameters;
            this.Append(" INNER JOIN (");
            this.query.Append(subquery.SQL);
            this.query.Append(") AS ");
            var table = this.RegisterTable(typeof(U), this.GetTableAlias(tableAlias.Body as MemberExpression));
            this.query.Append(table.name);
            return this;
        }

        public BaseQueryBuilder<T, TMe> From()
        {
            this.AppendFrom();
            return this.Table(typeof(T), (string) null);
        }

        public BaseQueryBuilder<T, TMe> AndFrom<U>(Expression<Func<T, U>> tableAlias)
        {
            this.Append(",");
            return this.Table(tableAlias);
        }

        public BaseQueryBuilder<T, TMe> LeftJoin(Type tableType, string tableAlias)
        {
            this.Append("LEFT JOIN");
            return this.Table(tableType, tableAlias);
        }

        public BaseQueryBuilder<T, TMe> LeftJoin<U>(Expression<Func<T, U>> tableAlias)
        {
            this.Append("LEFT JOIN");
            return this.Table(tableAlias);
        }

        public BaseQueryBuilder<T, TMe> RightJoin<U>(Expression<Func<T, U>> tableAlias)
        {
            this.Append("RIGHT JOIN");
            return this.Table(tableAlias);
        }

        public BaseQueryBuilder<T, TMe> LeftJoinOn<U>(Expression<Func<T, U>> column)
        {
            return this.LeftJoin(column).On(column);
        }

        public string Result
        {
            get
            {
                return this.query.ToString();
            }
        }

        public void Parameter(object parameter)
        {
            if (parameter == null && this.currentContext != ContextEnum.Set && this.currentContext != ContextEnum.Values)
            {
                var toChange = this.query.ToString().TrimEnd(' ');
                if (toChange.LastIndexOf('=') != toChange.Length - 1)
                    throw new Exception("Compare to null must be =");
                this.query.Clear();
                this.query.Append(toChange.Substring(0, toChange.Length - 1));
                this.query.Append(" IS NULL");
                return;
            }

            if (this.parameters == null)
                this.parameters = new List<object>();

            var parameterIndex = this.parameters.Count;
            if (parameter != null)
            {
                var parameterType = parameter.GetType();
                if (parameterType == typeof(TimeSpan))
                {
                    parameter = ((TimeSpan)parameter).TotalSeconds;
                }
                else if (IsForeign(parameterType))
                {
                    parameter = parameterType.GetProperty("Id").GetValue(parameter);
                    if (parameter.Equals(0))
                        throw new Exception("Id should not be 0");
                }
            }

            this.parameters.Add(parameter);
            
            this.query.Append("@Item" + parameterIndex);
        }

        protected FolkeCommand CreateCommand(IFolkeConnection connection, object[] parameters)
        {
            var command = connection.OpenCommand();
            if (parameters != null)
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameterName = "Item" + i.ToString();
                    var parameter = parameters[i];
                    var commandParameter = command.CreateParameter();
                    commandParameter.ParameterName = parameterName;
                    if (parameter == null)
                        commandParameter.Value = null;
                    else
                    {
                        var parameterType = parameter.GetType();
                        if (parameterType.IsEnum)
                            commandParameter.Value = parameterType.GetEnumName(parameter);
                        else if (parameter is IFolkeTable)
                            commandParameter.Value = ((IFolkeTable)parameter).Id;
                        else
                            commandParameter.Value = parameter;
                    }
                    command.Parameters.Add(commandParameter);
                }
            }
            command.CommandText = this.query.ToString();
            return command;
        }

        protected void AddExpression(Expression expression)
        {
            if (expression is UnaryExpression)
            {
                var unary = expression as UnaryExpression;
                switch (unary.NodeType)
                {
                    case ExpressionType.Negate:
                        this.query.Append('-');
                        break;
                    case ExpressionType.Not:
                        this.query.Append(" NOT ");
                        break;
                    case ExpressionType.Convert:
                        break;
                    default:
                        throw new Exception("ExpressionType in UnaryExpression not supported");
                }
                this.AddExpression(unary.Operand);
                return;
            }
            
            if (expression is BinaryExpression)
            {
                var binary = expression as BinaryExpression;
                this.query.Append('(');
                
                this.AddExpression(binary.Left);

                if (binary.Right.NodeType == ExpressionType.Constant && ((ConstantExpression) binary.Right).Value == null)
                {
                    if (binary.NodeType == ExpressionType.Equal)
                        this.query.Append(" IS NULL");
                    else if (binary.NodeType == ExpressionType.NotEqual)
                        this.query.Append(" IS NOT NULL");
                    else
                        throw new Exception("Operator not supported with null right member in " + binary.ToString());
                    this.query.Append(")");
                    return;
                }

                switch (binary.NodeType)
                {
                    case ExpressionType.Add:
                        this.query.Append("+");
                        break;
                    case ExpressionType.AndAlso:
                        this.query.Append(" AND ");
                        break;
                    case ExpressionType.Divide:
                        this.query.Append("/");
                        break;
                    case ExpressionType.Equal:
                        this.query.Append('=');
                        break;
                    case ExpressionType.GreaterThan:
                        this.query.Append(">");
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        this.query.Append(">=");
                        break;
                    case ExpressionType.LessThan:
                        this.query.Append("<");
                        break;
                    case ExpressionType.LessThanOrEqual:
                        this.query.Append("<=");
                        break;
                    case ExpressionType.Modulo:
                        this.query.Append("%");
                        break;
                    case ExpressionType.Multiply:
                        this.query.Append('*');
                        break;
                    case ExpressionType.OrElse:
                        this.query.Append(" OR ");
                        break;
                    case ExpressionType.Subtract:
                        this.query.Append('-');
                        break;
                    default:
                        throw new Exception("Expression type not supported");
                }

                if (binary.Right.NodeType == ExpressionType.Constant && binary.Left.NodeType == ExpressionType.Convert
                    && ((UnaryExpression)binary.Left).Operand.Type.IsEnum)
                {
                    var enumType = ((UnaryExpression)binary.Left).Operand.Type;
                    var enumIndex = (int) ((ConstantExpression)binary.Right).Value;
                    this.Parameter(enumType.GetEnumValues().GetValue(enumIndex));
                }
                else
                {
                    this.AddExpression(binary.Right);
                }
                this.query.Append(')');
                return;
            }

            if (expression is ConstantExpression)
            {
                var constant = expression as ConstantExpression;
                this.Parameter(constant.Value);
                return;
            }

            if (expression is MemberExpression)
            {
                if (this.TryColumn(expression as MemberExpression))
                    return;
            }

            if (expression.NodeType == ExpressionType.Parameter)
            {
                this.AppendField(this.parameterTable.name, typeof(int), "Id");
                return;
            }

            if (expression.NodeType == ExpressionType.Call)
            {
                var call = expression as MethodCallExpression;
                if (call.Method.DeclaringType == typeof(SqlOperator))
                {
                    switch (call.Method.Name)
                    {
                        case "Like":
                            this.AddExpression(call.Arguments[0]);
                            this.query.Append(" LIKE ");
                            this.AddExpression(call.Arguments[1]);
                            break;
                        default:
                            throw new Exception("Bizarre");
                    }
                    return;
                }
            }

            var value = System.Linq.Expressions.Expression.Lambda(expression).Compile().DynamicInvoke();
            this.Parameter(value);
        }
        
        public BaseQueryBuilder<T, TMe> SelectAll<U>(Expression<Func<T, U>> tableAlias)
        {
            this.Select();
            return this.All(tableAlias);
        }

        public BaseQueryBuilder<T, TMe> SelectAll<U>(Expression<Func<U>> tableAlias)
        {
            this.Select();
            return this.All(typeof(U), (MemberExpression) tableAlias.Body);
        }

        public BaseQueryBuilder<T, TMe> SelectAll()
        {
            this.Select();
            return this.All(typeof(T), (string) null);
        }

        public BaseQueryBuilder<T, TMe> Select()
        {
            if (this.currentContext == ContextEnum.Select)
            {
                this.Append(",");
            }
            else
            {
                this.currentContext = ContextEnum.Select;
                this.Append("SELECT");
            }
            return this;
        }
        
        public BaseQueryBuilder<T, TMe> Select(params Expression<Func<T, object>>[] column)
        {
            this.Select();
            return this.Columns(typeof(T), (string) null, column.Select(x => GetExpressionPropertyInfo(x)));
        }

        public BaseQueryBuilder<T, TMe> Select<U,V>(Expression<Func<U>> tableAlias, Expression<Func<V>> column)
        {
            this.Select();
            return this.Column(typeof(U), (MemberExpression) tableAlias.Body, ((MemberExpression) column.Body).Member as PropertyInfo);
        }
        
        public BaseQueryBuilder<T, TMe> AndAll(Type tableType, MemberExpression tableAlias)
        {
            return this.AndAll(tableType, this.GetTableAlias(tableAlias));
        }

        public BaseQueryBuilder<T, TMe> AndAll(Type tableType, string tableAlias)
        {
            this.Append(",");
            return this.All(tableType, tableAlias);
        }

        public BaseQueryBuilder<T, TMe> AndAll<U>(Expression<Func<T, U>> tableAlias)
        {
            return this.AndAll(tableAlias.Body.Type, (MemberExpression) tableAlias.Body);
        }

        public BaseQueryBuilder<T, TMe> SelectCountAll()
        {
            this.Select();
            this.Append(" COUNT(*)");
            return this;
        }

        public BaseQueryBuilder<T, TMe> On<U>(Expression<Func<T, U>> leftColumn, Expression<Func<T, U>> rightColumn)
        {
            this.currentContext = ContextEnum.Join;
            this.Append("ON ");
            this.OnField(leftColumn.Body as MemberExpression);
            this.query.Append("=");
            this.OnField(rightColumn.Body as MemberExpression);
            return this;
        }

        public BaseQueryBuilder<T, TMe> On(PropertyInfo leftColumn, string leftTableAlias, PropertyInfo rightColumn, string rightTableAlias)
        {
            this.currentContext = ContextEnum.Join;
            this.Append("ON ");
            this.AppendField(this.GetTable(leftTableAlias).name, leftColumn.PropertyType, leftColumn.Name);
            this.query.Append("=");
            this.AppendField(this.GetTable(rightTableAlias).name, rightColumn.PropertyType, rightColumn.Name);
            return this;
        }
        
        public BaseQueryBuilder<T, TMe> On<U>(Expression<Func<T, U>> rightColumn)
        {
            this.currentContext = ContextEnum.Join;
            this.Append("ON ");
            var memberExpression = (MemberExpression) rightColumn.Body;
            this.Column(memberExpression);
            this.query.Append("=");
            this.Column(memberExpression.Type, this.GetTableAlias(memberExpression), memberExpression.Type.GetProperty("Id"));
            return this;
        }

        public BaseQueryBuilder<T, TMe> Max<U>(Expression<Func<T, U>> column)
        {
            return this.Append("MAX(").Column(column).Append(")");
        }

        public BaseQueryBuilder<T, TMe> BeginMax()
        {
            return this.Append("MAX(");
        }

        public BaseQueryBuilder<T, TMe> EndMax()
        {
            this.query.Append(')');
            return this;
        }

        public BaseQueryBuilder<T, TMe> BeginSub()
        {
            if (this.queryStack == null)
                this.queryStack = new Stack<ContextEnum>();
            this.queryStack.Push(this.currentContext);
            this.currentContext = ContextEnum.Unknown;
            this.query.Append('(');
            return this;
        }

        public BaseQueryBuilder<T, TMe> EndSub()
        {
            this.currentContext = this.queryStack.Pop();
            this.query.Append(')');
            return this;
        }

        public BaseQueryBuilder<T, TMe> Where()
        {
            if (this.currentContext == ContextEnum.Unknown)
                this.SelectAll().From();
            this.Append(this.currentContext == ContextEnum.Where ? "AND" : "WHERE");
            this.currentContext = ContextEnum.Where;
            return this;
        }

        public BaseQueryBuilder<T, TMe> OrWhere()
        {
            this.Append(this.currentContext == ContextEnum.Where ? "OR" : "WHERE");
            this.currentContext = ContextEnum.Where;
            return this;
        }

        public BaseQueryBuilder<T, TMe> Exists()
        {
            return this.Append("EXISTS");
        }

        public BaseQueryBuilder<T, TMe> NotExists()
        {
            return this.Append("NOT EXISTS");
        }
        
        public BaseQueryBuilder<T, TMe> Equals()
        {
            this.query.Append('=');
            return this;
        }

        /// <summary>
        /// Add a IN operator. 
        /// Example: Where().Column(x => x.Value).In(new[]{12, 13, 14})
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public BaseQueryBuilder<T, TMe> In<U>(IEnumerable<U> values)
        {
            this.query.Append(" IN (");
            bool first = true;
            foreach (var value in values)
            {
                if (first)
                    first = false;
                else
                    this.query.Append(',');
                this.Parameter(value);
            }
            this.query.Append(')');
            return this;
        }

        public BaseQueryBuilder<T, TMe> WhereIn<U>(Expression<Func<T, U>> column, IEnumerable<U> values)
        {
            return this.Where().Column(column).In(values);
        }

        public BaseQueryBuilder<T, TMe> Fetch(params Expression<Func<T, object>>[] fetches)
        {
            this.SelectAll();
            foreach (var fetch in fetches)
                this.AndAll(fetch);
            this.From();
            foreach (var fetch in fetches)
                this.LeftJoinOn(fetch);
            return this;
        }

        public BaseQueryBuilder<T, TMe> Where(Expression<Func<T, TMe, bool>> expression)
        {
            this.Where();
            this.AddExpression(expression.Body);
            return this;
        }

        public BaseQueryBuilder<T, TMe> Where(Expression<Func<T, bool>> expression)
        {
            this.Where();
            this.AddExpression(expression.Body);
            return this;
        }

        public BaseQueryBuilder<T, TMe> AndOn(Expression<Func<T, bool>> expression)
        {
            this.Append("AND ");
            this.AddExpression(expression.Body);
            return this;
        }

        public BaseQueryBuilder<T, TMe> Update()
        {
            this.Append("UPDATE ");
            this.Table(typeof(T), (string) null);
            return this;
        }

        public BaseQueryBuilder<T, TMe> InsertInto()
        {
            this.Append("INSERT INTO");
            this.AppendTableName(typeof(T));
            return this;
        }

        public BaseQueryBuilder<T, TMe> GroupBy<U>(Expression<Func<T, U>> column)
        {
            if (this.currentContext != ContextEnum.GroupBy)
                this.Append("GROUP BY ");
            else
                this.query.Append(',');
            this.currentContext = ContextEnum.GroupBy;
            if (!this.TryColumn(column.Body as MemberExpression))
                throw new Exception(column + " is not a valid column");
            return this;
        }

        public BaseQueryBuilder<T, TMe> OrderBy<U>(Expression<Func<T, U>> column)
        {
            if (this.currentContext != ContextEnum.OrderBy)
                this.Append("ORDER BY ");
            else
                this.query.Append(',');
            this.currentContext = ContextEnum.OrderBy;
            if (!this.TryColumn(column.Body as MemberExpression))
                throw new Exception(column + " is not a valid column");
            return this;
        }

        public BaseQueryBuilder<T, TMe> Desc()
        {
            return this.Append("DESC");
        }

        public BaseQueryBuilder<T, TMe> Asc()
        {
            return this.Append("ASC");
        }

        public BaseQueryBuilder<T, TMe> Limit(int offset, int count)
        {
            this.query.Append(" LIMIT ").Append(offset).Append(",").Append(count);
            return this;
        }

        public BaseQueryBuilder<T, TMe> Limit<U>(Expression<Func<TMe, U>> offset, int count)
        {
            var expression = offset.Body as MemberExpression;
            this.query.Append(" LIMIT @").Append(expression.Member.Name).Append(",").Append(count);
            return this;
        }

        public BaseQueryBuilder<T, TMe> Values(T value)
        {
            this.currentContext = ContextEnum.Values;
            this.query.Append(" (");
            bool first = true;
            var type = value.GetType();
            foreach (var property in type.GetProperties())
            {
                if (IsIgnored(property.PropertyType) || IsReadOnly(property))
                    continue;
                if (first)
                    first = false;
                else
                    this.query.Append(",");
                this.AppendField(null, property.PropertyType, this.GetColumnName(property));
            }
            this.query.Append(") VALUES (");
            first = true;
            foreach (var property in type.GetProperties())
            {
                if (IsIgnored(property.PropertyType) || IsReadOnly(property))
                    continue;
                if (first)
                    first = false;
                else
                    this.query.Append(",");
                this.Parameter(property.GetValue(value));
            }
            this.query.Append(")");
            return this;
        }

        public BaseQueryBuilder<T, TMe> SetAll(T value)
        {
            this.Append("SET ");
            this.currentContext = ContextEnum.Set;
            var type = value.GetType();
            bool first = true;
            var table = this.parameterTable;
            foreach (var property in type.GetProperties())
            {
                if (IsIgnored(property.PropertyType) || IsReadOnly(property))
                    continue;

                if (first)
                    first = false;
                else
                    this.query.Append(",");
                this.AppendField(table.name, property.PropertyType, this.GetColumnName(property));
                this.query.Append("=");
                this.Parameter(property.GetValue(value));
            }
            return this;
        }

        public BaseQueryBuilder<T, TMe> Set<U>(Expression<Func<T, U>> column, Expression<Func<T, U>> value)
        {
            if (this.currentContext == ContextEnum.Set)
            {
                this.Append(", ");
            }
            else
            {
                this.Append("SET ");
                this.currentContext = ContextEnum.Set;
            }

            this.Column(column);
            this.Append("=");
            this.AddExpression(value.Body);
            return this;
        }

        private class MappedField
        {
            public FieldAlias selectedField;
            public PropertyInfo propertyInfo;
            public MappedClass mappedClass;
        }

        private class MappedCollection
        {
            public string[] listJoins;
            public ConstructorInfo listConstructor;
            public PropertyInfo propertyInfo;
        }

        private class MappedClass
        {
            public IList<MappedField> fields = new List<MappedField>();
            public IList<MappedCollection> collections;
            public MappedField idField;
            public ConstructorInfo constructor;

            public object Construct(IFolkeConnection connection, Type type, int id)
            {
                var ret = this.constructor.Invoke(null);
                
                if (this.idField != null)
                    this.idField.propertyInfo.SetValue(ret, id);

                if (this.collections != null)
                {
                    foreach (var collection in this.collections)
                    {
                        collection.propertyInfo.SetValue(ret, collection.listConstructor.Invoke(new object[] { connection, type, id, collection.listJoins }));
                    }
                }
                return ret;
            }
        }

        private MappedClass baseMappedClass;
        
        private MappedClass MapClass(Type type, string alias = null)
        {
            if (this.selectedFields == null)
                return null;

            var mappedClass = new MappedClass();

            var idProperty = type.GetProperty("Id");
            mappedClass.constructor = type.GetConstructor(Type.EmptyTypes);
            if (idProperty != null)
            {
                var selectedField = this.selectedFields.SingleOrDefault(f => f.alias == alias && f.propertyInfo == idProperty);
                mappedClass.idField = new MappedField { selectedField = selectedField, propertyInfo = idProperty };
            }
            
            foreach (var property in type.GetProperties())
            {
                if (property.Name == "Id")
                    continue;

                var propertyType = property.PropertyType;
                if (Nullable.GetUnderlyingType(propertyType) != null)
                {
                    propertyType = Nullable.GetUnderlyingType(propertyType);
                }

                
                if (propertyType.IsGenericType)
                {
                    var foreignType = propertyType.GenericTypeArguments[0];
                    var folkeList = typeof(FolkeList<>).MakeGenericType(foreignType);
                    if (property.PropertyType.IsAssignableFrom(folkeList))
                    {
                        var joins = property.GetCustomAttributes<FolkeListAttribute>().Select(x => x.Join).ToArray();
                        var constructor = folkeList.GetConstructor(new[] { typeof(IFolkeConnection), typeof(Type), typeof(int), typeof(string[]) });
                        var mappedCollection = new MappedCollection { propertyInfo = property, listJoins = joins, listConstructor = constructor };
                        if (mappedClass.collections == null)
                            mappedClass.collections = new List<MappedCollection>();
                        mappedClass.collections.Add(mappedCollection);
                    }
                }
                else if (!IsIgnored(propertyType))
                {
                    var fieldInfo = this.selectedFields.SingleOrDefault(f => f.alias == alias && f.propertyInfo == property);
                    bool isForeign = IsForeign(property.PropertyType);
                    if (fieldInfo != null || (isForeign && (mappedClass.idField == null || mappedClass.idField.selectedField != null)))
                    {
                        var mappedField = new MappedField { propertyInfo = property, selectedField = fieldInfo };

                        if (IsForeign(property.PropertyType))
                        {
                            mappedField.mappedClass = this.MapClass(property.PropertyType, alias == null ? property.Name : alias + "." + property.Name);
                        }
                        mappedClass.fields.Add(mappedField);
                    }
                }
            }
            return mappedClass;
        }
        
        private object Read(IFolkeConnection connection, Type type, DbDataReader reader, MappedClass mappedClass, int expectedId = 0)
        {
            var cache = connection.Cache;
            object value = null;
            var idMappedField = mappedClass.idField;
            
            if (idMappedField != null)
            {
                if (!cache.ContainsKey(type.Name))
                    cache[type.Name] = new Dictionary<int, object>();
                var typeCache = cache[type.Name];

                int id;

                if (idMappedField.selectedField != null)
                {
                    var index = idMappedField.selectedField.index;

                    if (expectedId == 0 && reader.IsDBNull(index))
                        return null;

                    id = reader.GetInt32(index);
                    if (expectedId != 0 && id != expectedId)
                        throw new Exception("Unexpected id");
                }
                else
                {
                    if (expectedId == 0)
                        throw new Exception("Id can not be 0");

                    id = expectedId;
                }

                if (typeCache.ContainsKey(id))
                {
                    value = typeCache[id];
                }
                else
                {
                    value = mappedClass.Construct(connection, type, id);
                    typeCache[id] = value;
                }
            }
            else
            {
                value = mappedClass.Construct(connection, type, 0);
            }

            foreach (var mappedField in mappedClass.fields)
            {
                var fieldInfo = mappedField.selectedField;
                
                if (fieldInfo != null && reader.IsDBNull(fieldInfo.index))
                    continue;
                
                if (mappedField.mappedClass == null)
                {
                    object field = reader.GetTypedValue(mappedField.propertyInfo.PropertyType, fieldInfo.index);
                    mappedField.propertyInfo.SetValue(value, field);
                }
                else 
                {
                    int id = fieldInfo == null ? 0 : reader.GetInt32(fieldInfo.index);
                    object other = this.Read(connection, mappedField.propertyInfo.PropertyType, reader, mappedField.mappedClass, id);
                    mappedField.propertyInfo.SetValue(value, other);
                }
            }
            return value;
        }


        public T Single(IFolkeConnection connection, params object[] parameters)
        {
            using (var command = this.CreateCommand(connection, parameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new Exception("No result found");
                    var value = this.Read(connection, typeof(T), reader, this.baseMappedClass);
                    reader.Close();
                    return (T)value;
                }
            }
        }

        public T SingleOrDefault(IFolkeConnection connection, params object[] parameters)
        {
            using (var command = this.CreateCommand(connection, parameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return default(T);
                    var value = this.Read(connection, typeof(T), reader, this.baseMappedClass);
                    reader.Close();
                    return (T)value;
                }
            }
        }

        public bool TryExecute(IFolkeConnection connection, params object[] parameters)
        {
            try
            {
                this.Execute(connection, parameters);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public void Execute(IFolkeConnection connection, params object[] parameters)
        {
            using (var command = this.CreateCommand(connection, parameters))
            {
                command.ExecuteNonQuery();
            }
        }

        public IList<T> List(IFolkeConnection connection, params object[] parameters)
        {
            using (var command = this.CreateCommand(connection, parameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    var ret = new List<T>();
                    while (reader.Read())
                    {
                        var value = this.Read(connection, typeof(T), reader, this.baseMappedClass);
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
        /// <typeparam name="U">The scalar value type</typeparam>
        /// <param name="connection">A connection</param>
        /// <param name="parameters">Optional parameters if the query had parameters</param>
        /// <returns>A single value</returns>
        public U Scalar<U>(FolkeConnection connection, params object[] parameters)
        {
            using (var command = this.CreateCommand(connection, parameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read() || reader.IsDBNull(0))
                    {
                        return default(U);
                    }
                    var ret = reader.GetTypedValue<U>(0);
                    reader.Close();
                    return ret;
                }
            }
        }

        // TODO move this elsewhere
        protected static bool IsReadOnly(PropertyInfo property)
        {
            return property.Name == "Id";
        }

        // TODO move this elsewhere
        public static bool IsForeign(Type type)
        {
            return type.GetInterface("IFolkeTable") != null;
        }

        // TODO move this elsewhere
        public static bool IsIgnored(Type type)
        {
            return type.IsGenericType;
        }

        public BaseQueryBuilder<T, TMe> Delete()
        {
            this.currentContext = ContextEnum.Delete;
            this.noAlias = true;
            return this.Append("DELETE");
        }

        public U Scalar<U>()
        {
            return this.Scalar<U>(this.connection, this.parameters == null ? null : this.parameters.ToArray());
        }

        public IList<T> List()
        {
            return this.List(this.connection, this.parameters == null ? null : this.parameters.ToArray());
        }

        public void Execute()
        {
            this.Execute(this.connection, this.parameters == null ? null : this.parameters.ToArray());
        }

        public bool TryExecute()
        {
            return this.TryExecute(this.connection, this.parameters == null ? null : this.parameters.ToArray());
        }

        public T SingleOrDefault()
        {
            return this.SingleOrDefault(this.connection, this.parameters == null ? null : this.parameters.ToArray());
        }

        /// <summary>
        /// A single result
        /// </summary>
        /// <returns></returns>
        public T Single()
        {
            return this.Single(this.connection, this.parameters == null ? null : this.parameters.ToArray());
        }
    }
}