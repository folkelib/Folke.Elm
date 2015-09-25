using System;
using Folke.Elm.Fluent;

namespace Folke.Elm
{
    public class PreparedQueryBuilder<T>
        where T : class, new()
    {
        protected IQueryableCommand<T> query;
        private readonly Func<ISelectResult<T, FolkeTuple>, IQueryableCommand<T>> prepare;

        public PreparedQueryBuilder(Func<ISelectResult<T, FolkeTuple>, IQueryableCommand<T>> prepare)
        {
            this.prepare = prepare;
        }

        public IQueryableCommand<T> Build(IFolkeConnection connection)
        {
            if (query == null)
            {
                var select = FluentBaseBuilder<T, FolkeTuple>.Select(connection.Driver, connection.Mapper);
                query = prepare.Invoke(select);
            }
            return query.Build(connection);
        }
    }

    public class PreparedQueryBuilder<T, T0>
        where T : class, new()
    {
        protected IQueryableCommand<T> query;
        private readonly Func<ISelectResult<T, FolkeTuple<T0>>, IQueryableCommand<T>> prepare;

        public PreparedQueryBuilder(Func<ISelectResult<T, FolkeTuple<T0>>, IQueryableCommand<T>> prepare)
        {
            this.prepare = prepare;
        }

        public IQueryableCommand<T> Build(IFolkeConnection connection, T0 param0)
        {
            if (query == null)
            {
                var select = FluentBaseBuilder<T, FolkeTuple<T0>>.Select(connection.Driver, connection.Mapper);
                query = prepare.Invoke(select);
            }
            return query.Build(connection, param0);
        }
    }

    public class PreparedQueryBuilder<T, T0, T1>
        where T : class, new()
    {
        protected IQueryableCommand<T> query;
        private readonly Func<ISelectResult<T, FolkeTuple<T0,T1>>, IQueryableCommand<T>> prepare;

        public PreparedQueryBuilder(Func<ISelectResult<T, FolkeTuple<T0, T1>>, IQueryableCommand<T>> prepare)
        {
            this.prepare = prepare;
        }

        public IQueryableCommand<T> Build(IFolkeConnection connection, T0 param0, T1 param1)
        {
            if (query == null)
            {
                var select = FluentBaseBuilder<T, FolkeTuple<T0, T1>>.Select(connection.Driver, connection.Mapper);
                query = prepare.Invoke(select);
            }
            return query.Build(connection, param0, param1);
        }
    }

    public class PreparedQueryBuilder<T, T0, T1, T2>
        where T : class, new()
    {
        protected IQueryableCommand<T> query;
        private readonly Func<ISelectResult<T, FolkeTuple<T0, T1, T2>>, IQueryableCommand<T>> prepare;

        public PreparedQueryBuilder(Func<ISelectResult<T, FolkeTuple<T0, T1, T2>>, IQueryableCommand<T>> prepare)
        {
            this.prepare = prepare;
        }

        public IQueryableCommand<T> Build(IFolkeConnection connection, T0 param0, T1 param1, T2 param2)
        {
            if (query == null)
            {
                var select = FluentBaseBuilder<T, FolkeTuple<T0, T1, T2>>.Select(connection.Driver, connection.Mapper);
                query = prepare.Invoke(select);
            }
            return query.Build(connection, param0, param1);
        }
    }

    public class PreparedQueryBuilder<T, T0, T1, T2, T3>
        where T : class, new()
    {
        protected IQueryableCommand<T> query;
        private readonly Func<ISelectResult<T, FolkeTuple<T0, T1, T2, T3>>, IQueryableCommand<T>> prepare;

        public PreparedQueryBuilder(Func<ISelectResult<T, FolkeTuple<T0, T1, T2, T3>>, IQueryableCommand<T>> prepare)
        {
            this.prepare = prepare;
        }

        public IQueryableCommand<T> Build(IFolkeConnection connection, T0 param0, T1 param1, T2 param2, T3 param3)
        {
            if (query == null)
            {
                var select = FluentBaseBuilder<T, FolkeTuple<T0, T1, T2, T3>>.Select(connection.Driver, connection.Mapper);
                query = prepare.Invoke(select);
            }
            return query.Build(connection, param0, param1);
        }
    }

    public class PreparedQueryBuilder<T, T0, T1, T2, T3, T4>
        where T : class, new()
    {
        protected IQueryableCommand<T> query;
        private readonly Func<ISelectResult<T, FolkeTuple<T0, T1, T2, T3, T4>>, IQueryableCommand<T>> prepare;

        public PreparedQueryBuilder(Func<ISelectResult<T, FolkeTuple<T0, T1, T2, T3, T4>>, IQueryableCommand<T>> prepare)
        {
            this.prepare = prepare;
        }

        public IQueryableCommand<T> Build(IFolkeConnection connection, T0 param0, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            if (query == null)
            {
                var select = FluentBaseBuilder<T, FolkeTuple<T0, T1, T2, T3, T4>>.Select(connection.Driver, connection.Mapper);
                query = prepare.Invoke(select);
            }
            return query.Build(connection, param0, param1);
        }
    }
}
