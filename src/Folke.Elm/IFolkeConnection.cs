using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Folke.Elm.Fluent;
using Folke.Elm.Mapping;

namespace Folke.Elm
{
    /// <summary>
    /// A connection to the database.
    /// </summary>
    public interface IFolkeConnection : IDisposable
    {
        /// <summary>
        /// Gets the cache of data.
        /// TODO: create a class.
        /// </summary>
        IDictionary<string, IDictionary<object, object>> Cache { get; }

        /// <summary>
        /// Gets the database driver
        /// </summary>
        IDatabaseDriver Driver { get; }

        /// <summary>
        /// Gets the mapper
        /// </summary>
        IMapper Mapper { get; }

        /// <summary>
        /// Begins a transaction if a transaction is not already open. The transaction will be 
        /// roll backed on dispose if not committed.
        /// </summary>
        /// <returns>The new transaction</returns>
        FolkeTransaction BeginTransaction();

        /// <summary>
        /// Updates a value in the database
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="value">The object to update</param>
        void Update<T>(T value) where T : class, new();
        
        /// <summary>Updates asynchronously a value in the database</summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="value">The object to update</param>
        /// <returns>The <see cref="Task"/>.</returns>
        Task UpdateAsync<T>(T value) where T : class, new();

        /// <summary>
        /// Creates an update expression
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <returns>An update expression</returns>
        IUpdateResult<T, FolkeTuple> Update<T>();

        /// <summary>
        /// Loads an object by its primary key. Throws an error if the object can not be found.
        /// </summary>
        /// <typeparam name="T">The object class</typeparam>
        /// <param name="id">The primary key value</param>
        /// <returns>The object</returns>
        T Load<T>(object id) where T : class, new();

        /// <summary>
        /// Loads an object by its primary key. Includes objects referenced by its properties.
        /// Throws an error if the object can not be found.
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="id">The primary key value</param>
        /// <param name="fetches">The properties to fill with objects from the database.</param>
        /// <returns>The found object</returns>
        T Load<T>(object id, params Expression<Func<T, object>>[] fetches) where T : class, new();

        /// <summary>
        /// Loads asynchronously an object by its primary key. Includes objects referenced by its properties.
        /// Throws an error if the object can not be found.
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="id">The primary key value</param>
        /// <param name="fetches">The properties to fill with objects from the database.</param>
        /// <returns>The found object</returns>
        Task<T> LoadAsync<T>(object id, params Expression<Func<T, object>>[] fetches) where T : class, new();

        /// <summary>
        /// Gets an object by its primary key value. Returns null if the object can not be found.
        /// </summary>
        /// <typeparam name="T">The object class</typeparam>
        /// <param name="id">The id</param>
        /// <returns>The object or null if it can not be found</returns>
        T Get<T>(object id) where T : class, new();

        /// <summary>
        /// Gets asynchronously an object by its primary key value. Returns null if the object can not be found.
        /// </summary>
        /// <typeparam name="T">The object class</typeparam>
        /// <param name="id">The primary key</param>
        /// <param name="fetches">The properties to fill with objects from the database.</param>
        /// <returns>The object or null if it can not be found</returns>
        Task<T> GetAsync<T>(object id, params Expression<Func<T, object>>[] fetches) where T : class, new();
        
        /// <summary>
        /// Saves an object in the database
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="value">The value</param>
        void Save<T>(T value) where T : class, new();

        /// <summary>Saves asynchronously an object in the database</summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="value">The value</param>
        /// <returns>The <see cref="Task"/>.</returns>
        Task SaveAsync<T>(T value) where T : class, new();

        /// <summary>
        /// Deletes an object from the database
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="value">The object to delete</param>
        void Delete<T>(T value) where T : class, new();

        /// <summary>Deletes asynchronously an object from the database</summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="value">The object to delete</param>
        /// <returns>The <see cref="Task"/>.</returns>
        Task DeleteAsync<T>(T value) where T : class, new();

        /// <summary>
        /// Creates a delete statement.
        /// </summary>
        /// <typeparam name="T">The type of object to delete</typeparam>
        /// <returns>An helper to create the delete statement.</returns>
        IDeleteResult<T, FolkeTuple> Delete<T>() where T : class, new();

        /// <summary>
        /// Refreshes an existing object, updating its properties with the values from the database
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="value">The object to update</param>
        /// <returns>The updated object</returns>
        T Refresh<T>(T value) where T : class, new();

        /// <summary>
        /// Updates all the references to an old element, that will point to a new element.
        /// The old element can then safely be deleted.
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="oldElement">The old element whose reference will point to the new element</param>
        /// <param name="newElement">The new element. All the references to the old element in the database
        /// will point to the new element.</param>
        void Merge<T>(T oldElement, T newElement) where T : class, IFolkeTable, new();

        /// <summary>
        /// Creates a table in the database
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="drop">Should the table be dropped if it already exists</param>
        void CreateTable<T>(bool drop = false) where T : class, new();

        /// <summary>
        /// Drop a table.
        /// </summary>
        /// <typeparam name="T">The type that is mapped to the table</typeparam>
        void DropTable<T>() where T : class, new();

        /// <summary>
        /// Creates or updates a table.
        /// Uses <see cref="UpdateSchema()"/> if you wand to do this for all the tables
        /// defined in <see cref="Mapper"/>.
        /// </summary>
        /// <typeparam name="T">The table type</typeparam>
        void CreateOrUpdateTable<T>() where T : class, new();

        /// <summary>
        /// Ensures that all the tables in the database match the mapping defined in the <see cref="Mapper"/>.
        /// </summary>
        void UpdateSchema();

        /// <summary>
        /// Ensures that all the tables in the database match the mapping defined in this assembly.
        /// See <see cref="IMapper"/> in order to know which types are mapped.
        /// </summary>
        /// <param name="assembly">The assembly</param>
        void UpdateSchema(Assembly assembly);

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <returns>The command</returns>
        IFolkeCommand CreateCommand();

        /// <summary>Creates a new command.</summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandParameters">The command parameters.</param>
        /// <returns>The command</returns>
        IFolkeCommand CreateCommand(string commandText, object[] commandParameters);

        /// <summary>
        /// Create a select expression
        /// </summary>
        /// <typeparam name="T">The base table</typeparam>
        /// <returns>The query</returns>
        ISelectResult<T, FolkeTuple> Select<T>() where T : class, new();

        /// <summary>
        /// Create a select expression with a parameter table
        /// </summary>
        /// <typeparam name="T">The table to select from</typeparam>
        /// <typeparam name="TParameters">The class that holds the parameter for the query</typeparam>
        /// <returns>The query</returns>
        ISelectResult<T, TParameters> Select<T, TParameters>() where T : class, new();

        /// <summary>
        /// Create a query that selects all the field from the T type
        /// </summary>
        /// <typeparam name="T">The table to select on</typeparam>
        /// <returns>The query</returns>
        IFromResult<T, FolkeTuple> SelectAllFrom<T>() where T : class, new();

        /// <summary>
        /// Create a query that selects all the fields from the T type and all the properties in parameter
        /// </summary>
        /// <typeparam name="T">The type to select on</typeparam>
        /// <param name="fetches">The other tables to select (using a left join)</param>
        /// <returns>A select expression</returns>
        IFromResult<T, FolkeTuple> SelectAllFrom<T>(params Expression<Func<T, object>>[] fetches)
            where T : class, new();

        /// <summary>
        /// Create a query that insert values in a table
        /// </summary>
        /// <typeparam name="T">The table where the values are inserted</typeparam>
        /// <returns>The query</returns>
        IInsertIntoResult<T, FolkeTuple> InsertInto<T>() where T : class, new();

        /// <summary>Creates a <see cref="IQueryable{T}"/> interface</summary>
        /// <typeparam name="T">The object type to query</typeparam>
        /// <returns>The <see cref="IQueryable{T}"/> object</returns>
        IQueryable<T> Query<T>();
    }
}
