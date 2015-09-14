using System;
using System.Linq.Expressions;

namespace Folke.Orm.Fluent
{
    public class FluentWhereExpressionBuilder<T, TMe>
    {
        private readonly BaseQueryBuilder queryBuilder;

        public FluentWhereExpressionBuilder(BaseQueryBuilder queryBuilder)
        {
            this.queryBuilder = queryBuilder;
        }

        public FluentWhereExpressionBuilder<T, TMe> Or(Expression<Func<T, bool>> expression)
        {
            queryBuilder.AppendOr();
            queryBuilder.AddExpression(expression.Body);
            return this;
        }
    }
}
