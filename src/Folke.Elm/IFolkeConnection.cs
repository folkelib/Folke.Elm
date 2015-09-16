using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Folke.Elm.Fluent;
using Folke.Elm.Mapping;

namespace Folke.Elm
{
    public interface IFolkeConnection : IDisposable
    {
        FolkeTransaction BeginTransaction();

        [Obsolete("Use SelectAllFrom")]
        FluentFromBuilder<T, FolkeTuple> QueryOver<T>() where T : class, new();
        [Obsolete("Use SelectAllFrom")]
        FluentFromBuilder<T, FolkeTuple> QueryOver<T>(params Expression<Func<T, object>>[] fetches) where T : class, new();

        [Obsolete("Use Select")]
        FluentSelectBuilder<T, FolkeTuple> Query<T>() where T : class, new();
        void Update<T>(T value) where T : class, new();
        FluentUpdateBuilder<T, FolkeTuple> Update<T>();

        /// <summary>
        /// Loads an object by its id. Throws an error if the object can not be found.
        /// </summary>
        /// <typeparam name="T">The object class</typeparam>
        /// <param name="id">The id</param>
        /// <returns>The object</returns>
        T Load<T>(int id) where T : class, IFolkeTable, new();

        T Load<T>(int id, params Expression<Func<T, object>>[] fetches) where T : class, IFolkeTable, new();

        /// <summary>
        /// Gets an object by its id. Returns null if the object can not be found.
        /// </summary>
        /// <typeparam name="T">The object class</typeparam>
        /// <param name="id">The id</param>
        /// <returns>The object or null if it can not be found</returns>
        T Get<T>(int id) where T : class, IFolkeTable, new();

        T Get<T>(int id, params Expression<Func<T, object>>[] fetches) where T : class, IFolkeTable, new();
        void Save<T>(T value) where T : class, new();
        void Delete<T>(T value) where T : class, new();

        FluentDeleteBuilder<T, FolkeTuple> Delete<T>() where T : class, new();
        T Refresh<T>(T value) where T : class, new();
        void Merge<T>(T oldElement, T newElement) where T : class, IFolkeTable, new();
        void CreateTable<T>(bool drop = false) where T : class, new();
        void DropTable<T>() where T : class, new();
        void CreateOrUpdateTable<T>() where T : class, new();
        void UpdateSchema(Assembly assembly);

        FolkeCommand OpenCommand();
        IDictionary<string, IDictionary<object, object>> Cache { get; } //TODO
        IDatabaseDriver Driver { get; }
        IMapper Mapper { get; }
        T Load<T>(object id) where T : class, new();
        T Load<T>(object id, params Expression<Func<T, object>>[] fetches) where T : class, new();
        T Get<T>(object id) where T : class, new();
        T Get<T>(object id, params Expression<Func<T, object>>[] fetches) where T : class, new();
        FolkeCommand CreateCommand(string commandText, object[] commandParameters);
        Task<T> LoadAsync<T>(object id, params Expression<Func<T, object>>[] fetches) where T : class, new();
        Task<T> GetAsync<T>(object id, params Expression<Func<T, object>>[] fetches) where T : class, new();
        Task SaveAsync<T>(T value) where T : class, new();
        Task DeleteAsync<T>(T value) where T : class, new();
        Task UpdateAsync<T>(T value) where T : class, new();

        /// <summary>
        /// Create a select expression
        /// </summary>
        /// <typeparam name="T">The base table</typeparam>
        /// <returns>The query</returns>
        FluentSelectBuilder<T, FolkeTuple> Select<T>() where T : class, new();

        /// <summary>
        /// Create a select expression with a parameter table
        /// </summary>
        /// <typeparam name="T">The table to select from</typeparam>
        /// <typeparam name="TParameters">The class that holds the parameter for the query</typeparam>
        /// <returns>The query</returns>
        FluentSelectBuilder<T, TParameters> Select<T, TParameters>() where T : class, new();

        /// <summary>
        /// Create a query that selects all the field from the T type
        /// </summary>
        /// <typeparam name="T">The table to select on</typeparam>
        /// <returns>The query</returns>
        FluentFromBuilder<T, FolkeTuple> SelectAllFrom<T>() where T : class, new();

        /// <summary>
        /// Create a query that selects all the fields from the T type and all the properties in parameter
        /// </summary>
        /// <typeparam name="T">The type to select on</typeparam>
        /// <param name="fetches">The other tables to select (using a left join)</param>
        /// <returns></returns>
        FluentFromBuilder<T, FolkeTuple> SelectAllFrom<T>(params Expression<Func<T, object>>[] fetches)
            where T : class, new();

        /// <summary>
        /// Create a query that insert values in a table
        /// </summary>
        /// <typeparam name="T">The table where the values are inserted</typeparam>
        /// <returns>The query</returns>
        FluentInsertIntoBuilder<T, FolkeTuple> InsertInto<T>() where T : class, new();
    }
}
