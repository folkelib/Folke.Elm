using Folke.Elm.InformationSchema;
using System.ComponentModel.DataAnnotations.Schema;

namespace Folke.Elm.Mysql
{
    [Table("COLUMNS", Schema = "INFORMATION_SCHEMA")]
    public class MySqlColumnDefinition : ColumnDefinition, IColumnDefinition
    {
        public string COLUMN_TYPE { get; set; }
        public string COLUMN_KEY { get; set; }
        public string EXTRA { get; set; }
        public string PRIVILEGES { get; set; }
        public string COLUMN_COMMENT { get; set; }

        public override string ColumnType
        {
            get { return COLUMN_TYPE; }
        }
    }
}
