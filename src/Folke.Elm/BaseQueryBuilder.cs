using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Folke.Elm.Mapping;
using Folke.Elm.Visitor;

namespace Folke.Elm
{
    public class BaseQueryBuilder<T> : BaseQueryBuilder, IQueryableCommand<T>
    {
        public BaseQueryBuilder(IFolkeConnection connection)
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
        protected readonly IFolkeConnection connection;
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

        public BaseQueryBuilder(IFolkeConnection connection, Type type = null, Type parametersType = null)
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

        protected internal TableAlias GetTable(string tableAlias)
        {
            return tables.SingleOrDefault(t => t.alias == tableAlias);
        }

        protected internal TableAlias GetTable(Expression alias)
        {
            return GetTable(GetTableAlias(alias));
        }

        protected internal TableAlias GetTable(Expression aliasExpression, bool register)
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
            var parameterIndex = AddParameter(parameter);

            query.Append(" @Item" + parameterIndex);
        }

        private int AddParameter(object parameter)
        {
            if (parameters == null)
                parameters = new List<object>();

            var parameterIndex = parameters.Count;
            if (parameter != null)
            {
                var parameterType = parameter.GetType();
                if (parameterType == typeof (TimeSpan))
                {
                    parameter = ((TimeSpan) parameter).TotalSeconds;
                }
               /* else if (parameterType.GetTypeInfo().IsEnum)
                {
                    var enumIndex = (int)parameter;
                    parameter = Enum.GetNames(parameterType).GetValue(enumIndex);
                }*/
                else if (Mapper.IsMapped(parameterType))
                {
                    var key = Mapper.GetTypeMapping(parameterType).Key;
                    parameter = key.PropertyInfo.GetValue(parameter);
                    if (parameter.Equals(0))
                        throw new Exception("Id should not be 0");
                }
            }

            parameters.Add(parameter);
            return parameterIndex;
        }

        internal void AddBooleanExpression(Expression expression, bool registerTable = false)
        {
            var lambda = expression as LambdaExpression;
            if (lambda != null)
            {
                AddExpression(lambda.Body, registerTable);
                return;
            }

            var visitable = ParseBooleanExpression(expression, registerTable);
            var visitor = new SqlVisitor(query, noAlias);
            visitable.Accept(visitor);
        }

        internal IVisitable ParseBooleanExpression(Expression expression, bool registerTable = false)
        {
            if ((expression is MemberExpression || expression is ParameterExpression) && !Driver.HasBooleanType)
            {
                return new BinaryOperator(BinaryOperatorType.Equal, ParseExpression(expression, registerTable),
                    new ConstantNumber(1));
            }
            else
            {
                return ParseExpression(expression, registerTable);
            }
        }

        internal void AddExpression(Expression expression, bool registerTable = false)
        {
            var visitable = ParseExpression(expression, registerTable);
            var visitor = new SqlVisitor(query, noAlias);
            visitable.Accept(visitor);
        }

        internal IVisitable ParseExpression(Expression expression, bool registerTable = false)
        {
            if (expression.NodeType == ExpressionType.Constant)
            {
                var constantExpression = (ConstantExpression) expression;
                if (constantExpression.Value == null)
                    return new Null();
                if (constantExpression.Type.GetTypeInfo().IsEnum)
                {
                    var enumType = constantExpression.Type;
                    var enumIndex = (int)constantExpression.Value;
                    return new Parameter(AddParameter(Enum.GetValues(enumType).GetValue(enumIndex)));
                }
            }

            var lambda = expression as LambdaExpression;
            if (lambda != null)
            {
                return ParseExpression(lambda.Body, registerTable);
            }

            var unary = expression as UnaryExpression;
            if (unary != null)
            {
                UnaryOperatorType unaryOperatorType;
                switch (unary.NodeType)
                {
                    case ExpressionType.Negate:
                        unaryOperatorType = UnaryOperatorType.Negate;
                        break;
                    case ExpressionType.Not:
                        unaryOperatorType = UnaryOperatorType.Not;
                        break;
                    case ExpressionType.Convert:
                    case ExpressionType.Quote:
                        return ParseExpression(unary.Operand, registerTable);
                    default:
                        throw new Exception("ExpressionType in UnaryExpression not supported");
                }

                IVisitable subExpression;

                if (unary.NodeType == ExpressionType.Not)
                    subExpression = ParseBooleanExpression(unary.Operand, registerTable);
                else
                    subExpression = ParseExpression(unary.Operand, registerTable);
                return new UnaryOperator(unaryOperatorType, subExpression);
            }

            var binary = expression as BinaryExpression;
            if (binary != null)
            {
                bool booleanOperator;
                switch (binary.NodeType)
                {
                    case ExpressionType.AndAlso:
                    case ExpressionType.OrElse:
                        booleanOperator = true;
                        break;
                    default:
                        booleanOperator = false;
                        break;
                }

                IVisitable left;
                if (booleanOperator)
                {
                    left = ParseBooleanExpression(binary.Left, registerTable);
                }
                else
                {
                    left = ParseExpression(binary.Left, registerTable);
                }
                
                BinaryOperatorType type;

                switch (binary.NodeType)
                {
                    case ExpressionType.Add:
                        type = BinaryOperatorType.Add;
                        break;
                    case ExpressionType.AndAlso:
                        type = BinaryOperatorType.AndAlso;
                        break;
                    case ExpressionType.Divide:
                        type = BinaryOperatorType.Divide;
                        break;
                    case ExpressionType.Equal:
                        type = BinaryOperatorType.Equal;
                        break;
                    case ExpressionType.GreaterThan:
                        type = BinaryOperatorType.GreaterThan;
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        type = BinaryOperatorType.GreaterThanOrEqual;
                        break;
                    case ExpressionType.LessThan:
                        type = BinaryOperatorType.LessThan;
                        break;
                    case ExpressionType.LessThanOrEqual:
                        type = BinaryOperatorType.LessThanOrEqual;
                        break;
                    case ExpressionType.Modulo:
                        type = BinaryOperatorType.Modulo;
                        break;
                    case ExpressionType.Multiply:
                        type = BinaryOperatorType.Multiply;
                        break;
                    case ExpressionType.NotEqual:
                        type = BinaryOperatorType.NotEqual;
                        break;
                    case ExpressionType.OrElse:
                        type = BinaryOperatorType.OrElse;
                        break;
                    case ExpressionType.Subtract:
                        type = BinaryOperatorType.Subtract;
                        break;
                    default:
                        throw new Exception("Expression type not supported");
                }

                IVisitable right;

                if (booleanOperator)
                {
                    right = ParseBooleanExpression(binary.Right, registerTable);
                }
                else
                {
                    right = ParseExpression(binary.Right, registerTable);
                }

                if (binary.Left.NodeType == ExpressionType.Convert && binary.Left.NodeType == ExpressionType.Convert
                    && ((UnaryExpression)binary.Left).Operand.Type.GetTypeInfo().IsEnum)
                {
                    var parameter = right as Parameter;
                    if (parameter != null)
                    {
                        parameters[parameter.Index] =
                            Enum.GetValues(((UnaryExpression) binary.Left).Operand.Type)
                                .GetValue((int)parameters[parameter.Index]);
                    }
                }

                if (right.GetType() == typeof (Null))
                {
                    if (binary.NodeType == ExpressionType.Equal)
                        return new UnaryOperator(UnaryOperatorType.IsNull, left);
                    if (binary.NodeType == ExpressionType.NotEqual)
                        return new UnaryOperator(UnaryOperatorType.IsNotNull, left);
                    throw new Exception("Operator not supported with null right member in " + binary);
                }

                return new BinaryOperator(type, left, right);
            }

            var constant = expression as ConstantExpression;
            if (constant != null)
            {
                if (constant.Type == typeof (ElmQueryable) || constant.Type.GetTypeInfo().BaseType == typeof(ElmQueryable))
                {
                    var queryable = (ElmQueryable)constant.Value;
                    var table = RegisterTable(queryable.ElementType, null);
                    return new Select(ParseSelectedColumn(table),
                        new AliasDefinition(new Table(table.Mapping.TableName, table.Mapping.TableSchema), table.name));
                }
                else
                {
                    return ParseParameter(constant.Value);
                }
            }

            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = (MemberExpression) expression;
                if (memberExpression.Expression != null && memberExpression.Expression.Type == parametersType)
                {
                    return new NamedParameter(memberExpression.Member.Name);
                }
            }

            var column = ExpressionToColumn(expression, registerTable);
            if (column != null)
            {
                return new Column(column.Table.name, column.Column.ColumnName);
            }

            if (expression.NodeType == ExpressionType.Call)
            {
                var call = (MethodCallExpression)expression;

                if (call.Method.DeclaringType == typeof (Queryable))
                {
                    switch (call.Method.Name)
                    {
                        case nameof(Queryable.Where):
                            return new Where(ParseExpression(call.Arguments[0], registerTable), ParseExpression(call.Arguments[1], registerTable));

                        case nameof(Queryable.Skip):
                            return new Skip(ParseExpression(call.Arguments[0], registerTable), ParseExpression(call.Arguments[1]));

                        case nameof(Queryable.Take):
                            return new Take(ParseExpression(call.Arguments[0], registerTable), ParseExpression(call.Arguments[1]));

                        case nameof(Queryable.OrderBy):
                            return new OrderBy(ParseExpression(call.Arguments[0], registerTable), ParseExpression(call.Arguments[1], registerTable));
                            
                        default:
                            throw new Exception("Unsupported Queryable method");
                    }
                }

                if (call.Method.DeclaringType == typeof(ExpressionHelpers))
                {
                    switch (call.Method.Name)
                    {
                        case nameof(ExpressionHelpers.Like):
                            return new BinaryOperator(BinaryOperatorType.Like, ParseExpression(call.Arguments[0], registerTable), ParseExpression(call.Arguments[1], registerTable));
                        case nameof(ExpressionHelpers.In):
                            return new BinaryOperator(BinaryOperatorType.In,
                                ParseExpression(call.Arguments[0], registerTable),
                                ParseValues((IEnumerable) Expression.Lambda(call.Arguments[1]).Compile().DynamicInvoke()));
                        case nameof(ExpressionHelpers.Between):
                            return new Between(ParseExpression(call.Arguments[0], registerTable), ParseExpression(call.Arguments[1], registerTable),
                                            ParseExpression(call.Arguments[2], registerTable));
                        default:
                            throw new Exception("Unsupported expression helper");
                    }
                }

                if (call.Method.DeclaringType == typeof(Math))
                {
                    MathFunctionType type;
                    switch (call.Method.Name)
                    {
                        case nameof(Math.Abs):
                            type = MathFunctionType.Abs;
                            break;

                        case nameof(Math.Cos):
                            type = MathFunctionType.Cos;
                            break;

                        case nameof(Math.Sin):
                            type = MathFunctionType.Sin;
                            break;
                        default:
                            throw new NotImplementedException("Not implemented math function");
                    }

                    return new MathFunction(type, ParseExpression(call.Arguments[0], registerTable));
                }

                if (call.Method.DeclaringType == typeof(SqlFunctions))
                {
                    switch (call.Method.Name)
                    {
                        case nameof(SqlFunctions.LastInsertedId):
                            return new LastInsertedId();
                        case nameof(SqlFunctions.Max):
                            return new MathFunction(MathFunctionType.Max, ParseExpression(call.Arguments[0], registerTable));
                        case nameof(SqlFunctions.Sum):
                            return new MathFunction(MathFunctionType.Sum, ParseExpression(call.Arguments[0], registerTable));
                        default:
                            throw new Exception("Unsupported sql function");
                    }
                }

                if (call.Method.DeclaringType == typeof(string))
                {
                    switch (call.Method.Name)
                    {
                        case nameof(string.StartsWith):
                        {
                            var text = (string)Expression.Lambda(call.Arguments[0]).Compile().DynamicInvoke();
                            text = text.Replace("\\", "\\\\").Replace("%", "\\%") + "%";
                            return new BinaryOperator(BinaryOperatorType.Like, ParseExpression(call.Object, registerTable), ParseParameter(text));
                        }
                        case nameof(string.Contains):
                            {
                                var text = (string)Expression.Lambda(call.Arguments[0]).Compile().DynamicInvoke() ?? string.Empty;
                                text = "%" + text.Replace("\\", "\\\\").Replace("%", "\\%") + "%";
                                return new BinaryOperator(BinaryOperatorType.Like, ParseExpression(call.Object, registerTable), ParseParameter(text));
                            }
                            
                        default:
                            throw new Exception("Unsupported string method");
                    }
                }

                if (call.Method.Name == nameof(object.Equals))
                {
                    return new BinaryOperator(BinaryOperatorType.Equal, ParseExpression(call.Object, registerTable), ParseExpression(call.Arguments[0], registerTable));
                }
            }

            var value = Expression.Lambda(expression).Compile().DynamicInvoke();
            return ParseParameter(value);
        }

        private IVisitable ParseParameter(object value)
        {
            if (value == null) return new Null();
            return new Parameter(AddParameter(value));
        }

        private IVisitable ParseValues(IEnumerable values)
        {
            var list = new List<IVisitable>();

            foreach (var value in values)
            {
                list.Add(new Parameter(AddParameter(value)));
            }
            return new Values(list);
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
            if (columnMemberExpression != null)
            {

                if (columnMemberExpression.NodeType == ExpressionType.Convert)
                    columnMemberExpression = ((UnaryExpression)columnMemberExpression).Operand;
                var memberExpression = columnMemberExpression as MemberExpression;
                if (memberExpression != null)
                {
                    var table = GetTable(memberExpression, registerDefaultTable);
                    if (table == null)
                    {
                        // Asking the id of the item pointed by a foreign key is the same as asking the foreign key
                        var keyOfTable = Mapper.GetKey(memberExpression.Type);
                        if (keyOfTable != null && columnMember.Member == keyOfTable.PropertyInfo)
                        {
                            return ExpressionToColumn(memberExpression);
                        }
                        return null;
                    }

                    return new TableColumn { Column = table.Mapping.Columns[columnMember.Member.Name], Table = table };
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
                    return new TableColumn { Column = defaultTable.Mapping.Columns[columnMember.Member.Name], Table = defaultTable };
                }
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

        private IVisitable ParseSelectedColumn(TableAlias table)
        {
            if (selectedFields == null)
                selectedFields = new List<FieldAlias>();
            var columns = table.Mapping.Columns;
            var fields = new List<IVisitable>();
            foreach (var column in columns.Values)
            {
                selectedFields.Add(new FieldAlias { PropertyMapping = column, tableAlias = table.alias, index = selectedFields.Count });
                fields.Add(new Column(table.name, column.ColumnName));
            }
            return new Fields(fields);
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

        internal void AppendSubWhereEnd()
        {
            Append(")");
            currentContext = ContextEnum.Where;
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

        public SqlStringBuilder StringBuilder => query;
    }
}