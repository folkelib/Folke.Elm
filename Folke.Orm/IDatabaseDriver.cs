using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Orm
{
    public interface IDatabaseDriver
    {
        DbConnection CreateConnection();
        IDatabaseSettings Settings { get; }
        string GetSqlType(PropertyInfo property);
        bool EquivalentTypes(string firstType, string secondType);
        char BeginSymbol { get; }
        char EndSymbol { get; }
    }
}
