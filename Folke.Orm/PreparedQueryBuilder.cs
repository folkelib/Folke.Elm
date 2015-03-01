using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Orm
{
    public class PreparedQueryBuilder<T>
        where T : class, new()
    {
        protected FluentGenericQueryBuilder<T, FolkeTuple> query;
        private readonly Func<FluentGenericQueryBuilder<T, FolkeTuple>, FluentGenericQueryBuilder<T, FolkeTuple>> prepare;

        public PreparedQueryBuilder(Func<FluentGenericQueryBuilder<T, FolkeTuple>, FluentGenericQueryBuilder<T, FolkeTuple>> prepare)
        {
            this.prepare = prepare;
        }

        private FluentGenericQueryBuilder<T, FolkeTuple> GetQuery(IDatabaseDriver driver)
        {
            if (query == null)
            {
                query = new FluentGenericQueryBuilder<T, FolkeTuple>(driver);
                prepare.Invoke(query);
            }
            return query;
        }
        
        public IList<T> List(IFolkeConnection connection)
        {
            return GetQuery(connection.Driver).List(connection);
        }
    }

    public class PreparedQueryBuilder<T, U>
        where T : class, new()
    {
        protected FluentGenericQueryBuilder<T, FolkeTuple<U>> query;
        private readonly Func<FluentGenericQueryBuilder<T, FolkeTuple<U>>, FluentGenericQueryBuilder<T, FolkeTuple<U>>> prepare;

        public PreparedQueryBuilder(Func<FluentGenericQueryBuilder<T, FolkeTuple<U>>, FluentGenericQueryBuilder<T, FolkeTuple<U>>> prepare)
        {
            this.prepare = prepare;
        }

        private FluentGenericQueryBuilder<T, FolkeTuple<U>> GetQuery(IDatabaseDriver driver)
        {
            if (query == null)
            {
                query = new FluentGenericQueryBuilder<T, FolkeTuple<U>>(driver);
                prepare.Invoke(query);
            }
            return query;
        }

        public IList<T> List(IFolkeConnection connection, U param0)
        {
            return GetQuery(connection.Driver).List(connection, param0);
        }

        public T SingleOrDefault(IFolkeConnection connection, U param0)
        {
            return GetQuery(connection.Driver).SingleOrDefault(connection, param0);
        }
    }

    public class PreparedQueryBuilder<T, U, V>
        where T : class, new()
    {
        protected FluentGenericQueryBuilder<T, FolkeTuple<U, V>> query;
        private Func<FluentGenericQueryBuilder<T, FolkeTuple<U, V>>, FluentGenericQueryBuilder<T, FolkeTuple<U, V>>> prepare;

        public PreparedQueryBuilder(Func<FluentGenericQueryBuilder<T, FolkeTuple<U, V>>, FluentGenericQueryBuilder<T, FolkeTuple<U, V>>> prepare)
        {
            this.prepare = prepare;
        }

        private FluentGenericQueryBuilder<T, FolkeTuple<U, V>> GetQuery(IDatabaseDriver driver)
        {
            if (query == null)
            {
                query = new FluentGenericQueryBuilder<T, FolkeTuple<U, V>>(driver);
                prepare.Invoke(query);
            }
            return query;
        }

        public IList<T> List(IFolkeConnection connection, U param0, V param1)
        {
            return GetQuery(connection.Driver).List(connection, param0, param1);
        }

        public T SingleOrDefault(IFolkeConnection connection, U param0, V param1)
        {
            return GetQuery(connection.Driver).SingleOrDefault(connection, param0, param1);
        }
    }
}
