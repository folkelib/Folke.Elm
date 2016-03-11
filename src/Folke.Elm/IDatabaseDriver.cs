using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using Folke.Elm.InformationSchema;
using Folke.Elm.Mapping;

namespace Folke.Elm
{
    /// <summary>A database driver, allows to abstract the access to the underlying database vendor</summary>
    public interface IDatabaseDriver
    {
        /// <summary>Gets a value indicating whether the boolean type supported or not.
        /// Otherwise it will be emulated with a number</summary>
        bool HasBooleanType { get; }

        /// <summary>Creates a new database connection</summary>
        /// <param name="connectionString">The connection string</param>
        /// <returns>The connection</returns>
        DbConnection CreateConnection(string connectionString);

        /// <summary>Gets the SQL Type for property</summary>
        /// <param name="property">The property</param>
        /// <param name="maxLength">The maximum length of the column</param>
        /// <returns>The full column type, including length</returns>
        string GetSqlType(PropertyInfo property, int maxLength);

        /// <summary>Are the two types equivalent (used by the schema updater)</summary>
        /// <param name="firstType">The first type</param>
        /// <param name="secondType">The second type</param>
        /// <returns>true if the two types are equivalent and no schema change is necessary</returns>
        bool EquivalentTypes(string firstType, string secondType);

        IList<IColumnDefinition> GetColumnDefinitions(FolkeConnection connection, TypeMapping typeMap);

        IList<TableDefinition> GetTableDefinitions(FolkeConnection connection);

        SqlStringBuilder CreateSqlStringBuilder();

        object ConvertValueToParameter(IMapper mapper, object value);

        object ConvertReaderValueToProperty(object readerValue, Type propertyType);

        object ConvertReaderValueToValue(DbDataReader reader, Type type, int index);

        // Capabilities
        bool CanAddIndexInCreateTable();

        bool CanDoMultipleActionsInAlterTable();
    }
}
