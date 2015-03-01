using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Reflection;
using Folke.Orm.InformationSchema;

namespace Folke.Orm.Sqlite
{
    public class SqliteDriver : IDatabaseDriver
    {
        public IDatabaseSettings Settings { get; private set; }

        public SqliteDriver()
        {
        }

        public DbConnection CreateConnection(string connectionString)
        {
            return new SQLiteConnection(connectionString);
        }

        public string GetSqlType(PropertyInfo property)
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
                var attribute = property.GetCustomAttribute<ColumnAttribute>();
                if (attribute != null && attribute.MaxLength != 0)
                {
                    if (attribute.MaxLength > 255)
                        return "TEXT";
                    else
                        return "VARCHAR(" + attribute.MaxLength + ")";
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
            var list = new List<ColumnDefinition>();
            using (var command = connection.OpenCommand())
            {
                command.CommandText = string.Format("PRAGMA table_info({0})", typeMap.TableName);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ColumnDefinition
                        {
                            ColumnName = reader.GetString(1),
                            ColumnType = reader.GetString(2)
                        });
                    }
                }
            }
            return list;
        }

        public IList<TableDefinition> GetTableDefinitions(FolkeConnection connection, string p)
        {
            var list = new List<TableDefinition>();
            using (var command = connection.OpenCommand())
            {
                command.CommandText = ".tables";
                using (var reader = command.ExecuteReader())
                {
                    list.Add(new TableDefinition
                    {
                        Name = reader.GetString(0)
                    });
                }
            }
            return list;
        }

        public SqlStringBuilder CreateSqlStringBuilder()
        {
            return new SqliteStringBuilder();
        }
    }
}
