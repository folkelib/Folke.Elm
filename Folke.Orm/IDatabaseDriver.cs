using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using Folke.Orm.InformationSchema;
using Folke.Orm.Mapping;

namespace Folke.Orm
{
    public interface IDatabaseDriver
    {
        DbConnection CreateConnection(string connectionString);
        string GetSqlType(PropertyInfo property, int maxLength);
        bool EquivalentTypes(string firstType, string secondType);

        IList<ColumnDefinition> GetColumnDefinitions(FolkeConnection connection, TypeMapping typeMap);

        IList<TableDefinition> GetTableDefinitions(FolkeConnection connection, string p);

        SqlStringBuilder CreateSqlStringBuilder();
    }
}
