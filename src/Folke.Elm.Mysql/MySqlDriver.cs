using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Folke.Elm.InformationSchema;
using Folke.Elm.Mapping;
using MySql.Data.MySqlClient;

namespace Folke.Elm.Mysql
{
    public class MySqlDriver : IDatabaseDriver
    {
        public virtual DbConnection CreateConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        public string GetSqlType(PropertyInfo property, int maxLength = 0)
        {
            var type = property.PropertyType;
            if (type.IsGenericType)
                type = Nullable.GetUnderlyingType(type);

            if (type == typeof(bool))
            {
                return "TINYINT";
            }
            else if (type == typeof(short))
            {
                return "SMALLINT";
            }
            else if (type == typeof(int))
            {
                return "INT";
            }
            else if (type == typeof(long))
            {
                return "BIGINT";
            }
            else if (type == typeof(float))
            {
                return "FLOAT";
            }
            else if (type == typeof(double))
            {
                return "DOUBLE";
            }
            else if (type == typeof(DateTime))
            {
                return "DATETIME";
            }
            else if (type == typeof(TimeSpan))
            {
                return "INT";
            }
            else if (type.IsEnum)
            {
                return "VARCHAR(255)";
            }
            else if (type == typeof(decimal))
            {
                return "DECIMAL(15,5)";
            }
            else if (type == typeof(string))
            {
                if (maxLength != 0)
                {
                    if (maxLength > 255)
                        return "TEXT";
                    else
                        return "VARCHAR(" + maxLength + ")";
                }
                else
                    return "VARCHAR(255)";
            }
            else if (type == typeof(Guid))
            {
                return "CHAR(36)";
            }
            else
                throw new Exception("Unsupported type " + type.Name);
        }


        public bool EquivalentTypes(string firstType, string secondType)
        {
            firstType = firstType.ToLowerInvariant();
            secondType = secondType.ToLowerInvariant();

            if (firstType == secondType)
                return true;

            var parent = firstType.IndexOf('(');
            if (parent >= 0)
                firstType = firstType.Substring(0, parent);
            parent = secondType.IndexOf('(');
            if (parent >= 0)
                secondType = secondType.Substring(0, parent);
            if (firstType == secondType)
                return true;
            if (firstType.IndexOf("text") >= 0 && secondType.IndexOf("text") >= 0)
                return true;
            return false;
        }

        public IList<ColumnDefinition> GetColumnDefinitions(FolkeConnection connection, TypeMapping typeMap)
        {
            return connection.Select<Columns>().All().From().Where(x => x.TABLE_NAME == typeMap.TableName && x.TABLE_SCHEMA == connection.Database).List().Cast<ColumnDefinition>().ToList();
        }

        public IList<TableDefinition> GetTableDefinitions(FolkeConnection connection)
        {
            return connection.Select<Tables>().All().From().Where(t => t.Schema == connection.Database).List().Select(x => new TableDefinition { Name = x.Name, Schema = x.Schema }).ToList();
        }

        public SqlStringBuilder CreateSqlStringBuilder()
        {
            return new MysqlStringBuilder();
        }

        public object ConvertValueToParameter(IMapper mapper, object value)
        {
            if (value == null) return null;
            var parameterType = value.GetType();
            if (parameterType.GetTypeInfo().IsEnum)
            {
                return Enum.GetName(parameterType, value);
            }

            if (parameterType.GetTypeInfo().IsValueType || parameterType == typeof(string))
            {
                return value;
            }

            return mapper.GetTypeMapping(parameterType).Key.PropertyInfo.GetValue(value);
        }

        public object ConvertReaderValueToProperty(object readerValue, Type propertyType)
        {
            return readerValue;
        }

        public bool CanAddIndexInCreateTable()
        {
            return true;
        }
    }
}
