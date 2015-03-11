using System;
using System.Linq.Expressions;

namespace Folke.Orm
{
    using Folke.Orm.Fluent;

    public class PreparedLoadBuilder<T>
        where T : class, IFolkeTable, new()
    {
        protected FluentQueryableBuilder<T, FolkeTuple<int>> query;

        private readonly Expression<Func<T, object>>[] fetches;

        public PreparedLoadBuilder()
        {
        }

        public PreparedLoadBuilder(params Expression<Func<T, object>>[] fetches)
        {
            this.fetches = fetches;
        }

        private FluentQueryableBuilder<T, FolkeTuple<int>> GetQuery(IDatabaseDriver driver)
        {
            if (query == null)
            {
                if (fetches == null)
                {
                    query = new FluentSelectBuilder<T, FolkeTuple<int>>(driver).All().From().Where((x, y) => x.Id == y.Item0);
                }
                else
                {
                    var selectQuery = new FluentSelectBuilder<T, FolkeTuple<int>>(driver).All();
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
            return GetQuery(connection.Driver).Single(connection, id);
        }

        public T Get(IFolkeConnection connection, int id)
        {
            return GetQuery(connection.Driver).SingleOrDefault(connection, id);
        }
    }

}
