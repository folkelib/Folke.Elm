namespace Folke.Orm
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq.Expressions;
    using System.Reflection;

    using Folke.Orm.InformationSchema;

    public class FolkeConnection : IFolkeConnection
    {
        private readonly DbConnection connection;
        private FolkeTransaction transaction;

        public FolkeConnection(IDatabaseDriver databaseDriver)
        {
            Cache = new Dictionary<string, IDictionary<int, object>>();
            Driver = databaseDriver;
            Database = databaseDriver.Settings.Database;
            connection = databaseDriver.CreateConnection();
        }

        public IDictionary<string, IDictionary<int, object>> Cache { get; private set; }

        public string Database { get; set; }

        public IDatabaseDriver Driver { get; set; }
        
        public FolkeCommand OpenCommand()
        {
            if (transaction == null)
                connection.Open();
            return new FolkeCommand(this, connection.CreateCommand());
        }

        public FolkeTransaction BeginTransaction()
        {
            connection.Open();
            transaction = new FolkeTransaction(this, connection.BeginTransaction());
            return transaction;
        }

        public QueryBuilder<T> Query<T>() where T : class, new()
        {
            return new QueryBuilder<T>(this);
        }

        public QueryBuilder<T> QueryOver<T>() where T : class, new()
        {
            var ret = new QueryBuilder<T>(this);
            ret.SelectAll().From();
            return ret;
        }

        public QueryBuilder<T> QueryOver<T>(params Expression<Func<T, object>>[] fetches) where T : class, new()
        {
            var query = new QueryBuilder<T>(this);
            query.SelectAll();
            foreach (var fetch in fetches)
                query.AndAll(fetch);
            query.From();
            foreach (var fetch in fetches)
                query.LeftJoinOn(fetch);
            return query;
        }
        
        public void Delete<T>(T value) where T : class, IFolkeTable, new()
        {
            new QueryBuilder<T>(this).Delete().From().Where(x => x.Id == value.Id).Execute();
        }

        public void Update<T>(T value) where T : class, IFolkeTable, new()
        {
            if (value.Id == 0)
                throw new Exception("Id must not be 0");
            new QueryBuilder<T>(this).Update().Set(value).Where(x => x.Id == value.Id).Execute();
        }

        public T Refresh<T>(T value) where T : class, IFolkeTable, new()
        {
            if (value.Id == 0)
                throw new Exception("Id must not be 0");
            return new QueryBuilder<T>(this).SelectAll().From().Where(x => x.Id == value.Id).Single();
        }

        public T Load<T>(int id) where T : class, IFolkeTable, new()
        {
            return new QueryBuilder<T>(this).SelectAll().From().Where(x => x.Id == id).Single();
        }

        public T Load<T>(int id, params Expression<Func<T, object>>[] fetches) where T : class, IFolkeTable, new()
        {
            return CreateLoadOrGetQuery(fetches).Where(x => x.Id == id).Single();
        }

        public T Get<T>(int id) where T : class, IFolkeTable, new()
        {
            return new QueryBuilder<T>(this).SelectAll().From().Where(x => x.Id == id).SingleOrDefault();
        }

        public T Get<T>(int id, params Expression<Func<T, object>>[] fetches) where T : class, IFolkeTable, new()
        {
            return CreateLoadOrGetQuery(fetches).Where(x => x.Id == id).SingleOrDefault();
        }

        public void Save<T>(T value) where T : class, IFolkeTable, new()
        {
            if (value.Id != 0)
                throw new Exception("Id must be 0");
            new QueryBuilder<T>(this).InsertInto().Values(value).Execute();
            value.Id = new QueryBuilder<T>(this).Append("SELECT last_insert_id()").Scalar<int>();
            if (!Cache.ContainsKey(typeof(T).Name))
                Cache[typeof(T).Name] = new Dictionary<int, object>();
            Cache[typeof(T).Name][value.Id] = value;
        }

        public void CreateTable<T>(bool drop = false) where T : class, new()
        {
            if (drop)
            {
                new SchemaQueryBuilder<T>(this).DropTable().TryExecute();
            }

            new SchemaQueryBuilder<T>(this).CreateTable().Execute();
        }
        
        public void DropTable<T>() where T : class, new()
        {
            new SchemaQueryBuilder<T>(this).DropTable().Execute();
        }
        
        public void CreateOrUpdateTable<T>() where T : class, new()
        {
            new SchemaUpdater(this).CreateOrUpdate<T>();
        }

        public void UpdateSchema(Assembly assembly)
        {
            var schemaUpdater = new SchemaUpdater(this);
            schemaUpdater.CreateOrUpdateAll(assembly);
        }

        /// <summary>
        /// Remove an old element, replacing each reference to this element
        /// with references to a new element
        /// </summary>
        /// <typeparam name="T">The element type</typeparam>
        /// <param name="oldElement">The element to delete</param>
        /// <param name="newElement">The element that replaces the previous element</param>
        public void Merge<T>(T oldElement, T newElement) where T : class, IFolkeTable, new()
        {
            var type = typeof(T);
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            string tableName;
            string tableSchema;
            if (tableAttribute != null)
            {
                tableName = tableAttribute.Name;
                tableSchema = tableAttribute.Schema ?? connection.Database;
            }
            else
            {
                tableName = type.Name;
                tableSchema = connection.Database;
            }

            var columns =
                this.QueryOver<KeyColumnUsage>()
                    .Where(c => c.ReferencedTableName == tableName && c.ReferencedTableSchema == tableSchema)
                    .List();

            foreach (var column in columns)
            {
                using (var command = this.OpenCommand())
                {
                    command.CommandText = string.Format(
                        "UPDATE `{0}`.`{1}` SET `{2}` = {3} WHERE `{2}` = {4}",
                        column.TableSchema,
                        column.TableName,
                        column.ColumnName,
                        newElement.Id,
                        oldElement.Id);
                    command.ExecuteNonQuery();
                }
            }

            Delete(oldElement);
        }

        internal void CloseCommand()
        {
            if (transaction == null)
                connection.Close();
        }

        internal void EndTransaction()
        {
            // In case EndTransaction has already been called
            if (transaction != null) 
            {
                transaction = null;
                connection.Close();
            }
        }

        internal void CreateTable(Type t, bool drop = false, IList<string> existingTables = null)
        {
            if (drop)
            {
                new SchemaQueryBuilder<FolkeTuple>(this).DropTable(t).TryExecute();
            }

            new SchemaQueryBuilder<FolkeTuple>(this).CreateTable(t, existingTables).Execute();
        }

        private BaseQueryBuilder<T, FolkeTuple> CreateLoadOrGetQuery<T>(Expression<Func<T, object>>[] fetches) where T : class, IFolkeTable, new()
        {
            var query = new QueryBuilder<T>(this).SelectAll();
            foreach (var fetch in fetches)
            {
                query.AndAll(fetch);
            }

            query.From();
            foreach (var fetch in fetches)
            {
                query.LeftJoinOn(fetch);
            }

            return query;
        }
    } 
}