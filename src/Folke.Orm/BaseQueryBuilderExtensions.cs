using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Folke.Orm
{
    public static class BaseQueryBuilderExtensions
    {
        public static object Scalar(this BaseQueryBuilder baseQueryBuilder, FolkeConnection folkeConnection, params object[] commandParameters)
        {
            return folkeConnection.Scalar(baseQueryBuilder.Sql, commandParameters);
        }

        public static object Scalar(this BaseQueryBuilder baseQueryBuilder)
        {
            return baseQueryBuilder.Connection.Scalar(baseQueryBuilder.Sql, baseQueryBuilder.Parameters == null ? null : baseQueryBuilder.Parameters.ToArray());
        }

        public static async Task<object> ScalarAsync(this BaseQueryBuilder baseQueryBuilder, FolkeConnection folkeConnection, params object[] commandParameters)
        {
            return await folkeConnection.ScalarAsync(baseQueryBuilder.Sql, commandParameters);
        }

        public static async Task<object> ScalarAsync(this BaseQueryBuilder baseQueryBuilder)
        {
            return await baseQueryBuilder.Connection.ScalarAsync(baseQueryBuilder.Sql, baseQueryBuilder.Parameters == null ? null : baseQueryBuilder.Parameters.ToArray());
        }

        /// <summary>
        /// Assumes that the result is one scalar value and nothing else, get this
        /// </summary>
        /// <typeparam name="TU">The scalar value type</typeparam>
        /// <param name="baseQueryBuilder">The query builder</param>
        /// <param name="folkeConnection">A folkeConnection</param>
        /// <param name="commandParameters">Optional commandParameters if the query had commandParameters</param>
        /// <returns>A single value</returns>
        public static TU Scalar<TU>(this BaseQueryBuilder baseQueryBuilder, FolkeConnection folkeConnection, params object[] commandParameters)
        {
            return folkeConnection.Scalar<TU>(baseQueryBuilder.Sql, commandParameters);
        }

        public static TU Scalar<TU>(this BaseQueryBuilder baseQueryBuilder)
        {
            return baseQueryBuilder.Connection.Scalar<TU>(baseQueryBuilder.Sql, baseQueryBuilder.Parameters == null ? null : baseQueryBuilder.Parameters.ToArray());
        }

        public static async Task<TU> ScalarAsync<TU>(this BaseQueryBuilder baseQueryBuilder, FolkeConnection folkeConnection, params object[] commandParameters)
        {
            return await folkeConnection.ScalarAsync<TU>(baseQueryBuilder.Sql, commandParameters);
        }

        public static async Task<TU> ScalarAsync<TU>(this BaseQueryBuilder baseQueryBuilder)
        {
            return await baseQueryBuilder.Connection.ScalarAsync<TU>(baseQueryBuilder.Sql, baseQueryBuilder.Parameters == null ? null : baseQueryBuilder.Parameters.ToArray());
        }

        public static List<T> List<T>(this BaseQueryBuilder<T> baseQueryBuilder, IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = folkeConnection.CreateCommand(baseQueryBuilder.Sql, commandParameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    var ret = new List<T>();
                    while (reader.Read())
                    {
                        var value = baseQueryBuilder.MappedClass.Read(folkeConnection, typeof(T), reader);
                        ret.Add((T)value);
                    }
                    reader.Dispose();
                    return ret;
                }
            }
        }
        
        public static IList<T> List<T>(this BaseQueryBuilder<T> baseQueryBuilder)
        {
            return List(baseQueryBuilder, baseQueryBuilder.Connection, baseQueryBuilder.Parameters);
        }

        public static async Task<List<T>> ListAsync<T>(this BaseQueryBuilder<T> baseQueryBuilder, IFolkeConnection folkeConnection, params object[] commandParameters)
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
                    reader.Dispose();
                    return ret;
                }
            }
        }

        public static async Task<IList<T>> ListAsync<T>(this BaseQueryBuilder<T> baseQueryBuilder)
        {
            return await ListAsync(baseQueryBuilder, baseQueryBuilder.Connection, baseQueryBuilder.Parameters);
        }

        public static T Single<T>(this BaseQueryBuilder<T> baseQueryBuilder, IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = folkeConnection.CreateCommand(baseQueryBuilder.Sql, commandParameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new Exception("No result found");
                    var value = baseQueryBuilder.MappedClass.Read(folkeConnection, typeof(T), reader);
                    reader.Dispose();
                    return (T)value;
                }
            }
        }

        /// <summary>
        /// A single result
        /// </summary>
        /// <returns></returns>
        public static T Single<T>(this BaseQueryBuilder<T> baseQueryBuilder)
        {
            return Single(baseQueryBuilder, baseQueryBuilder.Connection, baseQueryBuilder.Parameters);
        }

        public static async Task<T> SingleAsync<T>(this BaseQueryBuilder<T> baseQueryBuilder, IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = folkeConnection.CreateCommand(baseQueryBuilder.Sql, commandParameters))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!reader.Read())
                        throw new Exception("No result found");
                    var value = baseQueryBuilder.MappedClass.Read(folkeConnection, typeof(T), reader);
                    reader.Dispose();
                    return (T)value;
                }
            }
        }

        /// <summary>
        /// A single result
        /// </summary>
        /// <returns></returns>
        public static async Task<T> SingleAsync<T>(this BaseQueryBuilder<T> baseQueryBuilder)
        {
            return await SingleAsync(baseQueryBuilder, baseQueryBuilder.Connection, baseQueryBuilder.Parameters);
        }

        public static T SingleOrDefault<T>(this BaseQueryBuilder<T> baseQueryBuilder, IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = folkeConnection.CreateCommand(baseQueryBuilder.Sql, commandParameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return default(T);
                    var value = baseQueryBuilder.MappedClass.Read(folkeConnection, typeof(T), reader);
                    reader.Dispose();
                    return (T)value;
                }
            }
        }

        public static T SingleOrDefault<T>(this BaseQueryBuilder<T> baseQueryBuilder)
        {
            return SingleOrDefault(baseQueryBuilder, baseQueryBuilder.Connection, baseQueryBuilder.Parameters);
        }

        public static async Task<T> SingleOrDefaultAsync<T>(this BaseQueryBuilder<T> baseQueryBuilder, IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = folkeConnection.CreateCommand(baseQueryBuilder.Sql, commandParameters))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!reader.Read())
                        return default(T);
                    var value = baseQueryBuilder.MappedClass.Read(folkeConnection, typeof(T), reader);
                    reader.Dispose();
                    return (T)value;
                }
            }
        }

        public static async Task<T> SingleOrDefaultAsync<T>(this BaseQueryBuilder<T> baseQueryBuilder)
        {
            return await SingleOrDefaultAsync(baseQueryBuilder, baseQueryBuilder.Connection, baseQueryBuilder.Parameters);
        }

        public static bool TryExecute(this BaseQueryBuilder baseQueryBuilder, IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            try
            {
                Execute(baseQueryBuilder, folkeConnection, commandParameters);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool TryExecute(this BaseQueryBuilder baseQueryBuilder)
        {
            return TryExecute(baseQueryBuilder, baseQueryBuilder.Connection, baseQueryBuilder.Parameters);
        }

        public static void Execute(this BaseQueryBuilder baseQueryBuilder, IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = folkeConnection.CreateCommand(baseQueryBuilder.Sql, commandParameters))
            {
                command.ExecuteNonQuery();
            }
        }

        public static void Execute(this BaseQueryBuilder baseQueryBuilder)
        {
            Execute(baseQueryBuilder, baseQueryBuilder.Connection, baseQueryBuilder.Parameters);
        }

        public static async Task ExecuteAsync(this BaseQueryBuilder baseQueryBuilder, IFolkeConnection folkeConnection, params object[] commandParameters)
        {
            using (var command = folkeConnection.CreateCommand(baseQueryBuilder.Sql, commandParameters))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task ExecuteAsync(this BaseQueryBuilder baseQueryBuilder)
        {
            await ExecuteAsync(baseQueryBuilder, baseQueryBuilder.Connection, baseQueryBuilder.Parameters);
        }
    }
}
