using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Folke.Orm
{
    public static class ExpressionHelpers
    {
        public static object Property(this object o, PropertyInfo propertyInfo)
        {
            return propertyInfo.GetValue(o);
        }

        public static bool Like(this string a, string pattern)
        {
            return a == pattern;
        }

        public static bool In<T>(this T a, IEnumerable<T> collection)
        {
            return collection.Contains(a);
        }
    }
}
