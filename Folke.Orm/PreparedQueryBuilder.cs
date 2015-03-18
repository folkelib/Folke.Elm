using System;
using System.Collections.Generic;
using Folke.Orm.Fluent;

namespace Folke.Orm
{
    public class PreparedQueryBuilder<T>
        where T : class, new()
    {
        protected FluentSelectBuilder<T, FolkeTuple> query;
        private readonly Func<FluentSelectBuilder<T, FolkeTuple>, FluentQueryableBuilder<T, FolkeTuple>> prepare;

        public PreparedQueryBuilder(Func<FluentSelectBuilder<T, FolkeTuple>, FluentQueryableBuilder<T, FolkeTuple>> prepare)
        {
            this.prepare = prepare;
        }

        private FluentSelectBuilder<T, FolkeTuple> GetQuery(IDatabaseDriver driver)
        {
            if (query == null)
            {
                query = new FluentSelectBuilder<T, FolkeTuple>(driver);
                prepare.Invoke(query);
            }
            return query;
        }
        
        public IList<T> List(IFolkeConnection connection)
        {
            return GetQuery(connection.Driver).List(connection);
        }
    }

    public class PreparedQueryBuilder<T, TU>
        where T : class, new()
    {
        protected FluentSelectBuilder<T, FolkeTuple<TU>> query;
        private readonly Func<FluentSelectBuilder<T, FolkeTuple<TU>>, FluentQueryableBuilder<T, FolkeTuple<TU>>> prepare;

        public PreparedQueryBuilder(Func<FluentSelectBuilder<T, FolkeTuple<TU>>, FluentQueryableBuilder<T, FolkeTuple<TU>>> prepare)
        {
            this.prepare = prepare;
        }

        private FluentSelectBuilder<T, FolkeTuple<TU>> GetQuery(IDatabaseDriver driver)
        {
            if (query == null)
            {
                query = new FluentSelectBuilder<T, FolkeTuple<TU>>(driver);
                prepare.Invoke(query);
            }
            return query;
        }

        public IList<T> List(IFolkeConnection connection, TU param0)
        {
            return GetQuery(connection.Driver).List(connection, param0);
        }

        public T SingleOrDefault(IFolkeConnection connection, TU param0)
        {
            return GetQuery(connection.Driver).SingleOrDefault(connection, param0);
        }
    }

    public class PreparedQueryBuilder<T, TU, TV>
        where T : class, new()
    {
        protected FluentSelectBuilder<T, FolkeTuple<TU, TV>> query;
        private readonly Func<FluentSelectBuilder<T, FolkeTuple<TU, TV>>, FluentQueryableBuilder<T, FolkeTuple<TU, TV>>> prepare;

        public PreparedQueryBuilder(Func<FluentSelectBuilder<T, FolkeTuple<TU, TV>>, FluentQueryableBuilder<T, FolkeTuple<TU, TV>>> prepare)
        {
            this.prepare = prepare;
        }

        private FluentQueryableBuilder<T, FolkeTuple<TU, TV>> GetQuery(IDatabaseDriver driver)
        {
            if (query == null)
            {
                query = new FluentSelectBuilder<T, FolkeTuple<TU, TV>>(driver);
                prepare.Invoke(query);
            }
            return query;
        }

        public IList<T> List(IFolkeConnection connection, TU param0, TV param1)
        {
            return GetQuery(connection.Driver).List(connection, param0, param1);
        }

        public T SingleOrDefault(IFolkeConnection connection, TU param0, TV param1)
        {
            return GetQuery(connection.Driver).SingleOrDefault(connection, param0, param1);
        }

        public T1 Scalar<T1>(FolkeConnection connection, TU param0, TV param1)
        {
            return GetQuery(connection.Driver).Scalar<T1>(connection, param0, param1);
        }
    }
}
