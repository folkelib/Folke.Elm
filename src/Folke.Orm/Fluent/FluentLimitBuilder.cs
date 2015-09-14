namespace Folke.Orm.Fluent
{
    using System;
    using System.Globalization;
    using System.Linq.Expressions;

    public class FluentLimitBuilder<T, TMe> : FluentQueryableBuilder<T, TMe>
    {
        public FluentLimitBuilder(BaseQueryBuilder queryBuilder, int offset, int count)
            : base(queryBuilder)
        {
            queryBuilder.Append("LIMIT");
            queryBuilder.Append(offset.ToString(CultureInfo.InvariantCulture));
            queryBuilder.Append(",");
            queryBuilder.Append(count.ToString(CultureInfo.InvariantCulture));
        }

        public FluentLimitBuilder(BaseQueryBuilder queryBuilder, Expression<Func<T, int>> offset, int count)
            : base(queryBuilder)
        {
            queryBuilder.Append("LIMIT");
            queryBuilder.AddExpression(offset.Body);
            queryBuilder.Append(",");
            queryBuilder.Append(count.ToString(CultureInfo.InvariantCulture));
        }

        public FluentLimitBuilder(BaseQueryBuilder queryBuilder, Expression<Func<T, TMe, int>> offset, int count)
            : base(queryBuilder)
        {
            queryBuilder.Append("LIMIT");
            queryBuilder.AddExpression(offset.Body);
            queryBuilder.Append(",");
            queryBuilder.Append(count.ToString(CultureInfo.InvariantCulture));
       }
    }
}
