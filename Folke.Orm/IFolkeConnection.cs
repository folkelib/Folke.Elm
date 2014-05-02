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
        void Update<T>(T value) where T : class, IFolkeTable, new();
        T Load<T>(int id) where T : class, IFolkeTable, new();
        T Load<T>(int id, params Expression<Func<T, object>>[] fetches) where T : class, IFolkeTable, new();
        T Get<T>(int id) where T : class, IFolkeTable, new();
        void Save<T>(T value) where T : class, IFolkeTable, new();
        void Delete<T>(T value) where T : class, IFolkeTable, new();
        T Refresh<T>(T value) where T : class, IFolkeTable, new();

        void CreateTable<T>(bool drop = false) where T : class, new();
        void DropTable<T>() where T : class, new();
        void CreateOrUpdateTable<T>() where T : class, new();
        void UpdateSchema(Assembly assembly);

        FolkeCommand OpenCommand();
        IDictionary<string, IDictionary<int, object>> Cache { get; } //TODO
        IDatabaseDriver Driver { get; }
    }
}
