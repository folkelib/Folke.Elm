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
        DbConnection CreateConnection(string connectionString);
        string GetSqlType(PropertyInfo property, int maxLength);
        bool EquivalentTypes(string firstType, string secondType);

        IList<ColumnDefinition> GetColumnDefinitions(FolkeConnection connection, TypeMapping typeMap);

        IList<TableDefinition> GetTableDefinitions(FolkeConnection connection);

        SqlStringBuilder CreateSqlStringBuilder();

        object ConvertValueToParameter(IMapper mapper, object value);
        object ConvertReaderValueToProperty(object readerValue, Type propertyType);

        // Capabilities
        bool CanAddIndexInCreateTable();
    }
}
