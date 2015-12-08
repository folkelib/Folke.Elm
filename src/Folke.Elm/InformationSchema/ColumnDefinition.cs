using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Folke.Elm.InformationSchema
{
    [Table("COLUMNS", Schema = "INFORMATION_SCHEMA")]
    public class ColumnDefinition : IColumnDefinition
    {
        public string TABLE_CATALOG { get; set; }
        public string TABLE_SCHEMA { get; set; }
        public string TABLE_NAME { get; set; }
        public string COLUMN_NAME { get; set; }
        public int ORDINAL_POSITION { get; set; }
        public string COLUMN_DEFAULT { get;set; }
        public string IS_NULLABLE { get; set; }
        public string DATA_TYPE { get; set; }
        public int? CHARACTER_MAXIMUM_LENGTH { get; set; }
        public int? CHARACTER_OCTET_LENGTH { get; set; }
        public byte? NUMERIC_PRECISION { get; set; }
        public int? NUMERIC_SCALE { get; set; }
        public string CHARACTER_SET_NAME { get; set; }
        public string COLLATION_NAME { get; set; }

        [NotMapped]
        public virtual string ColumnName
        {
            get { return COLUMN_NAME; }
        }

        [NotMapped]
        public virtual string ColumnType
        {
            get { return DATA_TYPE; }
        }
    }
}
