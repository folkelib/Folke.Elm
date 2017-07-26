using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Folke.Elm.Fluent;
using Folke.Elm.Mapping;
using Folke.Elm.Parsers;
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
        private readonly SqlStringBuilder query;
        private IList<object> parameters;

        private MappedClass baseMappedClass;

        private readonly IList<SelectedField> selectedFields = new List<SelectedField>();
        private readonly IList<SelectedTable> tables;

        private SelectedTable defaultTable;
        private readonly TypeMapping defaultType;
        private readonly Type parametersType;
        private readonly ExpressionToVisitable expressionToVisitable;

        internal SelectedTable DefaultTable => defaultTable;
        internal IList<SelectedField> SelectedFields => selectedFields;
        
        public IMapper Mapper { get; set; }
        
        public string Sql => query.ToString();

        public IFolkeConnection Connection { get; }

        internal IDatabaseDriver Driver { get; }

        public object[] Parameters => parameters?.ToArray();

        public MappedClass MappedClass => baseMappedClass ?? (baseMappedClass = MappedClass.MapClass(selectedFields, defaultType, DefaultTable));

        public BaseQueryBuilder(IFolkeConnection connection, Type type = null, Type parametersType = null)
            : this(connection.Driver, connection.Mapper, type, parametersType)
        {
            Connection = connection;
        }

        public BaseQueryBuilder(IDatabaseDriver databaseDriver, IMapper mapper, Type defaultType, Type parametersType = null)
            : this(databaseDriver, mapper)
        {
            this.defaultType = defaultType != null ? Mapper.GetTypeMapping(defaultType) : null;
            this.parametersType = parametersType;
        }
        
        public BaseQueryBuilder(BaseQueryBuilder parentBuilder):this(parentBuilder.Driver, parentBuilder.Mapper)
        {
            if (parentBuilder.parameters == null)
                parentBuilder.parameters = new List<object>();
            parameters = parentBuilder.parameters;
            tables = parentBuilder.tables;
            defaultTable = parentBuilder.defaultTable;
            defaultType = parentBuilder.defaultType;
        }

        public BaseQueryBuilder(IDatabaseDriver databaseDriver, IMapper mapper)
        {
            Mapper = mapper;
            Driver = databaseDriver;
            query = databaseDriver.CreateSqlStringBuilder() ?? new SqlStringBuilder();
            tables = new List<SelectedTable>();
            expressionToVisitable = new ExpressionToVisitable(Mapper);
        }

        protected internal SelectedTable GetTable(LambdaExpression alias, bool register)
        {
            var result = TryGetTable(alias.Body, register);
            if (result == null)
            {
                throw new Exception($"The expression {alias} can't be resolved as a table. Ensure that {alias.Type} is mappable, it should have IFolkeTable interface or a Table attribute.");
            }
            return result;
        }

        /// <summary>
        /// Gets a table by its alias
        /// </summary>
        /// <param name="alias">The expression that is used as an alias to a table</param>
        /// <param name="register">Register the table if it has never been</param>
        /// <returns>The table or null if it is not a table</returns>
        protected internal SelectedTable TryGetTable(Expression alias, bool register)
        {
            SelectedTable table;
            switch (alias.NodeType)
            {
                case ExpressionType.MemberAccess:
                    if (!Mapper.IsMapped(alias.Type)) return null;
                    var memberExpression = (MemberExpression)alias;
                    var parentTable = TryGetTable(memberExpression.Expression, false);
                    if (parentTable == null)
                    {
                        return null;
                    }

                    var parentProperty = parentTable.Mapping.GetColumn(memberExpression.Member);
                    table = tables.FirstOrDefault(x => x.Parent == parentTable && x.ParentMember == parentProperty);
                    if (table == null)
                    {
                        if (!register)
                            return null;
                        table = RegisterTable(parentTable, parentProperty);
                    }
                    break;
                case ExpressionType.Parameter:
                    return RegisterRootTable(Mapper.GetTypeMapping(alias.Type));
                case ExpressionType.Convert:
                    return TryGetTable(((UnaryExpression) alias).Operand, register);
                default:
                    // This is not a table
                    return null;
            }

            return table;
        }

        internal SelectedTable RegisterTable(SelectedTable parentTable, PropertyMapping parentField)
        {
            var table = new SelectedTable
            {
                Parent = parentTable,
                Mapping = parentField.Reference,
                Alias = "t" + tables.Count,
                ParentMember = parentField
            };
            tables.Add(table);
            parentTable.Children[parentField] = table;
            return table;
        }

        public int AddParameter(object parameter)
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
                else if (parameterType.GetTypeInfo().IsEnum && parameterType.GetTypeInfo().GetCustomAttribute(typeof(FlagsAttribute)) != null)
                {
                    parameter = Convert.ChangeType(parameter, Enum.GetUnderlyingType(parameterType));
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
            return parameterIndex;
        }

        internal void AddBooleanExpression(Expression expression, ParseOptions options = 0)
        {
            var lambda = expression as LambdaExpression;
            if (lambda != null)
            {
                AddExpression(lambda.Body, options);
                return;
            }

            var visitable = ParseBooleanExpression(expression, options);
            visitable.Accept(query);
        }

        internal IVisitable ParseBooleanExpression(Expression expression, ParseOptions options = 0)
        {
            if ((expression is MemberExpression || expression is ParameterExpression) && !Driver.HasBooleanType)
            {
                return new BinaryOperator(BinaryOperatorType.Equal, ParseExpression(expression, options | ParseOptions.Value),
                    new ConstantNumber(1));
            }
            else
            {
                return ParseExpression(expression, options);
            }
        }

        internal void AddExpression(Expression expression, ParseOptions options = 0)
        {
            var visitable = ParseExpression(expression, options);
            visitable.Accept(query);
        }

        internal IVisitable ParseExpression(Expression expression, ParseOptions options = 0)
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
                return ParseExpression(lambda.Body, options);
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
                        return ParseExpression(unary.Operand, options);
                    default:
                        throw new Exception("ExpressionType in UnaryExpression not supported");
                }

                var subExpression = unary.NodeType == ExpressionType.Not ?
                    ParseBooleanExpression(unary.Operand, options)
                    : ParseExpression(unary.Operand, options);
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

                var left = booleanOperator ? 
                                      ParseBooleanExpression(binary.Left, options) 
                                      : ParseExpression(binary.Left, options | ParseOptions.Value);
                
                BinaryOperatorType type;

                switch (binary.NodeType)
                {
                    case ExpressionType.Add:
                        type = BinaryOperatorType.Add;
                        break;
                    case ExpressionType.And:
                        type = BinaryOperatorType.And;
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
                    case ExpressionType.Or:
                        type = BinaryOperatorType.Or;
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

                var right = booleanOperator ? 
                                       ParseBooleanExpression(binary.Right, options)
                                       : ParseExpression(binary.Right, options | ParseOptions.Value);

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
                    var table = RegisterRootTable(Mapper.GetTypeMapping(queryable.ElementType));
                    return new AliasDefinition(new Table(table.Mapping.TableName, table.Mapping.TableSchema), table.Alias);
                }
                return ParseParameter(constant.Value);
            }

            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = (MemberExpression) expression;
                if (memberExpression.Expression != null && memberExpression.Expression.Type == parametersType)
                {
                    return new NamedParameter(memberExpression.Member.Name);
                }
            }

            var column = ExpressionToColumn(expression, options);
            if (column != null)
            {
                return column;
            }

            if (expression.NodeType == ExpressionType.Call)
            {
                var call = (MethodCallExpression)expression;

                if (call.Method.DeclaringType == typeof (Queryable))
                {
                    switch (call.Method.Name)
                    {
                        case nameof(Queryable.Where):
                            return new Where(ParseExpression(call.Arguments[0], options), ParseExpression(call.Arguments[1], options));

                        case nameof(Queryable.Skip):
                            return new Skip(ParseExpression(call.Arguments[0], options), ParseExpression(call.Arguments[1]));

                        case nameof(Queryable.Take):
                            return new Take(ParseExpression(call.Arguments[0], options), ParseExpression(call.Arguments[1]));

                        case nameof(Queryable.OrderBy):
                            return new OrderBy(ParseExpression(call.Arguments[0], options), ParseExpression(call.Arguments[1], options));

                        case nameof(Queryable.Join):
                            return new Join(ParseExpression(call.Arguments[0], options), ParseExpression(call.Arguments[1], options), ParseExpression(call.Arguments[2], ParseOptions.Value), ParseExpression(call.Arguments[3], ParseOptions.Value), ParseExpression(call.Arguments[4]));

                        case nameof(Queryable.Count):
                            var from = ParseExpression(call.Arguments[0], options);
                            return new Select(new Count(), from);

                        default:
                            throw new Exception("Unsupported Queryable method");
                    }
                }

                if (call.Method.DeclaringType == typeof(ExpressionHelpers))
                {
                    switch (call.Method.Name)
                    {
                        case nameof(ExpressionHelpers.Like):
                            return new BinaryOperator(BinaryOperatorType.Like, ParseExpression(call.Arguments[0], options), ParseExpression(call.Arguments[1], options));
                        case nameof(ExpressionHelpers.In):
                            return new BinaryOperator(BinaryOperatorType.In,
                                ParseExpression(call.Arguments[0], options),
                                ParseValues((IEnumerable) Expression.Lambda(call.Arguments[1]).Compile().DynamicInvoke()));
                        case nameof(ExpressionHelpers.Between):
                            return new Between(ParseExpression(call.Arguments[0], options), ParseExpression(call.Arguments[1], options),
                                            ParseExpression(call.Arguments[2], options));
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

                    return new MathFunction(type, ParseExpression(call.Arguments[0], options));
                }

                if (call.Method.DeclaringType == typeof(SqlFunctions))
                {
                    switch (call.Method.Name)
                    {
                        case nameof(SqlFunctions.LastInsertedId):
                            return new LastInsertedId();
                        case nameof(SqlFunctions.Max):
                            return new MathFunction(MathFunctionType.Max, ParseExpression(call.Arguments[0], options));
                        case nameof(SqlFunctions.Sum):
                            return new MathFunction(MathFunctionType.Sum, ParseExpression(call.Arguments[0], options));
                        case nameof(SqlFunctions.IsNull):
                            return new MathFunction(MathFunctionType.IsNull, ParseExpression(call.Arguments[0], options), ParseExpression(call.Arguments[1], options));
                        case nameof(SqlFunctions.Case):
                            var cases = ((NewArrayExpression) call.Arguments[0]).Expressions;
                            return new Case(cases.Select(x => ParseExpression(x)));
                        case nameof(SqlFunctions.When):
                            return new When(ParseExpression(call.Arguments[0], options),
                                ParseExpression(call.Arguments[1], options));
                        case nameof(SqlFunctions.Else):
                            return new Else(ParseExpression(call.Arguments[0], options));
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
                            return new BinaryOperator(BinaryOperatorType.Like, ParseExpression(call.Object, options), ParseParameter(text));
                        }
                        case nameof(string.Contains):
                            {
                                var text = (string)Expression.Lambda(call.Arguments[0]).Compile().DynamicInvoke() ?? string.Empty;
                                text = "%" + text.Replace("\\", "\\\\").Replace("%", "\\%") + "%";
                                return new BinaryOperator(BinaryOperatorType.Like, ParseExpression(call.Object, options), ParseParameter(text));
                            }
                            
                        default:
                            throw new Exception("Unsupported string method");
                    }
                }

                if (call.Method.Name == nameof(object.Equals))
                {
                    return new BinaryOperator(BinaryOperatorType.Equal, ParseExpression(call.Object, options), ParseExpression(call.Arguments[0], options));
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

        internal SelectedTable RegisterRootTable(TypeMapping typeMapping = null)
        {
            if (typeMapping == defaultType || typeMapping == null)
            {
                if (defaultTable == null)
                {
                    defaultTable = new SelectedTable {Alias = "t", Parent = null, Mapping = typeMapping ?? defaultType};
                    tables.Add(defaultTable);
                }
                return defaultTable;
            }
            else
            {
                var table = tables.FirstOrDefault(x => x.Mapping == typeMapping && x.Parent == null);
                if (table == null)
                {
                    table = new SelectedTable { Alias = "t" + tables.Count, Mapping = typeMapping };
                    tables.Add(table);
                }
                return table;
            }
        }

        /// <summary>Adds a column to the list of selected values</summary>
        /// <param name="field"></param>
        internal SelectedField SelectField(Field field)
        {
            var selectedField = new SelectedField { Field = field, Index = selectedFields.Count };
            selectedFields.Add(selectedField);
            return selectedField;
        }

        /// <summary>Select a column by an expression that maps to a column</summary>
        /// <param name="column">The expression</param>
        internal SelectedField SelectField(Expression column)
        {
            return SelectField((Field)ExpressionToColumn(column, ParseOptions.RegisterTables | ParseOptions.Value));
        }

        /// <summary>Converts an expression to a column or a table</summary>
        /// <param name="columnExpression">The expression that should point to a table</param>
        /// <param name="options"><c>true</c> if the table must be added to the list of selected tables if it was not</param>
        /// <returns>The column or <c>null</c> if the expression did not point to a column</returns>
        internal IVisitable ExpressionToColumn(Expression columnExpression, ParseOptions options = 0)
        {
            if (columnExpression.NodeType == ExpressionType.Convert)
            {
                columnExpression = ((UnaryExpression) columnExpression).Operand;
            }

            if (columnExpression.NodeType == ExpressionType.Parameter)
            {
                if (options.HasFlag(ParseOptions.Value))
                {
                    return new Field(defaultTable, defaultType.Key);
                }
                return defaultTable;
            }

            if (columnExpression.NodeType == ExpressionType.Call)
            {
                var callExpression = (MethodCallExpression)columnExpression;
                if (callExpression.Method.DeclaringType == typeof (ExpressionHelpers) &&
                    callExpression.Method.Name == nameof(ExpressionHelpers.Property))
                {
                    var propertyInfo = (PropertyInfo)Expression.Lambda(callExpression.Arguments[1]).Compile().DynamicInvoke();
                    var table = TryGetTable(callExpression.Arguments[0], options.HasFlag(ParseOptions.RegisterTables));
                    return new Field(table, table.Mapping.Columns[propertyInfo.Name]);
                }

                if (callExpression.Method.DeclaringType == typeof(ExpressionHelpers) &&
                    callExpression.Method.Name == nameof(ExpressionHelpers.Key))
                {
                    var table = TryGetTable(callExpression.Arguments[0], options.HasFlag(ParseOptions.RegisterTables));
                    return new Field(table, table.Mapping.Key);
                }
                return null;
            }
            
            if (columnExpression.NodeType != ExpressionType.MemberAccess)
            {
                return null;
            }

            var columnMember = (MemberExpression)columnExpression;
            var columnIsTable = TryGetTable(columnExpression, options.HasFlag(ParseOptions.RegisterTables));
            if (columnIsTable != null)
            {
                if (options.HasFlag(ParseOptions.Value))
                {
                    return new Field(columnIsTable, columnIsTable.Mapping.Key);
                }

                return columnIsTable;
            }

            var parentTable = TryGetTable(columnMember.Expression, options.HasFlag(ParseOptions.RegisterTables));
            if (parentTable != null)
            {
                return new Field(parentTable, parentTable.Mapping.GetColumn(columnMember.Member));
            }

            return null;
        }
        
        /// <summary>
        /// Get the key column from a table expression.
        /// Example: x => x.Identity will returns the Id column from the Identity table
        /// </summary>
        /// <param name="tableExpression">The expression</param>
        /// <returns></returns>
        internal Field GetTableKey(Expression tableExpression)
        {
            var table = TryGetTable(tableExpression, false);
            return new Field(table, table.Mapping.Key);
        }

        internal IVisitable ParseSelectedColumn(SelectedTable table)
        {
            var columns = table.Mapping.Columns;
            var fields = new List<IVisitable>();
            AddAllColumns(table, columns, fields, null);
            return new Fields(fields);
        }

        private void AddAllColumns(SelectedTable table, Dictionary<string, PropertyMapping> columns, List<IVisitable> fields, string baseName)
        {
            foreach (var column in columns.Values)
            {
                if (column.Reference != null)
                {
                    AddAllColumns(table, column.Reference.Columns, fields, column.ComposeName(baseName));
                    continue;
                }
                var visitable = new Field(table, column);
                this.SelectField(visitable);
                fields.Add(visitable);
            }
        }

        public SqlStringBuilder StringBuilder => query;
    }
}