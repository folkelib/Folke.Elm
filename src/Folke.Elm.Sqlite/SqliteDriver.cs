using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using Folke.Elm.InformationSchema;
using Folke.Elm.Mapping;
using Microsoft.Data.Sqlite;

namespace Folke.Elm.Sqlite
{
    public class SqliteDriver : IDatabaseDriver
    {
        public IDatabaseSettings Settings { get; private set; }

        public SqliteDriver()
        {
        }

        public bool HasBooleanType { get; } = true;

        public DbConnection CreateConnection(string connectionString)
        {
            return new SqliteConnection(connectionString);
        }

        public string GetSqlType(PropertyInfo property, int maxLength)
        {
            var type = property.PropertyType;
            if (type.GetTypeInfo().IsGenericType)
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
                return "INTEGER";
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
            else if (type.GetTypeInfo().IsEnum)
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

            if (firstType.StartsWith("int") && secondType.StartsWith("int")) return true;

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
            var list = new List<IColumnDefinition>();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"PRAGMA table_info({typeMap.TableName})";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new SqliteColumnDefinition
                        {
                            ColumnName = reader.GetString(1),
                            ColumnType = reader.GetString(2)
                        });
                    }
                }
            }
            return list;
        }

        public IList<TableDefinition> GetTableDefinitions(FolkeConnection connection)
        {
            var list = new List<TableDefinition>();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"SELECT name FROM {connection.Database}.sqlite_master WHERE TYPE='table'";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new TableDefinition
                        {
                            Name = reader.GetString(0)
                        });
                    }
                }
            }
            return list;
        }

        public SqlStringBuilder CreateSqlStringBuilder()
        {
            return new SqliteStringBuilder();
        }

        public object ConvertValueToParameter(IMapper mapper, object value)
        {
            if (value == null) return DBNull.Value;
            var parameterType = value.GetType();
            if (parameterType.GetTypeInfo().IsEnum)
            {
                return Enum.GetName(parameterType, value);
            }

            if (parameterType == typeof (Guid))
            {
                return value.ToString();
            }

            if (parameterType.GetTypeInfo().IsValueType || parameterType == typeof(string))
            {
                return value;
            }

            value = mapper.GetTypeMapping(parameterType).Key.PropertyInfo.GetValue(value);
            if (value is Guid) return value.ToString();
            return value;
        }

        public object ConvertReaderValueToProperty(object readerValue, Type propertyType)
        {
            if (propertyType == readerValue.GetType())
                return readerValue;

            if (propertyType == typeof (Guid))
                return Guid.Parse((string) readerValue);

            return Convert.ChangeType(readerValue, propertyType);
        }

        public bool CanAddIndexInCreateTable()
        {
            return false;
        }

        public bool CanDoMultipleActionsInAlterTable()
        {
            return false;
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
                value = reader.GetBoolean(index);
            else if (type.GetTypeInfo().IsEnum)
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
            else
                value = null;
            return value;
        }
    }
}
