using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Orm
{
    using Folke.Orm.Fluent;

    public interface IFolkeConnection : IDisposable
    {
        FolkeTransaction BeginTransaction();

        FluentFromBuilder<T, FolkeTuple> QueryOver<T>() where T : class, new();
        FluentFromBuilder<T, FolkeTuple> QueryOver<T>(params Expression<Func<T, object>>[] fetches) where T : class, new();

        FluentSelectBuilder<T, FolkeTuple> Query<T>() where T : class, new();
        void Update<T>(T value) where T : class, new();

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
    }
}
