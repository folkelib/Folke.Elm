using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Orm
{
    public class MySqlDriver : IDatabaseDriver
    {
        public IDatabaseSettings Settings { get; private set; }

        public MySqlDriver(IDatabaseSettings settings)
        {
            Settings = settings;
        }

        public DbConnection CreateConnection()
        {
            return new MySqlConnection("Server=" + Settings.Host + "; Database=" + Settings.Database + "; Uid=" + Settings.User + "; Pwd=" + Settings.Password);
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


        public char BeginSymbol
        {
            get { return '`'; }
        }

        public char EndSymbol
        {
            get { return '`'; }
        }
    }
}
