using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using Folke.Orm.InformationSchema;

namespace Folke.Orm
{
    public interface IDatabaseDriver
    {
        DbConnection CreateConnection(string connectionString);
        IDatabaseSettings Settings { get; }
        string GetSqlType(PropertyInfo property);
        bool EquivalentTypes(string firstType, string secondType);

        IList<ColumnDefinition> GetColumnDefinitions(FolkeConnection connection, TypeMapping typeMap);

        IList<TableDefinition> GetTableDefinitions(FolkeConnection connection, string p);

        SqlStringBuilder CreateSqlStringBuilder();
    }
}
