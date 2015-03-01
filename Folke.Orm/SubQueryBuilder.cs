using System;
using System.Linq.Expressions;

namespace Folke.Orm
{
    public class SubQueryHelper<T, TP> where T : class, new()
    {
        public bool Exists(Action<FluentGenericQueryBuilder<T, TP>> expression)
        {
            return true;
        }

        public bool In(Expression<Action<T>> expression)
        {
            return true;
        }
    }
}
