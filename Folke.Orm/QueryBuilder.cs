using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;

namespace Folke.Orm
{
    public class QueryBuilder<T>:BaseQueryBuilder<T, FolkeTuple>
                where T : class, new()
    {
        public QueryBuilder(FolkeConnection connection):base(connection)
        {
        }
    }
    
    /// <summary>
    /// Used to create a SQL query and create objects with its results
    /// </summary>
    /// <typeparam name="T">The return type of the SQL query</typeparam>
    /// <typeparam name="TMe">A Tuple with the query parameters</typeparam>
    public class BaseQueryBuilder<T, TMe>
        where T : class, new()
    {
        enum ContextEnum
        {
            Unknown,
            Select,
            Where,
            OrderBy,
            Set,
            Join,
            Values,
            From,
            Delete
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

        public BaseQueryBuilder<T, TMe> Append(string sql)
        {
            if (query.Length != 0)
                query.Append(' ');
            query.Append(sql);
            return this;
        }

        protected TableAlias RegisterTable<U>(Expression<Func<T, U>> alias)
        {
            return RegisterTable(typeof(U), GetTableAlias(alias.Body as MemberExpression));
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
        /// <param name="columnType"></param>
        /// <param name="name"></param>
        private void AppendField(string tableName, Type columnType, string name)
        {
            query.Append(' ');
            if (tableName != null && !noAlias)
            {
                query.Append(tableName);
                query.Append(".");
            }
            query.Append(beginSymbol);
            query.Append(name);
            if (IsForeign(columnType))
                query.Append("_id");
            query.Append(endSymbol);
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
        /// <param name="property"></param>
        /// <returns></returns>
        private string GetColumnName(MemberInfo property)
        {
            var column = property.GetCustomAttribute<ColumnAttribute>();
            if (column != null && column.Name != null)
                return column.Name;
            return property.Name;
        }

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
            AppendField(table.name, column.PropertyType, GetColumnName(column));
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
            else
            {
                return Column(typeof(T), (string)null, (PropertyInfo)columnMember.Member);
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
            return Column((MemberExpression)column.Body);
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
                AppendField(table.name, typeof(int), "Id");
            }
            else if (expression.Expression is MemberExpression)
            {
                var accessTo = expression.Expression as MemberExpression;
                var table = GetTable(accessTo);
                AppendField(table.name, expression.Type, expression.Member.Name);
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
            if (currentContext == ContextEnum.Select)
                throw new Exception("Does not call TryColumn in a Select context");

            if (column.Expression is MemberExpression)
            {
                var accessTo = column.Expression as MemberExpression;
                var table = GetTable(accessTo);
                if (table != null)
                {
                    AppendField(table.name, column.Type, GetColumnName(column.Member));
                    return true;
                }
            }
            else if (column.Expression is ParameterExpression && column.Expression.Type == typeof(T) && parameterTable != null)
            {
                AppendField(parameterTable.name, column.Type, GetColumnName(column.Member));
                return true;
            }
            else if (column.Expression is ParameterExpression && column.Expression.Type == typeof(TMe))
            {
                query.Append("@" + column.Member.Name);
                return true;
            }
            else
            {
                var table = GetTable(column);
                if (table != null)
                {
                    AppendField(table.name, typeof(int), "Id");
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
                if (IsIgnored(column.PropertyType))
                    continue;

                selectedFields.Add(new FieldAlias { propertyInfo = column, alias = table.alias, index = selectedFields.Count });

                if (first)
                    first = false;
                else
                    query.Append(',');
                AppendField(table.name, column.PropertyType, GetColumnName(column));
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
            return Columns(typeof(T), (string) null, column.Select(x => GetExpressionPropertyInfo(x)));
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

        public BaseQueryBuilder<T, TMe> All<U>(Expression<Func<T, U>> tableAlias)
        {
            return All(typeof(U), (MemberExpression) tableAlias.Body);
        }

        protected void AppendTableName(Type type)
        {
            if (query.Length != 0)
                query.Append(' ');
            var schema = type.GetCustomAttribute<SchemaAttribute>();
            if (schema != null)
            {
                query.Append(beginSymbol);
                query.Append(schema.Name);
                query.Append(endSymbol);
                query.Append('.');
            }    
            query.Append(beginSymbol);
            query.Append(type.Name);
            query.Append(endSymbol);
        }

        protected BaseQueryBuilder<T, TMe> Table<U>(Expression<Func<T, U>> tableAlias)
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
                baseMappedClass = MapClass(typeof(T));
                Append("FROM");
            }
            else if (currentContext == ContextEnum.From)
                Append(",");
            currentContext = ContextEnum.From;
        }

        public BaseQueryBuilder<T, TMe> From<U>(Expression<Func<U>> tableAlias)
        {
            AppendFrom();
            return Table(typeof(U), (MemberExpression) tableAlias.Body);
        }

        public BaseQueryBuilder<T, TMe> From<U>(Expression<Func<T, U>> tableAlias)
        {
            AppendFrom();
            return Table(tableAlias);
        }

        public BaseQueryBuilder<T, TMe> From()
        {
            AppendFrom();
            return Table(typeof(T), (string) null);
        }

        public BaseQueryBuilder<T, TMe> AndFrom<U>(Expression<Func<T, U>> tableAlias)
        {
            Append(",");
            return Table(tableAlias);
        }

        public BaseQueryBuilder<T, TMe> LeftJoin(Type tableType, string tableAlias)
        {
            Append("LEFT JOIN");
            return Table(tableType, tableAlias);
        }

        public BaseQueryBuilder<T, TMe> LeftJoin<U>(Expression<Func<T, U>> tableAlias)
        {
            Append("LEFT JOIN");
            return Table(tableAlias);
        }

        public BaseQueryBuilder<T, TMe> RightJoin<U>(Expression<Func<T, U>> tableAlias)
        {
            Append("RIGHT JOIN");
            return Table(tableAlias);
        }

        public BaseQueryBuilder<T, TMe> LeftJoinOn<U>(Expression<Func<T, U>> column)
        {
            return LeftJoin(column).On(column);
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
                if (IsForeign(parameterType))
                {
                    parameter = parameterType.GetProperty("Id").GetValue(parameter);
                    if (parameter.Equals(0))
                        throw new Exception("Id should not be 0");
                }
            }

            parameters.Add(parameter);
            
            query.Append("@Item" + parameterIndex);
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
            command.CommandText = query.ToString();
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
                        throw new Exception("Operator not supported with null right member in " + binary.ToString());
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
                AppendField(parameterTable.name, typeof(int), "Id");
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
                            AddExpression(call.Arguments[0]);
                            query.Append(" LIKE ");
                            AddExpression(call.Arguments[1]);
                            break;
                        default:
                            throw new Exception("Bizarre");
                    }
                    return;
                }
            }

            var value = System.Linq.Expressions.Expression.Lambda(expression).Compile().DynamicInvoke();
            Parameter(value);
        }

        public BaseQueryBuilder<T, TMe> SelectAll<U>(Expression<Func<T, U>> tableAlias)
        {
            currentContext = ContextEnum.Select;
            Append("SELECT");
            return All(tableAlias);
        }

        public BaseQueryBuilder<T, TMe> SelectAll<U>(Expression<Func<U>> tableAlias)
        {
            currentContext = ContextEnum.Select;
            Append("SELECT");
            return All(typeof(U), (MemberExpression) tableAlias.Body);
        }

        public BaseQueryBuilder<T, TMe> SelectAll()
        {
            currentContext = ContextEnum.Select;
            Append("SELECT");
            return All(typeof(T), (string) null);
        }

        public BaseQueryBuilder<T, TMe> Select()
        {
            currentContext = ContextEnum.Select;
            return Append("SELECT");
        }
        
        public BaseQueryBuilder<T, TMe> Select(params Expression<Func<T, object>>[] column)
        {
            currentContext = ContextEnum.Select;
            Append("SELECT");
            return Columns(typeof(T), (string) null, column.Select(x => GetExpressionPropertyInfo(x)));
        }

        public BaseQueryBuilder<T, TMe> Select<U,V>(Expression<Func<U>> tableAlias, Expression<Func<V>> column)
        {
            currentContext = ContextEnum.Select;
            Append("SELECT");
            return Column(typeof(U), (MemberExpression) tableAlias.Body, ((MemberExpression) column.Body).Member as PropertyInfo);
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

        public BaseQueryBuilder<T, TMe> AndAll<U>(Expression<Func<T, U>> tableAlias)
        {
            return AndAll(tableAlias.Body.Type, (MemberExpression) tableAlias.Body);
        }

        public BaseQueryBuilder<T, TMe> SelectCountAll()
        {
            currentContext = ContextEnum.Select;
            Append("SELECT COUNT(*)");
            return this;
        }

        public BaseQueryBuilder<T, TMe> On<U>(Expression<Func<T, U>> leftColumn, Expression<Func<T, U>> rightColumn)
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
            AppendField(GetTable(leftTableAlias).name, leftColumn.PropertyType, leftColumn.Name);
            query.Append("=");
            AppendField(GetTable(rightTableAlias).name, rightColumn.PropertyType, rightColumn.Name);
            return this;
        }
        
        public BaseQueryBuilder<T, TMe> On<U>(Expression<Func<T, U>> rightColumn)
        {
            currentContext = ContextEnum.Join;
            Append("ON ");
            var memberExpression = (MemberExpression) rightColumn.Body;
            Column(memberExpression);
            query.Append("=");
            Column(memberExpression.Type, GetTableAlias(memberExpression), memberExpression.Type.GetProperty("Id"));
            return this;
        }

        public BaseQueryBuilder<T, TMe> Max<U>(Expression<Func<T, U>> column)
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

        public BaseQueryBuilder<T, TMe> BeginSub()
        {
            if (queryStack == null)
                queryStack = new Stack<ContextEnum>();
            queryStack.Push(currentContext);
            currentContext = ContextEnum.Unknown;
            query.Append('(');
            return this;
        }

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

        public BaseQueryBuilder<T, TMe> In<U>(U[] values)
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

        public BaseQueryBuilder<T, TMe> Fetch(params Expression<Func<T, object>>[] fetches)
        {
            SelectAll();
            foreach (var fetch in fetches)
                AndAll(fetch);
            From();
            foreach (var fetch in fetches)
                LeftJoinOn(fetch);
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

        public BaseQueryBuilder<T, TMe> OrderBy<U>(Expression<Func<T, U>> column)
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

        public BaseQueryBuilder<T, TMe> Limit<U>(Expression<Func<TMe, U>> offset, int count)
        {
            var expression = offset.Body as MemberExpression;
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
                if (IsIgnored(property.PropertyType) || IsReadOnly(property))
                    continue;
                if (first)
                    first = false;
                else
                    query.Append(",");
                AppendField(null, property.PropertyType, GetColumnName(property));
            }
            query.Append(") VALUES (");
            first = true;
            foreach (var property in type.GetProperties())
            {
                if (IsIgnored(property.PropertyType) || IsReadOnly(property))
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

        public BaseQueryBuilder<T, TMe> Set(T value)
        {
            Append("SET ");
            currentContext = ContextEnum.Set;
            var type = value.GetType();
            bool first = true;
            var table = parameterTable;
            foreach (var property in type.GetProperties())
            {
                if (IsIgnored(property.PropertyType) || IsReadOnly(property))
                    continue;

                if (first)
                    first = false;
                else
                    query.Append(",");
                AppendField(table.name, property.PropertyType, GetColumnName(property));
                query.Append("=");
                Parameter(property.GetValue(value));
            }
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
                var ret = constructor.Invoke(null);
                
                if (idField != null)
                    idField.propertyInfo.SetValue(ret, id);

                if (collections != null)
                {
                    foreach (var collection in collections)
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
            if (selectedFields == null)
                return null;

            var mappedClass = new MappedClass();

            var idProperty = type.GetProperty("Id");
            mappedClass.constructor = type.GetConstructor(Type.EmptyTypes);
            if (idProperty != null)
            {
                var selectedField = selectedFields.SingleOrDefault(f => f.alias == alias && f.propertyInfo == idProperty);
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
                    var fieldInfo = selectedFields.SingleOrDefault(f => f.alias == alias && f.propertyInfo == property);
                    bool isForeign = IsForeign(property.PropertyType);
                    if (fieldInfo != null || (isForeign && (mappedClass.idField == null || mappedClass.idField.selectedField != null)))
                    {
                        var mappedField = new MappedField { propertyInfo = property, selectedField = fieldInfo };

                        if (IsForeign(property.PropertyType))
                        {
                            mappedField.mappedClass = MapClass(property.PropertyType, alias == null ? property.Name : alias + "." + property.Name);
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
                    object other = Read(connection, mappedField.propertyInfo.PropertyType, reader, mappedField.mappedClass, id);
                    mappedField.propertyInfo.SetValue(value, other);
                }
            }
            return value;
        }


        public T Single(IFolkeConnection connection, params object[] parameters)
        {
            using (var command = CreateCommand(connection, parameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new Exception("No result found");
                    var value = Read(connection, typeof(T), reader, baseMappedClass);
                    reader.Close();
                    return (T)value;
                }
            }
        }

        public T SingleOrDefault(IFolkeConnection connection, params object[] parameters)
        {
            using (var command = CreateCommand(connection, parameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return default(T);
                    var value = Read(connection, typeof(T), reader, baseMappedClass);
                    reader.Close();
                    return (T)value;
                }
            }
        }

        public bool TryExecute(IFolkeConnection connection, params object[] parameters)
        {
            try
            {
                Execute(connection, parameters);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public void Execute(IFolkeConnection connection, params object[] parameters)
        {
            using (var command = CreateCommand(connection, parameters))
            {
                command.ExecuteNonQuery();
            }
        }

        public IList<T> List(IFolkeConnection connection, params object[] parameters)
        {
            using (var command = CreateCommand(connection, parameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    var ret = new List<T>();
                    while (reader.Read())
                    {
                        var value = Read(connection, typeof(T), reader, baseMappedClass);
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
            using (var command = CreateCommand(connection, parameters))
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
            currentContext = ContextEnum.Delete;
            noAlias = true;
            return Append("DELETE");
        }

        public U Scalar<U>()
        {
            return Scalar<U>(connection, this.parameters == null ? null : this.parameters.ToArray());
        }

        public IList<T> List()
        {
            return List(connection, this.parameters == null ? null : this.parameters.ToArray());
        }

        public void Execute()
        {
            Execute(connection, this.parameters == null ? null : this.parameters.ToArray());
        }

        public bool TryExecute()
        {
            return TryExecute(connection, this.parameters == null ? null : this.parameters.ToArray());
        }

        public T SingleOrDefault()
        {
            return SingleOrDefault(connection, this.parameters == null ? null : this.parameters.ToArray());
        }

        /// <summary>
        /// A single result
        /// </summary>
        /// <returns></returns>
        public T Single()
        {
            return Single(connection, this.parameters == null ? null : this.parameters.ToArray());
        }
    }
}