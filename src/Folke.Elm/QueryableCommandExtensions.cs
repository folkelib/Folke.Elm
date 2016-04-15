using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Folke.Elm
{
    public static class QueryableCommandExtensions
    {
        public static object Scalar(this IQueryableCommand baseQueryBuilder)
        {
            using (var command = baseQueryBuilder.CreateCommand())
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read() || reader.IsDBNull(0))
                    {
                        return null;
                    }
                    return reader.GetValue(0);
                }
            }
        }
        
        public static async Task<object> ScalarAsync(this IQueryableCommand baseQueryBuilder)
        {
            using (var command = baseQueryBuilder.Connection.CreateCommand(baseQueryBuilder.Sql, baseQueryBuilder.Parameters))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!reader.Read() || reader.IsDBNull(0))
                    {
                        return null;
                    }
                    return reader.GetValue(0);
                }
            }
        }

        public static IFolkeCommand CreateCommand(this IQueryableCommand queryableCommand)
        {
            return queryableCommand.Connection.CreateCommand(queryableCommand.Sql, queryableCommand.Parameters);
        }

        public static TU Scalar<TU>(this IQueryableCommand baseQueryBuilder)
        {
            using (var command = baseQueryBuilder.CreateCommand())
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read() || reader.IsDBNull(0))
                    {
                        return default(TU);
                    }
                    var ret = baseQueryBuilder.Connection.Driver.ConvertReaderValueToValue(reader, typeof(TU), 0);
                    return (TU)ret;
                }
            }
        }

        public static async Task<TU> ScalarAsync<TU>(this IQueryableCommand baseQueryBuilder)
        {
            using (var command = baseQueryBuilder.CreateCommand())
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!reader.Read() || reader.IsDBNull(0))
                    {
                        return default(TU);
                    }
                    var ret = baseQueryBuilder.Connection.Driver.ConvertReaderValueToValue(reader, typeof(TU), 0);
                    return (TU)ret;
                }
            }
        }
        
        internal static IEnumerable<T> Enumerate<T>(this IQueryableCommand<T> queryable)
        {
            using (var command = queryable.Connection.CreateCommand(queryable.Sql, queryable.Parameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return (T)queryable.MappedClass.Read(queryable.Connection, typeof(T), reader);
                    }
                }
            }
        }
        
        public static IQueryableCommand<T> Build<T>(this IQueryableCommand<T> baseQueryBuilder,
            IFolkeConnection connection, params object[] parameters)
        {
            return new QueryableCommand<T>(connection, baseQueryBuilder.MappedClass, baseQueryBuilder.Sql, parameters);
        }

        public static IList<T> ToList<T>(this IQueryableCommand<T> queryableCommand)
        {
            using (var command = queryableCommand.Connection.CreateCommand(queryableCommand.Sql, queryableCommand.Parameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    var ret = new List<T>();
                    while (reader.Read())
                    {
                        var value = queryableCommand.MappedClass.Read(queryableCommand.Connection, typeof(T), reader);
                        ret.Add((T)value);
                    }
                    return ret;
                }
            }
        }

        public static async Task<IList<T>> ToListAsync<T>(this IQueryableCommand<T> baseQueryBuilder)
        { 
            using (var command = baseQueryBuilder.Connection.CreateCommand(baseQueryBuilder.Sql, baseQueryBuilder.Parameters))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var ret = new List<T>();
                    while (reader.Read())
                    {
                        var value = baseQueryBuilder.MappedClass.Read(baseQueryBuilder.Connection, typeof(T), reader);
                        ret.Add((T)value);
                    }
                    return ret;
                }
            }
        }
        
        /// <summary>
        /// Getes the first result.
        /// Throws an exception if no result is found.
        /// </summary>
        /// <returns>The first result in the query.</returns>
        public static T First<T>(this IQueryableCommand<T> baseQueryBuilder)
        {
            using (var command = baseQueryBuilder.Connection.CreateCommand(baseQueryBuilder.Sql, baseQueryBuilder.Parameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new InvalidOperationException("No result found");
                    var value = baseQueryBuilder.MappedClass.Read(baseQueryBuilder.Connection, typeof(T), reader);
                    return (T)value;
                }
            }
        }

        public static T FirstOrDefault<T>(this IQueryableCommand<T> baseQueryBuilder)
        {
            using (var command = baseQueryBuilder.Connection.CreateCommand(baseQueryBuilder.Sql, baseQueryBuilder.Parameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return default(T);
                    var value = baseQueryBuilder.MappedClass.Read(baseQueryBuilder.Connection, typeof(T), reader);
                    return (T)value;
                }
            }
        }

        /// <summary>
        /// Gets the first result asynchronously.
        /// Throws an exception if there is no result.
        /// </summary>
        /// <returns></returns>
        public static async Task<T> FirstAsync<T>(this IQueryableCommand<T> baseQueryBuilder)
        {
            using (var command = baseQueryBuilder.Connection.CreateCommand(baseQueryBuilder.Sql, baseQueryBuilder.Parameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!await reader.ReadAsync())
                        throw new InvalidOperationException("No result found");
                    var value = baseQueryBuilder.MappedClass.Read(baseQueryBuilder.Connection, typeof(T), reader);
                    return (T)value;
                }
            }
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IQueryableCommand<T> baseQueryBuilder)
        {
            using (var command = baseQueryBuilder.Connection.CreateCommand(baseQueryBuilder.Sql, baseQueryBuilder.Parameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!await reader.ReadAsync())
                        return default(T);
                    var value = baseQueryBuilder.MappedClass.Read(baseQueryBuilder.Connection, typeof(T), reader);
                    return (T)value;
                }
            }
        }

        /// <summary>
        /// A single result
        /// </summary>
        /// <returns></returns>
        public static T Single<T>(this IQueryableCommand<T> baseQueryBuilder)
        {
            using (var command = baseQueryBuilder.Connection.CreateCommand(baseQueryBuilder.Sql, baseQueryBuilder.Parameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new InvalidOperationException("No result found");
                    var value = baseQueryBuilder.MappedClass.Read(baseQueryBuilder.Connection, typeof(T), reader);
                    if (reader.Read())
                        throw new InvalidOperationException("Not single");
                    return (T)value;
                }
            }
        }

        public static async Task<T> SingleAsync<T>(this IQueryableCommand<T> baseQueryBuilder)
        {
            using (var command = baseQueryBuilder.Connection.CreateCommand(baseQueryBuilder.Sql, baseQueryBuilder.Parameters))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!reader.Read())
                        throw new InvalidOperationException("No result found");
                    var value = baseQueryBuilder.MappedClass.Read(baseQueryBuilder.Connection, typeof(T), reader);
                    if (reader.Read())
                        throw new InvalidOperationException("Not single");
                    return (T)value;
                }
            }
        }

        public static T SingleOrDefault<T>(this IQueryableCommand<T> baseQueryBuilder)
        {
            using (var command = baseQueryBuilder.Connection.CreateCommand(baseQueryBuilder.Sql, baseQueryBuilder.Parameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return default(T);
                    var value = baseQueryBuilder.MappedClass.Read(baseQueryBuilder.Connection, typeof(T), reader);
                    if (reader.Read())
                        throw new InvalidOperationException("Not single");
                    return (T)value;
                }
            }
        }
        
        public static async Task<T> SingleOrDefaultAsync<T>(this IQueryableCommand<T> baseQueryBuilder)
        {
            using (var command = baseQueryBuilder.Connection.CreateCommand(baseQueryBuilder.Sql, baseQueryBuilder.Parameters))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!reader.Read())
                        return default(T);
                    var value = baseQueryBuilder.MappedClass.Read(baseQueryBuilder.Connection, typeof(T), reader);
                    if (reader.Read())
                        throw new InvalidOperationException("Not single");
                    return (T)value;
                }
            }
        }
        
        public static bool TryExecute(this IBaseCommand baseQueryBuilder)
        {
            try
            {
                Execute(baseQueryBuilder);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public static void Execute(this IBaseCommand baseQueryBuilder)
        {
            using (var command = baseQueryBuilder.Connection.CreateCommand(baseQueryBuilder.Sql, baseQueryBuilder.Parameters))
            {
                command.ExecuteNonQuery();
            }
        }
        
        public static async Task ExecuteAsync(this IBaseCommand baseQueryBuilder)
        {
            using (var command = baseQueryBuilder.Connection.CreateCommand(baseQueryBuilder.Sql, baseQueryBuilder.Parameters))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
