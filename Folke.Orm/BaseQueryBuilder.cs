using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Folke.Orm
{
    public class BaseQueryBuilder<T> : BaseQueryBuilder
    {
        public BaseQueryBuilder(FolkeConnection connection)
            : base(connection)
        {
            defaultType = typeof(T);
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
        protected StringBuilder query = new StringBuilder();
        protected IList<object> parameters;
        protected readonly FolkeConnection connection;
        protected MappedClass baseMappedClass;
        protected IDatabaseDriver driver;
        protected char beginSymbol;
        protected char endSymbol;
        protected IList<FieldAlias> selectedFields;
        protected IList<TableAlias> tables;
        protected TableAlias parameterTable;
        protected Type defaultType;
        protected Type parametersType;
        protected ContextEnum currentContext = ContextEnum.Unknown;
        protected bool noAlias;

        public BaseQueryBuilder(FolkeConnection connection):this(connection.Driver)
        {
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
            beginSymbol = driver.BeginSymbol;
            endSymbol = driver.EndSymbol;
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

        public object[] Parameters
        {
            get { return parameters == null ? null : parameters.ToArray(); }
        }

        public MappedClass MappedClass
        {
            get { return baseMappedClass; }
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

        protected string GetTableAlias(MemberExpression tableAlias)
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
        protected void AppendField(string tableName, MemberInfo propertyInfo)
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
            else if (tableExpression is ParameterExpression && tableExpression.Type == defaultType && parameterTable != null)
            {
                AppendField(parameterTable.name, propertyInfo);
                return true;
            }
            else if (tableExpression is ParameterExpression && tableExpression.Type == parametersType)
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
    }
}