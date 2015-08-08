namespace Folke.Orm.Fluent
{
    using System;
    using System.Linq.Expressions;

    public class FluentOrderByBuilder<T, TMe> : FluentQueryableBuilder<T, TMe>, ILimitFluentBuilder<T, TMe>
    {
        public FluentOrderByBuilder(BaseQueryBuilder queryBuilder, Expression<Func<T, object>> expression)
            : base(queryBuilder)
        {
            queryBuilder.AppendOrderBy();
            queryBuilder.AddExpression(expression.Body);
        }

        public FluentOrderByBuilder<T, TMe> Desc()
        {
            QueryBuilder.Append("DESC");
            return this;
        }

        public FluentOrderByBuilder<T, TMe> Asc()
        {
            QueryBuilder.Append("ASC");
            return this;
        }

        public FluentOrderByBuilder<T, TMe> OrderBy<TV>(Expression<Func<T, TV>> column)
        {
            QueryBuilder.AppendOrderBy();
            QueryBuilder.AppendColumn(column.Body);
            return this;
        }
    }
}
