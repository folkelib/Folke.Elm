using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Folke.Elm.Fluent;
using Folke.Elm.Mapping;
using Folke.Elm.Visitor;

namespace Folke.Elm
{
    public class ElmQueryable<T> : ElmQueryable, IOrderedQueryable<T>
    {
        public static ElmQueryable<T> Build(Expression<Action<ISelectResult<T, FolkeTuple>>> expression, ElmQueryProvider provider)
        {
            return new ElmQueryable<T>(expression.Body, provider);
        }

        public ElmQueryable(Expression expression, ElmQueryProvider provider) : base(expression, typeof(T), provider)
        {
        }

        public ElmQueryable(ElmQueryProvider provider) : base(null, typeof(T), provider)
        {
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            var queryBuilder = new BaseQueryBuilder<T>(queryProvider.Connection);
            var from = queryBuilder.ParseExpression(Expression, ParseOptions.RegisterTables);
            var select =  new Select(queryBuilder.ParseSelectedColumn(queryBuilder.DefaultTable), from);
            select.Accept(queryBuilder.StringBuilder);
            return queryBuilder.GetEnumerator();
        }
    }

    public class ElmQueryable : IOrderedQueryable
    {
        protected readonly ElmQueryProvider queryProvider;

        public ElmQueryable(Expression expression, Type elementType, ElmQueryProvider queryProvider)
        {
            this.queryProvider = queryProvider;
            Expression = expression ?? Expression.Constant(this);
            ElementType = elementType;
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Type ElementType { get; }
        public Expression Expression { get; }
        public IQueryProvider Provider => queryProvider;
    }
}
