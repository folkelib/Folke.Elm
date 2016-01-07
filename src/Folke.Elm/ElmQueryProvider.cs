using System.Linq;
using System.Linq.Expressions;

namespace Folke.Elm
{
    public class ElmQueryProvider : IQueryProvider
    {
        private readonly IFolkeConnection connection;

        public ElmQueryProvider(IFolkeConnection connection)
        {
            this.connection = connection;
        }

        public IFolkeConnection Connection => connection;

        public IQueryable CreateQuery(Expression expression)
        {
            return new ElmQueryable(expression, expression.Type, this);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new ElmQueryable<TElement>(expression, this);
        }

        public object Execute(Expression expression)
        {
            var builder = new BaseQueryBuilder(connection);
            builder.AddExpression(expression);
            return builder.Scalar();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var builder = new BaseQueryBuilder(connection);
            builder.AddExpression(expression);
            return builder.Scalar<TResult>();
        }
    }
}
