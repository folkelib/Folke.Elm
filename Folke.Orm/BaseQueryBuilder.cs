using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Folke.Orm
{
    public class BaseQueryBuilder<T> : BaseQueryBuilder
    {
        public BaseQueryBuilder(FolkeConnection connection)
            : base(connection, typeof(T))
        {
        }

        public BaseQueryBuilder()
        {
            defaultType = typeof(T);
        }

        protected BaseQueryBuilder(IDatabaseDriver databaseDriver)
            : base(databaseDriver)
        {
            defaultType = typeof(T);
        }
    }

    public class BaseQueryBuilder
    {
        protected enum ContextEnum
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

        protected SqlStringBuilder query;
        protected IList<object> parameters;
        protected readonly FolkeConnection connection;
        protected MappedClass baseMappedClass;
        protected IDatabaseDriver driver;
        protected IList<FieldAlias> selectedFields;
        protected IList<TableAlias> tables;
        protected TableAlias defaultTable;
        protected Type defaultType;
        protected Type parametersType;
        protected ContextEnum currentContext = ContextEnum.Unknown;
        protected bool noAlias;

        internal TableAlias DefaultTable { get { return defaultTable; } }
        internal IList<FieldAlias> SelectedFields { get { return selectedFields; } }

        public BaseQueryBuilder(FolkeConnection connection, Type type = null):this(connection.Driver)
        {
            defaultType = type;
            this.connection = connection;
        }

        public BaseQueryBuilder(IDatabaseDriver databaseDriver, Type defaultType, Type parametersType = null)
            : this(databaseDriver)
        {
            this.defaultType = defaultType;
            this.parametersType = parametersType;
        }

        public BaseQueryBuilder(IDatabaseDriver databaseDriver):this()
        {
            driver = databaseDriver;
            query = driver.CreateSqlStringBuilder();
        }

        public BaseQueryBuilder(BaseQueryBuilder parentBuilder):this(parentBuilder.Driver)
        {
            if (parentBuilder.parameters == null)
                parentBuilder.parameters = new List<object>();
            parameters = parentBuilder.parameters;
            tables = parentBuilder.tables;
            defaultTable = parentBuilder.defaultTable;
            defaultType = parentBuilder.defaultType;
        }

        public BaseQueryBuilder()
        {
            tables = new List<TableAlias>();
        }

        public string Sql
        {
            get
            {
                return query.ToString();
            }
        }

        internal FolkeConnection Connection
        {
            get { return connection; }
        }

        internal IDatabaseDriver Driver
        {
            get
            {
                return driver;
            }
        }

        public object[] Parameters
        {
            get { return parameters == null ? null : parameters.ToArray(); }
        }

        internal MappedClass MappedClass
        {
            get { return baseMappedClass; }
        }

        internal void AppendTableName(Type type)
        {
            query.AppendSpace();
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            if (tableAttribute != null)
            {
                if (tableAttribute.Schema != null)
                {
                    query.AppendSymbol(tableAttribute.Schema);
                    query.Append('.');
                }

                query.AppendSymbol(tableAttribute.Name);
            }
            else
            {
                query.AppendSymbol(type.Name);
            }
        }

        internal string GetTableAlias(Expression tableExpression)
        {
            if (tableExpression.NodeType == ExpressionType.Parameter)
            {
                if (tableExpression.Type != defaultType)
                    throw new Exception("Internal error");
                return null;
            }
            
            var tableAlias = tableExpression;
            string aliasValue = null;
            while (tableAlias != null && tableAlias.NodeType != ExpressionType.Parameter)
            {
                switch (tableAlias.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        var memberExpression = (MemberExpression) tableAlias;
                        if (aliasValue != null)
                            aliasValue = memberExpression.Member.Name + "." + aliasValue;
                        else
                            aliasValue = memberExpression.Member.Name;
                        tableAlias = memberExpression.Expression;
                        break;
                    case ExpressionType.Constant:
                        if (aliasValue != null)
                            aliasValue = tableAlias + "." + aliasValue;
                        else
                            aliasValue = tableAlias.ToString();
                        return aliasValue;
                    case ExpressionType.Call:
                        var callExpression = (MethodCallExpression) tableAlias;
                        if (aliasValue != null)
                            aliasValue = callExpression.Method.Name + "()." + aliasValue;
                        else
                            aliasValue = callExpression.Method.Name + "()";
                        tableAlias = callExpression.Object;
                        break;
                    case ExpressionType.Convert:
                        var convertExpression = (UnaryExpression) tableAlias;
                        tableAlias = convertExpression.Operand;
                        break;
                    default:
                        throw new Exception("Unexpected node type " + tableAlias.NodeType);
                }
            }
            return aliasValue;
        }

        internal protected TableAlias GetTable(string tableAlias)
        {
            return tables.SingleOrDefault(t => t.alias == tableAlias);
        }

        internal protected TableAlias GetTable(Expression alias)
        {
            return GetTable(GetTableAlias(alias));
        }

        internal protected TableAlias GetTable(Expression aliasExpression, bool register)
        {
            var alias = GetTableAlias(aliasExpression);
            if (register)
                return RegisterTable(aliasExpression.Type, alias);
            return GetTable(alias);
        }

        /// <summary>
        /// Add a field name to the query. Very low level.
        /// </summary>
        /// <param name="tableName">The table alias</param>
        /// <param name="propertyInfo">The property info of the column</param>
        internal void AppendColumn(string tableName, MemberInfo propertyInfo)
        {
            query.Append(' ');
            if (tableName != null && !noAlias)
            {
                query.AppendSymbol(tableName);
                query.Append(".");
            }
            query.AppendSymbol(TableHelpers.GetColumnName(propertyInfo));
        }

        internal void AppendColumn(TableColumn tableColumn)
        {
            AppendColumn(tableColumn.Table.name, tableColumn.Column);
        }

        internal void AppendColumn(Expression expression, bool registerTable = false)
        {
            AppendColumn(ExpressionToColumn(expression, registerTable));
        }

        internal void AppendParameter(object parameter)
        {
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
            
            query.Append(" @Item" + parameterIndex);
        }

        internal void AddExpression(Expression expression, bool registerTable = false)
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
                AddExpression(unary.Operand, registerTable);
                return;
            }
            
            if (expression is BinaryExpression)
            {
                var binary = expression as BinaryExpression;
                query.Append('(');

                AddExpression(binary.Left, registerTable);

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
                    AppendParameter(enumType.GetEnumValues().GetValue(enumIndex));
                }
                else
                {
                    AddExpression(binary.Right, registerTable);
                }
                query.Append(')');
                return;
            }

            if (expression is ConstantExpression)
            {
                var constant = expression as ConstantExpression;
                AppendParameter(constant.Value);
                return;
            }

            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = (MemberExpression) expression;
                if (memberExpression.Expression.Type == parametersType)
                {
                    query.Append(" @" + memberExpression.Member.Name);
                    return;
                }
            }

            var column = ExpressionToColumn(expression, registerTable);
            if (column != null)
            {
                AppendColumn(column);
                return;
            }

            if (expression.NodeType == ExpressionType.Call)
            {
                var call = (MethodCallExpression)expression;
                
                if (call.Method.DeclaringType == typeof(ExpressionHelpers))
                {
                    switch (call.Method.Name)
                    {
                        case "Like":
                            AddExpression(call.Arguments[0], registerTable);
                            query.Append(" LIKE");
                            AddExpression(call.Arguments[1], registerTable);
                            break;
                        case "In":
                            AddExpression(call.Arguments[0], registerTable);
                            query.Append(" IN");
                            AppendValues((IEnumerable)Expression.Lambda(call.Arguments[1]).Compile().DynamicInvoke());
                            break;
                        default:
                            throw new Exception("Unsupported expression helper");
                    }
                    return;
                }

                if (call.Method.DeclaringType == typeof(SqlFunctions))
                {
                    switch (call.Method.Name)
                    {
                        case "LastInsertedId":
                            query.AppendLastInsertedId();
                            break;
                        case "Max":
                            query.Append(" MAX(");
                            AddExpression(call.Arguments[0], registerTable);
                            query.Append(")");
                            break;
                        default:
                            throw new Exception("Unsupported sql function");
                    }
                    return;
                }

                if (call.Method.DeclaringType == typeof(string))
                {
                    switch (call.Method.Name)
                    {
                        case "StartsWith":
                            AddExpression(call.Object, registerTable);
                            query.Append(" LIKE");
                            var text = (string) Expression.Lambda(call.Arguments[0]).Compile().DynamicInvoke();
                            text = text.Replace("\\", "\\\\").Replace("%", "\\%") + "%";
                            AppendParameter(text);
                            break;
                        default:
                            throw new Exception("Unsupported string method");
                    }
                    return;
                }

                if (call.Method.Name == "Equals")
                {
                    query.Append('(');
                    AddExpression(call.Object, registerTable);
                    query.Append('=');
                    AddExpression(call.Arguments[0], registerTable);
                    query.Append(')');
                    return;
                }
            }

            var value = Expression.Lambda(expression).Compile().DynamicInvoke();
            AppendParameter(value);
        }

        private void AppendValues(IEnumerable values)
        {
            query.Append("(");
            bool first = true;
            foreach (var value in values)
            {
                if (first)
                    first = false;
                else
                    query.Append(',');
                AppendParameter(value);
            }
            query.Append(')');
        }

        internal TableAlias RegisterTable(Type type, string tableAlias)
        {
            if (tableAlias == null)
            {
                if (defaultTable == null)
                {
                    defaultTable = new TableAlias { name = "t", alias = null, type = defaultType };
                    tables.Add(defaultTable);
                }
                return defaultTable;
            }

            var table = tables.SingleOrDefault(t => t.alias == tableAlias);
            if (table == null)
            {
                table = new TableAlias { name = "t" + tables.Count, alias = tableAlias, type = type };
                tables.Add(table);
            }
            return table;
        }

        internal void SelectField(TableColumn column)
        {
            if (selectedFields == null)
                selectedFields = new List<FieldAlias>();
            selectedFields.Add(new FieldAlias {propertyInfo = column.Column, tableAlias = column.Table == null ? null : column.Table.alias, index = selectedFields.Count});
        }

        internal void SelectField(Expression column)
        {
            SelectField(ExpressionToColumn(column, registerDefaultTable: true));
        }

        internal TableColumn ExpressionToColumn(Expression columnExpression, bool registerDefaultTable = false)
        {
            if (columnExpression.NodeType == ExpressionType.Convert)
            {
                columnExpression = ((UnaryExpression) columnExpression).Operand;
            }

            if (columnExpression.NodeType == ExpressionType.Parameter)
            {
                return new TableColumn {Column = TableHelpers.GetKey(defaultType), Table = defaultTable };
            }

            if (columnExpression.NodeType == ExpressionType.Call)
            {
                var callExpression = (MethodCallExpression)columnExpression;
                if (callExpression.Method.DeclaringType == typeof (ExpressionHelpers) &&
                    callExpression.Method.Name == "Property")
                {
                    var propertyInfo = (PropertyInfo)Expression.Lambda(callExpression.Arguments[1]).Compile().DynamicInvoke();
                    return new TableColumn {Column = propertyInfo, Table = GetTable(callExpression.Arguments[0], registerDefaultTable)};
                }
                return null;
            }

            
            if (columnExpression.NodeType != ExpressionType.MemberAccess)
            {
                return null;
            }

            var columnMember = (MemberExpression)columnExpression;
            
            var memberExpression = columnMember.Expression as MemberExpression;
            if (memberExpression != null)
            {
                var table = GetTable(memberExpression, registerDefaultTable);
                if (table == null)
                {
                    if (columnMember.Member == TableHelpers.GetKey(memberExpression.Type))
                    {
                        return ExpressionToColumn(memberExpression);
                    }
                    return null;
                }

                return new TableColumn {Column = columnMember.Member, Table = table };
            }

            var parameterExpression = columnMember.Expression as ParameterExpression;
            if (parameterExpression != null && parameterExpression.Type == defaultType)
            {
                if (defaultTable == null)
                {
                    if (registerDefaultTable)
                    {
                        defaultTable = RegisterTable(defaultType, null);
                    }
                    else
                    {
                        var table = GetTable(columnExpression);
                        if (table != null)
                        {
                            return new TableColumn { Column = TableHelpers.GetKey(table.type), Table = table };
                        }
                        return null;
                    }
                }
                return new TableColumn {Column = columnMember.Member, Table = defaultTable};
            }
            return null;
        }

        /// <summary>
        /// Get the key column from a table expression.
        /// Example: x => x.Identity will returns the Id column from the Identity table
        /// </summary>
        /// <param name="tableExpression">The expression</param>
        /// <returns></returns>
        internal TableColumn GetTableKey(Expression tableExpression)
        {
            var table = GetTable(tableExpression);
            return new TableColumn {Column = TableHelpers.GetKey(table.type), Table = table};
        }
        
        internal void AppendSelectedColumn(TableColumn column)
        {
            SelectField(column);
            AppendColumn(column);
        }

        /// <summary>
        /// Append the selected columns of a table to a select expression
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="tableAlias"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        internal TableAlias AppendSelectedColumns(Type tableType, string tableAlias, IEnumerable<PropertyInfo> columns)
        {
            var table = RegisterTable(tableType, tableAlias);
            query.AppendSpace();
            bool first = true;
            if (selectedFields == null)
                selectedFields = new List<FieldAlias>();

            foreach (var column in columns)
            {
                if (TableHelpers.IsIgnored(column.PropertyType))
                    continue;

                selectedFields.Add(new FieldAlias { propertyInfo = column, tableAlias = table.alias, index = selectedFields.Count });

                if (first)
                    first = false;
                else
                    query.Append(',');
                AppendColumn(table.name, column);
            }
            return table;
        }

        internal void AppendFrom()
        {
            if (currentContext == ContextEnum.Select || currentContext == ContextEnum.Delete)
            {
                baseMappedClass = MappedClass.MapClass(selectedFields, defaultType);
                query.Append(" FROM");
            }
            else if (currentContext == ContextEnum.From)
                query.Append(",");
            currentContext = ContextEnum.From;
        }

        internal void Append(string sql)
        {
            query.AppendSpace();
            query.Append(sql);
        }

        internal void AppendDelete()
        {
            noAlias = true;
            query.Append("DELETE");
            currentContext = ContextEnum.Delete;
        }

        internal void Where()
        {
            Append(currentContext == ContextEnum.Where ? "AND" : "WHERE");
            currentContext = ContextEnum.Where;
        }

        internal void AppendAllSelects(Type tableType, string tableAlias)
        {
            AppendSelectedColumns(tableType, tableAlias, tableType.GetProperties());
        }

        /// <summary>
        /// Begins a select command
        /// </summary>
        /// <returns>The query builder</returns>
        internal void AppendSelect()
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
        }

        public class TableAlias
        {
            public Type type;
            public string name;
            public string alias;
        }

        public class FieldAlias
        {
            public MemberInfo propertyInfo;
            public string tableAlias;
            public int index;
        }

        public class TableColumn
        {
            public TableAlias Table { get; set; }
            public MemberInfo Column { get; set; }
        }

        internal void AppendTable(Expression tableExpression)
        {
            AppendTable(tableExpression.Type, tableExpression);
        }

        internal void AppendTable(Type tableType, Expression tableAlias)
        {
            AppendTable(tableType, GetTableAlias(tableAlias));
        }

        internal void AppendTable(Type tableType, string tableAlias)
        {
            var table = RegisterTable(tableType, tableAlias);
            
            AppendTableName(tableType);
            if (!noAlias)
            {
                query.Append(" as ");
                query.Append(table.name);
            }
        }

        internal TableAlias RegisterTable()
        {
            return RegisterTable(null, null);
        }

        internal void AppendOrderBy()
        {
            if (currentContext != ContextEnum.OrderBy)
            {
                Append("ORDER BY ");
            }
            else
            {
                query.Append(',');
            }
            currentContext = ContextEnum.OrderBy;
        }

        internal void AppendGroupBy()
        {
            if (currentContext != ContextEnum.GroupBy)
            {
                Append("GROUP BY ");
            }
            else
            {
                query.Append(',');
            }
            currentContext = ContextEnum.GroupBy;
        }

        internal void AppendSet()
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
        }

        public void AppendInParenthesis(string sql)
        {
            query.Append(" (");
            query.Append(sql);
            query.Append(')');
        }
    }
}