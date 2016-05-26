using System.ComponentModel.DataAnnotations.Schema;
using Folke.Elm.InformationSchema;

namespace Folke.Elm.PostgreSql
{
    [Table("columns", Schema = "information_schema")]
    public class PostgreSqlColumnDefinition : IColumnDefinition
    {
        [Column("column_name")]
        public string ColumnName { get; set; }

        [Column("data_type")]
        public string ColumnType { get; set; }
        
        [Column("table_name")]
        public string TableName { get; set; }

        [Column("table_schema")]
        public string TableSchema { get; set; }
    }
}
