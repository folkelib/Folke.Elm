using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using Folke.Elm.InformationSchema;
using Folke.Elm.Mapping;

namespace Folke.Elm
{
    public interface IDatabaseDriver
    {
        bool HasBooleanType { get; }

        DbConnection CreateConnection(string connectionString);
        string GetSqlType(PropertyInfo property, int maxLength);
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
