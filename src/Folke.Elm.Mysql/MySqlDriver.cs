using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Folke.Elm.InformationSchema;
using Folke.Elm.Mapping;
using MySql.Data.MySqlClient;
using Folke.Elm.Fluent;

namespace Folke.Elm.Mysql
{
    public class MySqlDriver : IDatabaseDriver
    {
        public bool HasBooleanType { get; } = true;

        public virtual DbConnection CreateConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        public string GetSqlType(PropertyMapping property, bool foreignKey)
        {
            var type = property.PropertyInfo.PropertyType;
            return GetSqlType(type, property.MaxLength);
        }

        private static string GetSqlType(Type type, int maxLength)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType)
                type = Nullable.GetUnderlyingType(type);

            if (type == typeof (bool))
            {
                return "TINYINT";
            }
            else if (type == typeof (short))
            {
                return "SMALLINT";
            }
            else if (type == typeof (int))
            {
                return "INT";
            }
            else if (type == typeof (long))
            {
                return "BIGINT";
            }
            else if (type == typeof (float))
            {
                return "FLOAT";
            }
            else if (type == typeof (double))
            {
                return "DOUBLE";
            }
            else if (type == typeof (DateTime))
            {
                return "DATETIME";
            }
            else if (type == typeof (TimeSpan))
            {
                return "INT";
            }
            else if (typeInfo.IsEnum)
            {
                if (typeInfo.GetCustomAttribute(typeof(FlagsAttribute)) != null)
                    return GetSqlType(Enum.GetUnderlyingType(type), maxLength);
                return "VARCHAR(255)";
            }
            else if (type == typeof (decimal))
            {
                return "DECIMAL(15,5)";
            }
            else if (type == typeof (string))
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
            else if (type == typeof (Guid))
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
            if (firstType.IndexOf("text", StringComparison.Ordinal) >= 0 && secondType.IndexOf("text", StringComparison.Ordinal) >= 0)
                return true;
            return false;
        }

        public IList<IColumnDefinition> GetColumnDefinitions(FolkeConnection connection, TypeMapping typeMap)
        {
            return connection.Select<MySqlColumnDefinition>().All().From().Where(x => x.TABLE_NAME == typeMap.TableName && x.TABLE_SCHEMA == connection.Database).ToList().Cast<IColumnDefinition>().ToList();
        }

        public IList<TableDefinition> GetTableDefinitions(FolkeConnection connection)
        {
            return connection.Select<Tables>().All().From().Where(t => t.Schema == connection.Database).ToList().Select(x => new TableDefinition { Name = x.Name, Schema = x.Schema }).ToList();
        }

        public SqlStringBuilder CreateSqlStringBuilder()
        {
            return new MysqlStringBuilder();
        }

        public object ConvertValueToParameter(IMapper mapper, object value)
        {
            if (value == null) return DBNull.Value;
            var parameterType = value.GetType();
            if (parameterType.GetTypeInfo().IsEnum)
            {
                if (parameterType.GetTypeInfo().GetCustomAttribute(typeof(FlagsAttribute)) != null)
                {
                    return Convert.ChangeType(value, Enum.GetUnderlyingType(parameterType));
                }
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

        public bool CanDoMultipleActionsInAlterTable()
        {
            return true;
        }

        public object ConvertReaderValueToValue(DbDataReader reader, Type type, int index)
        {
            object value;
            if (type.GetTypeInfo().IsGenericType)
                type = Nullable.GetUnderlyingType(type);

            if (type == typeof(string))
                value = reader.GetString(index);
            else if (type == typeof(int))
                value = reader.GetInt32(index);
            else if (type == typeof(long))
                value = reader.GetInt64(index);
            else if (type == typeof(float))
                value = reader.GetFloat(index);
            else if (type == typeof(double))
                value = reader.GetDouble(index);
            else if (type == typeof(decimal))
                value = reader.GetDecimal(index);
            else if (type == typeof(TimeSpan))
            {
                value = new TimeSpan(0, 0, reader.GetInt32(index));
            }
            else if (type == typeof(DateTime))
            {
                var date = reader.GetDateTime(index);
                value = date.ToLocalTime().ToUniversalTime(); // Allow to force UTC (from Unspecified)
            }
            else if (type == typeof(bool))
                value = (sbyte)reader.GetValue(index) == 1;
            else if (type == typeof(Guid))
                value = reader.GetGuid(index);
            else if (type.GetTypeInfo().IsEnum)
            {
                if (type.GetTypeInfo().GetCustomAttribute(typeof(FlagsAttribute)) != null)
                {
                    var numberValue = reader.GetValue(index);
                    value = Enum.ToObject(type, numberValue);
                }
                else
                {
                    var text = reader.GetString(index);
                    var names = Enum.GetNames(type);
                    var enumIndex = 0;
                    for (var i = 0; i < names.Length; i++)
                    {
                        if (names[i] == text)
                        {
                            enumIndex = i;
                            break;
                        }
                    }
                    value = Enum.GetValues(type).GetValue(enumIndex);
                }
            }
            else
                value = null;
            return value;
        }
    }
}
