using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Folke.Elm.Fluent;
using Folke.Elm.InformationSchema;
using Folke.Elm.Mapping;
using Npgsql;

namespace Folke.Elm.PostgreSql
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class PostgreSqlDriver : IDatabaseDriver
    {
        public bool HasBooleanType => true;

        public DbConnection CreateConnection(string connectionString)
        {
            return new NpgsqlConnection(connectionString);
        }

        public string GetSqlType(PropertyMapping property, bool foreignKey)
        {
            var type = property.PropertyInfo.PropertyType;
            return GetSqlType(type, property.MaxLength, property.IsAutomatic && !foreignKey);
        }

        private string GetSqlType(Type type, int maxLength, bool isAutomatic)
        { 
            if (type.GetTypeInfo().IsGenericType)
                type = Nullable.GetUnderlyingType(type);

            if (type == typeof(bool))
            {
                return "BOOLEAN";
            }
            else if (type == typeof(short))
            {
                if (isAutomatic) return "SMALLSERIAL";
                return "SMALLINT";
            }
            else if (type == typeof(int))
            {
                if (isAutomatic) return "SERIAL";
                return "INTEGER";
            }
            else if (type == typeof(long))
            {
                if (isAutomatic) return "BIGSERIAL";
                return "BIGINT";
            }
            else if (type == typeof(float))
            {
                return "REAL";
            }
            else if (type == typeof(double))
            {
                return "DOUBLE PRECISION";
            }
            else if (type == typeof(DateTime))
            {
                return "TIMESTAMP";
            }
            else if (type == typeof(TimeSpan))
            {
                return "INTEGER";
            }
            else if (type.GetTypeInfo().IsEnum)
            {
                if (type.GetTypeInfo().GetCustomAttribute(typeof(FlagsAttribute)) != null)
                    return GetSqlType(Enum.GetUnderlyingType(type), maxLength, isAutomatic);
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
                return "UUID";
            }
            else
                throw new Exception("Unsupported type " + type.Name);
        }

        private static readonly List<HashSet<string>> equivalences = new List<HashSet<string>>
        {
            new HashSet<string> {"varchar", "character varying"},
            new HashSet<string> {"character", "char"},
            new HashSet<string> {"serial", "integer", "int"},
            new HashSet<string> {"bigserial", "bigint"},
            new HashSet<string> {"smallserial", "smallint"},
            new HashSet<string> {"timestamp", "timestamp without time zone" }
        };

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
            foreach (var equivalence in equivalences)
            {
                if (equivalence.Contains(firstType) && equivalence.Contains(secondType))
                    return true;
            }
            return false;
        }

        public IList<IColumnDefinition> GetColumnDefinitions(FolkeConnection connection, TypeMapping typeMap)
        {
            var tableSchema = typeMap.TableSchema ?? "public";
            return connection.Select<PostgreSqlColumnDefinition>().All().From().Where(x => x.TableName == typeMap.TableName && x.TableSchema == tableSchema).ToList().Cast<IColumnDefinition>().ToList();
        }

        public IList<TableDefinition> GetTableDefinitions(FolkeConnection connection)
        {
            return connection.Select<PostgreSqlTableDefinition>().All().From().ToList().Select(x => new TableDefinition { Name = x.Name, Schema = x.Schema }).ToList();
        }

        public SqlStringBuilder CreateSqlStringBuilder()
        {
            return new PostgreSqlStringBuilder();
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

        public object ConvertReaderValueToValue(DbDataReader reader, Type type, int index)
        {
            object value;
            if (type.GetTypeInfo().IsGenericType)
                type = Nullable.GetUnderlyingType(type);

            if (type == typeof(Guid))
                value = reader.GetGuid(index);
            else if (type == typeof(string))
                value = reader.GetString(index);
            else if (type == typeof(byte))
                value = reader.GetByte(index);
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

        public bool CanAddIndexInCreateTable()
        {
            return false;
        }

        public bool CanDoMultipleActionsInAlterTable()
        {
            return true;
        }
    }
}
