using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Orm
{
    public interface IFolkeConnection
    {
        FolkeTransaction BeginTransaction();

        QueryBuilder<T> QueryOver<T>() where T : class, new();
        QueryBuilder<T> QueryOver<T>(params Expression<Func<T, object>>[] fetches) where T : class, new();
        QueryBuilder<T> Query<T>() where T : class, new();
        void Update<T>(T value) where T : class, new();

        /// <summary>
        /// Loads an object by its id. Throws an error if the object can not be found.
        /// </summary>
        /// <typeparam name="T">The object class</typeparam>
        /// <typeparam name="TU">The key type</typeparam>
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
        T Refresh<T>(T value) where T : class, new();
        void Merge<T>(T oldElement, T newElement) where T : class, IFolkeTable, new();
        void CreateTable<T>(bool drop = false) where T : class, new();
        void DropTable<T>() where T : class, new();
        void CreateOrUpdateTable<T>() where T : class, new();
        void UpdateSchema(Assembly assembly);

        FolkeCommand OpenCommand();
        IDictionary<string, IDictionary<object, object>> Cache { get; } //TODO
        IDatabaseDriver Driver { get; }
    }
}
