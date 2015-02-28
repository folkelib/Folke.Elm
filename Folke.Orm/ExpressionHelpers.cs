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
    }
}
