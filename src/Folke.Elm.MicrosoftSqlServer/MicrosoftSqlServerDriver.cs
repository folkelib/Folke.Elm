using System;
using System.Collections.Generic;
using System.Reflection;
using Folke.Elm.InformationSchema;
using Folke.Elm.Mapping;
using System.Data.Common;
using System.Data.SqlClient;
using Folke.Elm.Fluent;
using System.Linq;

namespace Folke.Elm.MicrosoftSqlServer
{
    public class MicrosoftSqlServerDriver : IDatabaseDriver
    {
        public MicrosoftSqlServerDriver()
        {
        }

        public bool CanAddIndexInCreateTable()
        {
            return true;
        }

        public bool CanDoMultipleActionsInAlterTable()
        {
            return false;
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

            if (type == typeof (Guid))
                value = reader.GetGuid(index);
            if (type == typeof(string))
                value = reader.GetString(index);
            else if (type == typeof (byte))
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
                if (type.GetTypeInfo().GetCustomAttribute(typeof (FlagsAttribute)) != null)
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

        public object ConvertValueToParameter(IMapper mapper, object value)
        {
            if (value == null) return DBNull.Value;
            var parameterType = value.GetType();
            if (parameterType.GetTypeInfo().IsEnum)
            {
                if (parameterType.GetTypeInfo().GetCustomAttribute(typeof (FlagsAttribute)) != null)
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

        public bool HasBooleanType { get; } = false;

        public DbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public SqlStringBuilder CreateSqlStringBuilder()
        {
            return new MicrosoftSqlServerStringBuilder();
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
            var tableSchema = typeMap.TableSchema ?? "dbo";
            return connection.Select<ColumnDefinition>().All().From().Where(x => x.TABLE_NAME == typeMap.TableName && x.TABLE_SCHEMA == tableSchema).ToList().Cast<IColumnDefinition>().ToList();
        }

        public string GetSqlType(PropertyInfo property, int maxLength)
        {
            var type = property.PropertyType;
            return GetSqlType(type, maxLength);
        }

        private string GetSqlType(Type type, int maxLength)
        {
            if (type.GetTypeInfo().IsGenericType)
                type = Nullable.GetUnderlyingType(type);

            if (type == typeof (bool))
            {
                return "BIT";
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
                return "REAL";
            }
            else if (type == typeof (double))
            {
                return "FLOAT";
            }
            else if (type == typeof (DateTime))
            {
                return "DATETIME2";
            }
            else if (type == typeof (TimeSpan))
            {
                return "INT";
            }
            else if (type.GetTypeInfo().IsEnum)
            {
                if (type.GetTypeInfo().GetCustomAttribute(typeof (FlagsAttribute)) != null)
                    return GetSqlType(Enum.GetUnderlyingType(type), maxLength);
                return "NVARCHAR(255)";
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
                        return "NVARCHAR(" + maxLength + ")";
                }
                else
                    return "NVARCHAR(255)";
            }
            else if (type == typeof (Guid))
            {
                return "UNIQUEIDENTIFIER";
            }
            else
                throw new Exception("Unsupported type " + type.Name);
        }

        public IList<TableDefinition> GetTableDefinitions(FolkeConnection connection)
        {
            return connection.Select<Tables>().All().From().ToList().Select(x => new TableDefinition { Name = x.Name, Schema = x.Schema }).ToList();
        }
    }
}
