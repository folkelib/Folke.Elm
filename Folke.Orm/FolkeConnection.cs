using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Folke.Orm.InformationSchema;

namespace Folke.Orm
{
    using Fluent;

    public class FolkeConnection : IFolkeConnection
    {
        private readonly DbConnection connection;
        private DbTransaction transaction;
        private int stackedTransactions;
        private bool askRollback;

        public FolkeConnection(IDatabaseDriver databaseDriver, string connectionString = null)
        {
            Cache = new Dictionary<string, IDictionary<object, object>>();
            Driver = databaseDriver;
            connection = databaseDriver.CreateConnection(connectionString);
            Database = connection.Database;
            Mapper = new Mapper(Database);
        }

        public IDictionary<string, IDictionary<object, object>> Cache { get; private set; }

        public string Database { get; set; }

        public IDatabaseDriver Driver { get; set; }

        internal Mapper Mapper { get; private set; }
        
        public FolkeCommand OpenCommand()
        {
            if (transaction == null)
                connection.Open();
            return new FolkeCommand(this, connection.CreateCommand());
        }

        public FolkeTransaction BeginTransaction()
        {
            if (transaction == null)
            {
                connection.Open();
                transaction = connection.BeginTransaction();
                askRollback = false;
            }

            stackedTransactions++;
            return new FolkeTransaction(this);
        }

        public FluentSelectBuilder<T, FolkeTuple> Select<T>() where T : class, new()
        {
            return new FluentSelectBuilder<T, FolkeTuple>(new BaseQueryBuilder<T>(this));
        }

        public FluentSelectBuilder<T, FolkeTuple> Query<T>() where T : class, new()
        {
            return new FluentSelectBuilder<T, FolkeTuple>(new BaseQueryBuilder<T>(this));
        }

        public FluentFromBuilder<T, FolkeTuple> QueryOver<T>() where T : class, new()
        {
            return Select<T>().All().From();
        }

        public FluentFromBuilder<T, FolkeTuple> QueryOver<T>(params Expression<Func<T, object>>[] fetches) where T : class, new()
        {
            var query = new FluentSelectBuilder<T, FolkeTuple>(new BaseQueryBuilder(this, typeof(T)));
            query.All();
            foreach (var fetch in fetches)
                query.All(fetch);
            var fromQuery = query.From();
            foreach (var fetch in fetches)
                fromQuery.LeftJoinOnId(fetch);
            return fromQuery;
        }
        
        public void Delete<T>(T value) where T : class, new()
        {
            var keyProperty = TableHelpers.GetKey(typeof(T));
            Delete<T>().From().Where(x => x.Property(keyProperty) == keyProperty.GetValue(value)).Execute();
        }

        public FluentDeleteBuilder<T, FolkeTuple> Delete<T>() where T : class, new()
        {
            return new FluentDeleteBuilder<T, FolkeTuple>(new BaseQueryBuilder(this, typeof(T)));
        }

        public async Task DeleteAsync<T>(T value) where T : class, new()
        {
            var keyProperty = TableHelpers.GetKey(typeof(T));
            await Delete<T>().From().Where(x => x.Property(keyProperty) == keyProperty.GetValue(value)).ExecuteAsync();
        }

        public void Update<T>(T value) where T : class, new()
        {
            var keyProperty = TableHelpers.GetKey(typeof(T));
            Update<T>().SetAll(value).Where(x => x.Property(keyProperty) == keyProperty.GetValue(value)).Execute();
        }

        public FluentUpdateBuilder<T, FolkeTuple> Update<T>()
        {
            return new FluentUpdateBuilder<T, FolkeTuple>(new BaseQueryBuilder(this, typeof(T)));
        }

        public async Task UpdateAsync<T>(T value) where T : class, new()
        {
            var keyProperty = TableHelpers.GetKey(typeof(T));
            await Update<T>().SetAll(value).Where(x => x.Property(keyProperty) == keyProperty.GetValue(value)).ExecuteAsync();
        }

        public T Refresh<T>(T value) where T : class, new()
        {
            var keyProperty = TableHelpers.GetKey(typeof(T));
            return Select<T>().All().From().Where(x => x.Property(keyProperty) == keyProperty.GetValue(value)).Single();
        }

        public T Load<T>(object id) where T : class, new()
        {
            var keyProperty = TableHelpers.GetKey(typeof(T));
            return Select<T>().All().From().Where(x => x.Property(keyProperty).Equals(id)).Single();
        }

        public T Load<T>(object id, params Expression<Func<T, object>>[] fetches) where T : class, new()
        {
            var keyProperty = TableHelpers.GetKey(typeof(T));
            return CreateLoadOrGetQuery(fetches).Where(x => x.Property(keyProperty) == id).Single();
        }

        public async Task<T> LoadAsync<T>(object id, params Expression<Func<T, object>>[] fetches) where T : class, new()
        {
            var keyProperty = TableHelpers.GetKey(typeof(T));
            return await CreateLoadOrGetQuery(fetches).Where(x => x.Property(keyProperty) == id).SingleAsync();
        }

        public T Load<T>(int id) where T : class, IFolkeTable, new()
        {
            return Select<T>().All().From().Where(x => x.Id == id).Single();
        }

        public T Load<T>(int id, params Expression<Func<T, object>>[] fetches) where T : class, IFolkeTable, new()
        {
            return CreateLoadOrGetQuery(fetches).Where(x => x.Id == id).Single();
        }

        public T Get<T>(int id) where T : class, IFolkeTable, new()
        {
            return Select<T>().All().From().Where(x => x.Id == id).SingleOrDefault();
        }

        public T Get<T>(int id, params Expression<Func<T, object>>[] fetches) where T : class, IFolkeTable, new()
        {
            return CreateLoadOrGetQuery(fetches).Where(x => x.Id == id).SingleOrDefault();
        }

        public T Get<T>(object id) where T : class, new()
        {
            var keyProperty = TableHelpers.GetKey(typeof(T));
            return Select<T>().All().From().Where(x => x.Property(keyProperty) == id).SingleOrDefault();
        }

        public T Get<T>(object id, params Expression<Func<T, object>>[] fetches) where T : class, new()
        {
            var keyProperty = TableHelpers.GetKey(typeof(T));
            return CreateLoadOrGetQuery(fetches).Where(x => x.Property(keyProperty) == id).SingleOrDefault();
        }

        public async Task<T> GetAsync<T>(object id, params Expression<Func<T, object>>[] fetches) where T : class, new()
        {
            var keyProperty = TableHelpers.GetKey(typeof(T));
            return await CreateLoadOrGetQuery(fetches).Where(x => x.Property(keyProperty) == id).SingleOrDefaultAsync();
        }

        public FluentInsertIntoBuilder<T, FolkeTuple> InsertInto<T>() where T : class, new()
        {
            return new FluentInsertIntoBuilder<T, FolkeTuple>(new BaseQueryBuilder(this));
        }

        public void Save<T>(T value) where T : class, new()
        {
            var keyProperty = TableHelpers.GetKey(typeof(T));
            bool automatic = TableHelpers.IsAutomatic(keyProperty);
            CreateSaveQuery(value, automatic, keyProperty).Execute();
            UpdateSavedValue(value, automatic, keyProperty);
        }

        public async Task SaveAsync<T>(T value) where T : class, new()
        {
            var keyProperty = TableHelpers.GetKey(typeof(T));
            bool automatic = TableHelpers.IsAutomatic(keyProperty);
            await CreateSaveQuery(value, automatic, keyProperty).ExecuteAsync();
            UpdateSavedValue(value, automatic, keyProperty);
        }

        private void UpdateSavedValue<T>(T value, bool automatic, PropertyInfo keyProperty) where T : class, new()
        {
            if (automatic)
            {
                var key = Select<FolkeTuple<int>>().SelectAs(x => SqlFunctions.LastInsertedId(), x => x.Item0).Scalar();
                keyProperty.SetValue(value, Convert.ChangeType(key, keyProperty.PropertyType));
            }

            if (!Cache.ContainsKey(typeof(T).Name))
            {
                Cache[typeof(T).Name] = new Dictionary<object, object>();
            }

            Cache[typeof(T).Name][keyProperty.GetValue(value)] = value;
        }

        private FluentInsertIntoBuilder<T, FolkeTuple> CreateSaveQuery<T>(T value, bool automatic, PropertyInfo keyProperty)
            where T : class, new()
        {
            if (automatic)
            {
                var defaultValue = Activator.CreateInstance(keyProperty.PropertyType);
                if (!keyProperty.GetValue(value).Equals(defaultValue))
                    throw new Exception("Id must be 0");
            }

            return InsertInto<T>().Values(value);
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
            var typeMap = Mapper.GetTypeMapping(type);

            var columns =
                QueryOver<KeyColumnUsage>()
                    .Where(c => c.ReferencedTableName == typeMap.TableName && c.ReferencedTableSchema == typeMap.TableSchema)
                    .List();

            foreach (var column in columns)
            {
                using (var command = OpenCommand())
                {
                    command.CommandText = String.Format(
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

        internal void RollbackTransaction()
        {
            stackedTransactions--;
            askRollback = true;

            if (stackedTransactions == 0) 
            {
                transaction.Rollback();
                transaction = null;
                connection.Close();
            }
        }

        internal void CommitTransaction()
        {
            stackedTransactions--;

            if (stackedTransactions == 0)
            {
                if (askRollback)
                    transaction.Rollback();
                else
                    transaction.Commit();
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

        private FluentFromBuilder<T, FolkeTuple> CreateLoadOrGetQuery<T>(Expression<Func<T, object>>[] fetches) where T : class, new()
        {
            var query = Select<T>().All();
            foreach (var fetch in fetches)
            {
                query.All(fetch);
            }

            var from = query.From();
            foreach (var fetch in fetches)
            {
                from.LeftJoinOnId(fetch);
            }

            return from;
        }

        public void Dispose()
        {
            if (transaction != null)
                transaction.Dispose();
            if (connection != null)
                connection.Dispose();
        }

        public FolkeCommand CreateCommand(string commandText, object[] commandParameters)
        {
            var command = OpenCommand();
            if (commandParameters != null)
            {
                for (var i = 0; i < commandParameters.Length; i++)
                {
                    var parameterName = "Item" + i.ToString(CultureInfo.InvariantCulture);
                    var parameter = commandParameters[i];
                    var commandParameter = command.CreateParameter();
                    commandParameter.ParameterName = parameterName;
                    if (parameter == null)
                        commandParameter.Value = null;
                    else
                    {
                        var parameterType = parameter.GetType();
                        if (parameterType.IsEnum)
                            commandParameter.Value = parameterType.GetEnumName(parameter);
                        else
                        {
                            var table = parameter as IFolkeTable;
                            commandParameter.Value = table != null ? table.Id : parameter;
                        }
                    }
                    command.Parameters.Add(commandParameter);
                }
            }
            command.CommandText = commandText;
            return command;
        }

        public TU Scalar<TU>(string query, params object[] commandParameters)
        {
            using (var command = CreateCommand(query, commandParameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read() || reader.IsDBNull(0))
                    {
                        return default(TU);
                    }
                    var ret = reader.GetTypedValue<TU>(0);
                    reader.Close();
                    return ret;
                }
            }
        }

        public async Task<TU> ScalarAsync<TU>(string query, params object[] commandParameters)
        {
            using (var command = CreateCommand(query, commandParameters))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!reader.Read() || reader.IsDBNull(0))
                    {
                        return default(TU);
                    }
                    var ret = reader.GetTypedValue<TU>(0);
                    reader.Close();
                    return ret;
                }
            }
        }

        public object Scalar(string query, params object[] commandParameters)
        {
            using (var command = CreateCommand(query, commandParameters))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read() || reader.IsDBNull(0))
                    {
                        return null;
                    }
                    var ret = reader.GetValue(0);
                    reader.Close();
                    return ret;
                }
            }
        }

        public async Task<object> ScalarAsync(string query, params object[] commandParameters)
        {
            using (var command = CreateCommand(query, commandParameters))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!reader.Read() || reader.IsDBNull(0))
                    {
                        return null;
                    }
                    var ret = reader.GetValue(0);
                    reader.Close();
                    return ret;
                }
            }
        }
    } 
}