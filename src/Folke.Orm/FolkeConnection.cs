using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Folke.Orm.InformationSchema;
using Folke.Orm.Mapping;

namespace Folke.Orm
{
    using Fluent;

    public class FolkeConnection : IFolkeConnection
    {
        private readonly DbConnection connection;
        private DbTransaction transaction;
        private int stackedTransactions;
        private bool askRollback;

        public FolkeConnection(IDatabaseDriver databaseDriver, IMapper mapper, string connectionString = null)
        {
            Cache = new Dictionary<string, IDictionary<object, object>>();
            Driver = databaseDriver;
            connection = databaseDriver.CreateConnection(connectionString);
            Database = connection.Database;
            Mapper = mapper;
        }

        public IDictionary<string, IDictionary<object, object>> Cache { get; private set; }

        public string Database { get; set; }

        public IDatabaseDriver Driver { get; set; }

        public IMapper Mapper { get; private set; }
        
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
            return new FluentSelectBuilder<T, FolkeTuple>(new BaseQueryBuilder(this, typeof(T)));
        }

        public FluentSelectBuilder<T, TParameters> Select<T, TParameters>() where T : class, new()
        {
            return new FluentSelectBuilder<T, TParameters>(new BaseQueryBuilder(this, typeof(T), typeof(TParameters)));
        }

        [Obsolete("Use Select")]
        public FluentSelectBuilder<T, FolkeTuple> Query<T>() where T : class, new()
        {
            return new FluentSelectBuilder<T, FolkeTuple>(new BaseQueryBuilder(this, typeof(T)));
        }

        public FluentFromBuilder<T, FolkeTuple> SelectAllFrom<T>() where T : class, new()
        {
            return Select<T>().All().From();
        }

        [Obsolete("Use SelectAll")]
        public FluentFromBuilder<T, FolkeTuple> QueryOver<T>() where T : class, new()
        {
            return SelectAllFrom<T>();
        }

        /// <summary>
        /// Select all the fields from the T type and the properties in parameter
        /// </summary>
        /// <typeparam name="T">The type to select on</typeparam>
        /// <param name="fetches">The other tables to select (using a left join)</param>
        /// <returns></returns>
        public FluentFromBuilder<T, FolkeTuple> SelectAllFrom<T>(params Expression<Func<T, object>>[] fetches)
            where T : class, new()
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

        [Obsolete("Use SelectAll")]
        public FluentFromBuilder<T, FolkeTuple> QueryOver<T>(params Expression<Func<T, object>>[] fetches) where T : class, new()
        {
            return SelectAllFrom<T>(fetches);
        }
        
        public void Delete<T>(T value) where T : class, new()
        {
            var keyProperty = Mapper.GetTypeMapping(typeof(T)).Key.PropertyInfo;
            Delete<T>().From().Where(x => x.Key() == keyProperty.GetValue(value)).Execute();
        }

        public FluentDeleteBuilder<T, FolkeTuple> Delete<T>() where T : class, new()
        {
            return new FluentDeleteBuilder<T, FolkeTuple>(new BaseQueryBuilder(this, typeof(T)));
        }

        public async Task DeleteAsync<T>(T value) where T : class, new()
        {
            var keyProperty = Mapper.GetTypeMapping(typeof(T)).Key.PropertyInfo;
            await Delete<T>().From().Where(x => x.Key() == keyProperty.GetValue(value)).ExecuteAsync();
        }

        public void Update<T>(T value) where T : class, new()
        {
            var keyProperty = Mapper.GetTypeMapping(typeof(T)).Key.PropertyInfo;
            Update<T>().SetAll(value).Where(x => x.Key() == keyProperty.GetValue(value)).Execute();
        }

        public FluentUpdateBuilder<T, FolkeTuple> Update<T>()
        {
            return new FluentUpdateBuilder<T, FolkeTuple>(new BaseQueryBuilder(this, typeof(T)));
        }

        public async Task UpdateAsync<T>(T value) where T : class, new()
        {
            var keyProperty = Mapper.GetTypeMapping(typeof(T)).Key.PropertyInfo;
            await Update<T>().SetAll(value).Where(x => x.Key() == keyProperty.GetValue(value)).ExecuteAsync();
        }

        public T Refresh<T>(T value) where T : class, new()
        {
            var keyProperty = Mapper.GetTypeMapping(typeof(T)).Key.PropertyInfo;
            return Select<T>().All().From().Where(x => x.Key() == keyProperty.GetValue(value)).Single();
        }

        public T Load<T>(object id) where T : class, new()
        {
            return Select<T>().All().From().Where(x => x.Key().Equals(id)).Single();
        }

        public T Load<T>(object id, params Expression<Func<T, object>>[] fetches) where T : class, new()
        {
            return CreateLoadOrGetQuery(fetches).Where(x => x.Key() == id).Single();
        }

        public async Task<T> LoadAsync<T>(object id, params Expression<Func<T, object>>[] fetches) where T : class, new()
        {
            return await CreateLoadOrGetQuery(fetches).Where(x => x.Key() == id).SingleAsync();
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
            return Select<T>().All().From().Where(x => x.Key() == id).SingleOrDefault();
        }

        public T Get<T>(object id, params Expression<Func<T, object>>[] fetches) where T : class, new()
        {
            return CreateLoadOrGetQuery(fetches).Where(x => x.Key() == id).SingleOrDefault();
        }

        public async Task<T> GetAsync<T>(object id, params Expression<Func<T, object>>[] fetches) where T : class, new()
        {
            return await CreateLoadOrGetQuery(fetches).Where(x => x.Key() == id).SingleOrDefaultAsync();
        }

        public FluentInsertIntoBuilder<T, FolkeTuple> InsertInto<T>() where T : class, new()
        {
            return new FluentInsertIntoBuilder<T, FolkeTuple>(new BaseQueryBuilder(this, typeof(T)));
        }

        public void Save<T>(T value) where T : class, new()
        {
            var keyProperty = Mapper.GetTypeMapping(typeof(T)).Key;
            bool automatic = keyProperty.IsAutomatic;
            CreateSaveQuery(value, automatic, keyProperty.PropertyInfo).Execute();
            UpdateSavedValue(value, automatic, keyProperty.PropertyInfo);
        }

        public async Task SaveAsync<T>(T value) where T : class, new()
        {
            var keyProperty = Mapper.GetTypeMapping(typeof(T)).Key;
            bool automatic = keyProperty.IsAutomatic;
            await CreateSaveQuery(value, automatic, keyProperty.PropertyInfo).ExecuteAsync();
            UpdateSavedValue(value, automatic, keyProperty.PropertyInfo);
        }

        private void UpdateSavedValue<T>(T value, bool automatic, PropertyInfo keyProperty) where T : class, new()
        {
            if (automatic)
            {
                var key = Select<FolkeTuple<int>>().Value(x => SqlFunctions.LastInsertedId(), x => x.Item0).Scalar();
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
                SelectAllFrom<KeyColumnUsage>()
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
            transaction?.Dispose();
            connection?.Dispose();
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
                        if (parameterType.GetTypeInfo().IsEnum)
                        {
                            commandParameter.Value = Enum.GetName(parameterType, parameter);
                        }
                        else if (parameterType.GetTypeInfo().IsValueType || parameterType == typeof(string))
                        {
                            commandParameter.Value = parameter;
                        }
                        else
                        {
                            commandParameter.Value =
                                Mapper.GetTypeMapping(parameterType).Key.PropertyInfo.GetValue(parameter);
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
                    // reader.Close();
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
                    // reader.Close();
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
                    // reader.Close();
                    return ret;
                }
            }
        }
    } 
}