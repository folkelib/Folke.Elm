using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Orm
{
    public class PreparedLoadBuilder<T>
        where T : class, IFolkeTable, new()
    {
        protected FluentGenericQueryBuilder<T, FolkeTuple<int>> query;
        private Expression<Func<T, object>>[] fetches;

        public PreparedLoadBuilder()
        {
        }

        public PreparedLoadBuilder(params Expression<Func<T, object>>[] fetches)
        {
            this.fetches = fetches;
        }

        private FluentGenericQueryBuilder<T, FolkeTuple<int>> GetQuery(IDatabaseDriver driver)
        {
            if (query == null)
            {
                if (fetches == null)
                {
                    query = new FluentGenericQueryBuilder<T, FolkeTuple<int>>(driver).SelectAll().From().Where((x, y) => x.Id == y.Item0);
                }
                else
                {
                    query = new FluentGenericQueryBuilder<T, FolkeTuple<int>>(driver).SelectAll();
                    foreach (var fetch in fetches)
                    {
                        query.AndAll(fetch);
                    }
                    query.From();
                    foreach (var fetch in fetches)
                    {
                        query.LeftJoinOnId(fetch);
                    }
                    query.Where((x, y) => x.Id == y.Item0);
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
