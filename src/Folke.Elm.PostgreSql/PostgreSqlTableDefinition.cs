using System.ComponentModel.DataAnnotations.Schema;

namespace Folke.Elm.PostgreSql
{
    [Table("tables", Schema = "information_schema")]
    public class PostgreSqlTableDefinition
    {
        [Column("table_name")]
        public string Name { get; set; }

        [Column("table_schema")]
        public string Schema { get; set; }
    }
}
