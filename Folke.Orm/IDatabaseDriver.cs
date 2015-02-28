using System.Data.Common;
using System.Reflection;

namespace Folke.Orm
{
    public interface IDatabaseDriver
    {
        DbConnection CreateConnection(string connectionString);
        IDatabaseSettings Settings { get; }
        string GetSqlType(PropertyInfo property);
        bool EquivalentTypes(string firstType, string secondType);
        char BeginSymbol { get; }
        char EndSymbol { get; }
    }
}
