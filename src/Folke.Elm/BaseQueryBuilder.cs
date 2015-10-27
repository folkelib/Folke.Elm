using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Folke.Elm.Mapping;

namespace Folke.Elm
{
    public class BaseQueryBuilder<T> : BaseQueryBuilder, IQueryableCommand<T>
    {
        public BaseQueryBuilder(FolkeConnection connection)
            : base(connection, typeof(T))
        {
        }

        protected BaseQueryBuilder(IDatabaseDriver databaseDriver, IMapper mapper)
            : base(databaseDriver, mapper, typeof(T))
        {
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.Enumerate().GetEnumerator();
        }
    }

    public class BaseQueryBuilder : IQueryableCommand
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
            GroupBy,

            /// <summary>
            /// In the middle of a WhereExpression in parenthesis
            /// </summary>
            WhereExpression
        }

        protected SqlStringBuilder query;
        protected IList<object> parameters;
        protected readonly FolkeConnection connection;
        protected MappedClass baseMappedClass;
        protected IDatabaseDriver driver;
        protected IList<FieldAlias> selectedFields;
        protected IList<TableAlias> tables;
        protected TableAlias defaultTable;
        protected TypeMapping defaultType;
        protected Type parametersType;
        protected ContextEnum currentContext = ContextEnum.Unknown;
        protected bool noAlias;

        internal TableAlias DefaultTable => defaultTable;
        internal IList<FieldAlias> SelectedFields => selectedFields;

        public BaseQueryBuilder(FolkeConnection connection, Type type = null, Type parametersType = null)
            : this(connection.Driver, connection.Mapper, type, parametersType)
        {
            this.connection = connection;
        }

        public BaseQueryBuilder(IDatabaseDriver databaseDriver, IMapper mapper, Type defaultType, Type parametersType = null)
            : this(databaseDriver, mapper)
        {
            this.defaultType = Mapper.GetTypeMapping(defaultType);
            this.parametersType = parametersType;
        }

        public BaseQueryBuilder(IDatabaseDriver databaseDriver, IMapper mapper):this(databaseDriver.CreateSqlStringBuilder())
        {
            Mapper = mapper;
            driver = databaseDriver;
        }

        public IMapper Mapper { get; set; }

        public BaseQueryBuilder(BaseQueryBuilder parentBuilder):this(parentBuilder.Driver, parentBuilder.Mapper)
        {
            if (parentBuilder.parameters == null)
                parentBuilder.parameters = new List<object>();
            parameters = parentBuilder.parameters;
            tables = parentBuilder.tables;
            defaultTable = parentBuilder.defaultTable;
            defaultType = parentBuilder.defaultType;
        }

        public BaseQueryBuilder(SqlStringBuilder stringBuilder = null)
        {
            query = stringBuilder ?? new SqlStringBuilder();
            tables = new List<TableAlias>();
        }

        public string Sql => query.ToString();

        public IFolkeConnection Connection => connection;

        internal IDatabaseDriver Driver => driver;

        public object[] Parameters => parameters?.ToArray();

        public MappedClass MappedClass => baseMappedClass;

        internal void AppendTableName(TypeMapping type)
        {
            query.AppendSpace();
            if (type.TableSchema != null)
            {
                query.AppendSymbol(type.TableSchema);
                query.Append('.');
            }

            query.AppendSymbol(type.TableName);
        }

        internal string GetTableAlias(Expression tableExpression)
        {
            if (tableExpression.NodeType == ExpressionType.Parameter)
            {
                if (tableExpression.Type != defaultType.Type)
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
        /// <param name="propertyMapping">The property info of the column</param>
        internal void AppendColumn(string tableName, PropertyMapping propertyMapping)
        {
            query.Append(' ');
            if (tableName != null && !noAlias)
            {
                query.AppendSymbol(tableName);
                query.Append(".");
            }
            query.AppendSymbol(propertyMapping.ColumnName);
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
                else if (Mapper.IsMapped(parameterType))
                {
                    var key = Mapper.GetTypeMapping(parameterType).Key;
                    parameter = key.PropertyInfo.GetValue(parameter);
                    if (parameter.Equals(0))
                        throw new Exception("Id should not be 0");
                }
            }

            parameters.Add(parameter);
            
            query.Append(" @Item" + parameterIndex);
        }

        internal void AddExpression(Expression expression, bool registerTable = false)
        {
            var unary = expression as UnaryExpression;
            if (unary != null)
            {
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

            var binary = expression as BinaryExpression;
            if (binary != null)
            {
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
                    case ExpressionType.NotEqual:
                        query.Append("<>");
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
                    && ((UnaryExpression)binary.Left).Operand.Type.GetTypeInfo().IsEnum)
                {
                    var enumType = ((UnaryExpression)binary.Left).Operand.Type;
                    var enumIndex = (int) ((ConstantExpression)binary.Right).Value;
                    AppendParameter(Enum.GetValues(enumType).GetValue(enumIndex));
                }
                else
                {
                    AddExpression(binary.Right, registerTable);
                }
                query.Append(')');
                return;
            }

            var constant = expression as ConstantExpression;
            if (constant != null)
            {
                AppendParameter(constant.Value);
                return;
            }

            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = (MemberExpression) expression;
                if (memberExpression.Expression != null && memberExpression.Expression.Type == parametersType)
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
                        case nameof(ExpressionHelpers.Like):
                            AddExpression(call.Arguments[0], registerTable);
                            query.Append(" LIKE");
                            AddExpression(call.Arguments[1], registerTable);
                            break;
                        case nameof(ExpressionHelpers.In):
                            AddExpression(call.Arguments[0], registerTable);
                            query.Append(" IN");
                            AppendValues((IEnumerable)Expression.Lambda(call.Arguments[1]).Compile().DynamicInvoke());
                            break;
                        case nameof(ExpressionHelpers.Between):
                            AddExpression(call.Arguments[0], registerTable);
                            query.Append(" BETWEEN ");
                            AddExpression(call.Arguments[1], registerTable);
                            query.Append(" AND ");
                            AddExpression(call.Arguments[2], registerTable);
                            break;
                        default:
                            throw new Exception("Unsupported expression helper");
                    }
                    return;
                }

                if (call.Method.DeclaringType == typeof(Math))
                {
                    switch (call.Method.Name)
                    {
                        case nameof(Math.Abs):
                            query.Append(" ABS(");
                            AddExpression(call.Arguments[0], registerTable);
                            query.Append(")");
                            break;

                        case nameof(Math.Cos):
                            query.Append(" COS(");
                            AddExpression(call.Arguments[0], registerTable);
                            query.Append(")");
                            break;

                        case nameof(Math.Sin):
                            query.Append(" SIN(");
                            AddExpression(call.Arguments[0], registerTable);
                            query.Append(")");
                            break;
                    }
                }

                if (call.Method.DeclaringType == typeof(SqlFunctions))
                {
                    switch (call.Method.Name)
                    {
                        case nameof(SqlFunctions.LastInsertedId):
                            query.AppendLastInsertedId();
                            break;
                        case nameof(SqlFunctions.Max):
                            query.Append(" MAX(");
                            AddExpression(call.Arguments[0], registerTable);
                            query.Append(")");
                            break;
                        case nameof(SqlFunctions.Sum):
                            query.Append(" SUM(");
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
                        case nameof(string.StartsWith):
                        {
                            AddExpression(call.Object, registerTable);
                            query.Append(" LIKE");
                            var text = (string)Expression.Lambda(call.Arguments[0]).Compile().DynamicInvoke();
                            text = text.Replace("\\", "\\\\").Replace("%", "\\%") + "%";
                            AppendParameter(text);
                            break;
                        }
                        case nameof(string.Contains):
                        {
                            AddExpression(call.Object, registerTable);
                            query.Append(" LIKE");
                            var text = (string)Expression.Lambda(call.Arguments[0]).Compile().DynamicInvoke();
                            text = "%" + text.Replace("\\", "\\\\").Replace("%", "\\%") + "%";
                            AppendParameter(text);
                            break;
                        }
                            
                        default:
                            throw new Exception("Unsupported string method");
                    }
                    return;
                }

                if (call.Method.Name == nameof(object.Equals))
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
                    defaultTable = new TableAlias { name = "t", alias = null, Mapping = defaultType };
                    tables.Add(defaultTable);
                }
                return defaultTable;
            }

            var table = tables.SingleOrDefault(t => t.alias == tableAlias);
            if (table == null)
            {
                table = new TableAlias { name = "t" + tables.Count, alias = tableAlias, Mapping = Mapper.GetTypeMapping(type) };
                tables.Add(table);
            }
            return table;
        }

        internal void SelectField(TableColumn column)
        {
            if (selectedFields == null)
                selectedFields = new List<FieldAlias>();
            selectedFields.Add(new FieldAlias {PropertyMapping = column.Column, tableAlias = column.Table?.alias, index = selectedFields.Count});
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
                return new TableColumn {Column = defaultType.Key, Table = defaultTable };
            }

            if (columnExpression.NodeType == ExpressionType.Call)
            {
                var callExpression = (MethodCallExpression)columnExpression;
                if (callExpression.Method.DeclaringType == typeof (ExpressionHelpers) &&
                    callExpression.Method.Name == "Property")
                {
                    var propertyInfo = (PropertyInfo)Expression.Lambda(callExpression.Arguments[1]).Compile().DynamicInvoke();
                    var table = GetTable(callExpression.Arguments[0], registerDefaultTable);
                    return new TableColumn {Column = table.Mapping.Columns[propertyInfo.Name], Table = table };
                }

                if (callExpression.Method.DeclaringType == typeof(ExpressionHelpers) &&
                    callExpression.Method.Name == "Key")
                {
                    var table = GetTable(callExpression.Arguments[0], registerDefaultTable);
                    return new TableColumn { Column = table.Mapping.Key, Table = table };
                }
                return null;
            }

            
            if (columnExpression.NodeType != ExpressionType.MemberAccess)
            {
                return null;
            }

            var columnMember = (MemberExpression)columnExpression;

            var columnMemberExpression = columnMember.Expression;
            if (columnMemberExpression.NodeType == ExpressionType.Convert)
                columnMemberExpression = ((UnaryExpression) columnMemberExpression).Operand;
            var memberExpression = columnMemberExpression as MemberExpression;
            if (memberExpression != null)
            {
                var table = GetTable(memberExpression, registerDefaultTable);
                if (table == null)
                {
                    // Asking the id of the item pointed by a foreign key is the same as asking the foreign key
                    var keyOfTable = Mapper.GetKey(memberExpression.Type);
                    if (keyOfTable !=null && columnMember.Member == keyOfTable.PropertyInfo)
                    {
                        return ExpressionToColumn(memberExpression);
                    }
                    return null;
                }

                return new TableColumn {Column = table.Mapping.Columns[columnMember.Member.Name], Table = table };
            }

            var parameterExpression = columnMemberExpression as ParameterExpression;
            if (parameterExpression != null && parameterExpression.Type == defaultType.Type)
            {
                if (defaultTable == null)
                {
                    if (registerDefaultTable)
                    {
                        defaultTable = RegisterTable(defaultType.Type, null);
                    }
                    else
                    {
                        var table = GetTable(columnExpression);
                        if (table != null)
                        {
                            return new TableColumn { Column = table.Mapping.Key, Table = table };
                        }
                        return null;
                    }
                }
                return new TableColumn {Column = defaultTable.Mapping.Columns[columnMember.Member.Name], Table = defaultTable};
            }

            var columnAsTable = GetTable(columnExpression, false);
            if (columnAsTable != null)
            {
                return new TableColumn {Column = columnAsTable.Mapping.Key, Table = columnAsTable};
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
            return new TableColumn {Column = table.Mapping.Key, Table = table};
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
        internal TableAlias AppendSelectedColumns(Type tableType, string tableAlias, IEnumerable<PropertyMapping> columns)
        {
            var table = RegisterTable(tableType, tableAlias);
            query.AppendSpace();
            bool first = true;
            if (selectedFields == null)
                selectedFields = new List<FieldAlias>();

            foreach (var column in columns)
            {
                selectedFields.Add(new FieldAlias { PropertyMapping = column, tableAlias = table.alias, index = selectedFields.Count });

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

        internal void AppendUpdate()
        {
            noAlias = true;
            query.Append("UPDATE");
        }

        internal void AppendDelete()
        {
            noAlias = true;
            query.Append("DELETE");
            currentContext = ContextEnum.Delete;
        }

        internal void AppendWhere()
        {
            Append(currentContext == ContextEnum.Where ? "AND" : "WHERE");
            currentContext = ContextEnum.Where;
        }

        internal TableAlias AppendAllSelects(Type tableType, string tableAlias)
        {
            var mapping = Mapper.GetTypeMapping(tableType);
            return AppendSelectedColumns(tableType, tableAlias, mapping.Columns.Values);
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
            public TypeMapping Mapping { get; set; }
            public string name;
            public string alias;
        }

        public class FieldAlias
        {
            public PropertyMapping PropertyMapping { get; set; }
            public string tableAlias;
            public int index;
        }

        public class TableColumn
        {
            public TableAlias Table { get; set; }
            public PropertyMapping Column { get; set; }
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
            
            AppendTableName(Mapper.GetTypeMapping(tableType));
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

        internal void AppendOr()
        {
            if (currentContext == ContextEnum.WhereExpression)
            {
                Append("OR ");
            }
            else
            {
                currentContext = ContextEnum.WhereExpression;
            }
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