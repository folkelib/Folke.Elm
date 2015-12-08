using Folke.Elm.InformationSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Folke.Elm.Sqlite
{
    public class SqliteColumnDefinition : IColumnDefinition
    {
        public string ColumnName { get; internal set; }
        public string ColumnType { get; internal set; }
    }
}
