using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Folke.Orm
{
    public static class TableHelpers
    {
        public static string GetTableName(Type type)
        {
            var attribute = type.GetCustomAttribute<TableAttribute>();
            return attribute != null ? attribute.Name : type.Name;
        }

        public static bool IsReadOnly(PropertyInfo property)
        {
            return IsAutomatic(property);
        }

        public static bool IsForeign(Type type)
        {
            return type.GetInterface("IFolkeTable") != null || type.GetCustomAttribute<TableAttribute>() != null;
        }

        public static bool IsAutomatic(PropertyInfo propertyInfo)
        {
            return IsKey(propertyInfo) && propertyInfo.PropertyType == typeof(int);
        }

        public static bool IsIgnored(Type type)
        {
            return type.IsGenericType && Nullable.GetUnderlyingType(type) == null;
        }

        public static Type GetMemberType(MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Property:
                    return ((PropertyInfo) memberInfo).PropertyType;
                default:
                    throw new Exception("Insupported member info");
            }
        }

        public static string GetColumnName(MemberInfo propertyInfo)
        {
            var columnAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();
            if (columnAttribute == null)
            {
                if (IsForeign(GetMemberType(propertyInfo)))
                {
                    return propertyInfo.Name + "_id";
                }
                return propertyInfo.Name;
            }
            return columnAttribute.Name;
        }

        public static PropertyInfo GetKey(Type type)
        {
            if (type.GetInterface("IFolkeTable") != null)
                return type.GetProperty("Id");
            return type.GetProperties().SingleOrDefault(x => x.GetCustomAttribute<KeyAttribute>() != null);
        }

        public static string GetKeyColumName(Type type)
        {
            if (type.GetInterface("IFolkeTable") != null)
            {
                return "Id";
            }
            var column = type.GetProperties().Single(x => x.GetCustomAttribute<KeyAttribute>() != null);
            return GetColumnName(column);
        }

        public static bool IsKey(MemberInfo propertyInfo)
        {
            if (propertyInfo.GetCustomAttribute<KeyAttribute>() != null)
                return true;
            if (propertyInfo.DeclaringType.GetInterface("IFolkeTable") != null)
                return propertyInfo.Name == "Id";
            return false;
        }

        public static PropertyInfo GetExpressionPropertyInfo<T>(Expression<Func<T, object>> column) where T : class, new()
        {
            MemberExpression member;
            if (column.Body.NodeType == ExpressionType.Convert)
                member = (MemberExpression)((UnaryExpression)column.Body).Operand;
            else
                member = (MemberExpression)column.Body;
            return member.Member as PropertyInfo;                
        }
    }
}
