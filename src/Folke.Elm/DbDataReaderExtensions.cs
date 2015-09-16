using System;
using System.Data.Common;
using System.Reflection;

namespace Folke.Elm
{
    internal static class DbDataReaderExtensions
    {
        public static T GetTypedValue<T>(this DbDataReader reader, int index)
        {
            return (T) reader.GetTypedValue(typeof(T), index);
        }

        public static object GetTypedValue(this DbDataReader reader, Type type, int index)
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
            else if (type == typeof (decimal))
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
