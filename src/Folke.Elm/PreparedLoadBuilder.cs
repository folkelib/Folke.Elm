using System;
using System.Linq.Expressions;
using Folke.Elm.Fluent;
using Folke.Elm.Mapping;

namespace Folke.Elm
{
    public class PreparedLoadBuilder<T>
        where T : class, IFolkeTable, new()
    {
        protected IQueryableCommand<T> query;

        private readonly Expression<Func<T, object>>[] fetches;

        public PreparedLoadBuilder()
        {
        }

        public PreparedLoadBuilder(params Expression<Func<T, object>>[] fetches)
        {
            this.fetches = fetches;
        }

        private IQueryableCommand<T> GetQuery(IDatabaseDriver driver, IMapper mapper)
        {
            if (query == null)
            {
                if (fetches == null)
                {
                    query = FluentBaseBuilder<T, FolkeTuple<int>>.Select(new BaseQueryBuilder(driver, mapper)).All().From().Where((x, y) => x.Id == y.Item0);
                }
                else
                {
                    var selectQuery = FluentBaseBuilder<T, FolkeTuple<int>>.Select(new BaseQueryBuilder(driver, mapper)).All();
                    foreach (var fetch in fetches)
                    {
                        selectQuery.All(fetch);
                    }

                    var fromQuery = selectQuery.From();
                    foreach (var fetch in fetches)
                    {
                        fromQuery.LeftJoinOnId(fetch);
                    }

                    fromQuery.Where((x, y) => x.Id == y.Item0);
                    query = fromQuery;
                }
            }

            return query;
        }

        public T Load(IFolkeConnection connection, int id)
        {
            return GetQuery(connection.Driver, connection.Mapper).Build(connection, id).First();
        }

        public T Get(IFolkeConnection connection, int id)
        {
            return GetQuery(connection.Driver, connection.Mapper).Build(connection, id).FirstOrDefault();
        }
    }

}
