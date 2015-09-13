using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Folke.Orm
{
    public static class TableHelpers
    {
        public static PropertyInfo GetExpressionPropertyInfo<T>(Expression<Func<T, object>> column)
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
