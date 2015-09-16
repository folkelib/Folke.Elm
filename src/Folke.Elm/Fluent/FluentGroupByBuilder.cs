using System;
using System.Linq.Expressions;

namespace Folke.Elm.Fluent
{
    public class FluentGroupByBuilder<T, TMe, TU> : FluentQueryableBuilder<T, TMe>
    {
        public FluentGroupByBuilder(BaseQueryBuilder queryBuilder, Expression<Func<T, TU>> column)
            : base(queryBuilder)
        {
            queryBuilder.AppendGroupBy();
            queryBuilder.AppendColumn(column.Body);
        }
    }
}
