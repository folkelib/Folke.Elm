namespace Folke.Orm.Fluent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public abstract class FluentQueryableBuilder<T, TMe> : FluentBaseBuilder<T, TMe>
    {
        protected FluentQueryableBuilder(BaseQueryBuilder queryBuilder)
            : base(queryBuilder)
        {
        }

        public object Scalar(FolkeConnection folkeConnection, params object[] commandParameters)
        {
            return folkeConnection.Scalar(baseQueryBuilder.Sql, commandParameters);
        }

        public object Scalar()
        {
            return baseQueryBuilder.Connection.Scalar(baseQueryBuilder.Sql, baseQueryBuilder.Parameters == null ? null : baseQueryBuilder.Parameters.ToArray());
        }

        public async Task<object> ScalarAsync(FolkeConnection folkeConnection, params object[] commandParameters)
        {
            return await folkeConnection.ScalarAsync(baseQueryBuilder.Sql, commandParameters);
        }

        public async Task<object> ScalarAsync()
        {
            return await baseQueryBuilder.Connection.ScalarAsync(baseQueryBuilder.Sql, baseQueryBuilder.Parameters == null ? null : baseQueryBuilder.Parameters.ToArray());
        }

        /// <summary>
        /// Assumes that the result is one scalar value and nothing else, get this
        /// </summary>
        /// <typeparam name="TU">The scalar value type</typeparam>
        /// <param name="folkeConnection">A connection</param>
        /// <param name="commandParameters">Optional commandParameters if the query had commandParameters</param>
        /// <returns>A single value</returns>
        public TU Scalar<TU>(FolkeConnection folkeConnection, params object[] commandParameters)
        {
            return folkeConnection.Scalar<TU>(baseQueryBuilder.Sql, commandParameters);
        }

        public TU Scalar<TU>()
        {
            return baseQueryBuilder.Connection.Scalar<TU>(baseQueryBuilder.Sql, baseQueryBuilder.Parameters == null ? null : baseQueryBuilder.Parameters.ToArray());
        }

        public async Task<TU> ScalarAsync<TU>(FolkeConnection folkeConnection, params object[] commandParameters)
        {
            return await folkeConnection.ScalarAsync<TU>(baseQueryBuilder.Sql, commandParameters);
        }

        public async Task<TU> ScalarAsync<TU>()
        {
            return await baseQueryBuilder.Connection.ScalarAsync<TU>(baseQueryBuilder.Sql, baseQueryBuilder.Parameters == null ? null : baseQueryBuilder.Parameters.ToArray());
        }


        public T Single(IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = folkeConnection.CreateCommand(baseQueryBuilder.Sql, commandParameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new Exception("No result found");
                    var value = baseQueryBuilder.MappedClass.Read(folkeConnection, typeof(T), reader);
                    reader.Close();
                    return (T)value;
                }
            }
        }

        /// <summary>
        /// A single result. Throws an exception is there is no result
        /// </summary>
        /// <returns>A single value</returns>
        public T Single()
        {
            return Single(baseQueryBuilder.Connection, baseQueryBuilder.Parameters);
        }

        public async Task<T> SingleAsync(IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = folkeConnection.CreateCommand(baseQueryBuilder.Sql, commandParameters))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!reader.Read())
                        throw new Exception("No result found");
                    var value = baseQueryBuilder.MappedClass.Read(folkeConnection, typeof(T), reader);
                    reader.Close();
                    return (T)value;
                }
            }
        }

        /// <summary>
        /// A single result
        /// </summary>
        /// <returns>The value</returns>
        public async Task<T> SingleAsync()
        {
            return await SingleAsync(baseQueryBuilder.Connection, baseQueryBuilder.Parameters);
        }

        public T SingleOrDefault(IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = folkeConnection.CreateCommand(baseQueryBuilder.Sql, commandParameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return default(T);
                    var value = baseQueryBuilder.MappedClass.Read(folkeConnection, typeof(T), reader);
                    reader.Close();
                    return (T)value;
                }
            }
        }

        public T SingleOrDefault()
        {
            return SingleOrDefault(baseQueryBuilder.Connection, baseQueryBuilder.Parameters);
        }

        public async Task<T> SingleOrDefaultAsync(IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = folkeConnection.CreateCommand(baseQueryBuilder.Sql, commandParameters))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!reader.Read())
                        return default(T);
                    var value = baseQueryBuilder.MappedClass.Read(folkeConnection, typeof(T), reader);
                    reader.Close();
                    return (T)value;
                }
            }
        }

        public async Task<T> SingleOrDefaultAsync()
        {
            return await SingleOrDefaultAsync(baseQueryBuilder.Connection, baseQueryBuilder.Parameters);
        }


        public List<T> List(IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = folkeConnection.CreateCommand(QueryBuilder.Sql, commandParameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    var ret = new List<T>();
                    while (reader.Read())
                    {
                        var value = QueryBuilder.MappedClass.Read(folkeConnection, typeof(T), reader);
                        ret.Add((T)value);
                    }

                    reader.Close();
                    return ret;
                }
            }
        }

        public IList<T> List()
        {
            return List(QueryBuilder.Connection, QueryBuilder.Parameters);
        }

        public async Task<List<T>> ListAsync(IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = folkeConnection.CreateCommand(baseQueryBuilder.Sql, commandParameters))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var ret = new List<T>();
                    while (reader.Read())
                    {
                        var value = baseQueryBuilder.MappedClass.Read(folkeConnection, typeof(T), reader);
                        ret.Add((T)value);
                    }
                    reader.Close();
                    return ret;
                }
            }
        }

        public async Task<IList<T>> ListAsync()
        {
            return await ListAsync(baseQueryBuilder.Connection, baseQueryBuilder.Parameters);
        }

        public void Execute(IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = folkeConnection.CreateCommand(QueryBuilder.Sql, commandParameters))
            {
                command.ExecuteNonQuery();
            }
        }

        public void Execute()
        {
            Execute(QueryBuilder.Connection, QueryBuilder.Parameters);
        }

        public async Task ExecuteAsync(IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = folkeConnection.CreateCommand(QueryBuilder.Sql, commandParameters))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task ExecuteAsync()
        {
            await ExecuteAsync(QueryBuilder.Connection, QueryBuilder.Parameters);
        }
    }
}