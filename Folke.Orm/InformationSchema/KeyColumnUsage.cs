using Folke.Orm.Mapping;

namespace Folke.Orm.InformationSchema
{
    [Table("KEY_COLUMN_USAGE", Schema = "INFORMATION_SCHEMA")]
    public class KeyColumnUsage
    {
        [Column("CONSTRAINT_CATALOG")]
        public string ConstraintCatalog { get; set; }

        [Column("CONSTRAINT_SCHEMA")]
        public string ConstraintSchema { get; set; }

        [Column("CONSTRAINT_NAME")]
        public string ConstraintName { get; set; }

        [Column("TABLE_CATALOG")]
        public string TableCatalog { get; set; }

        [Column("TABLE_SCHEMA")]
        public string TableSchema { get; set; }

        [Column("TABLE_NAME")]
        public string TableName { get; set; }

        [Column("COLUMN_NAME")]
        public string ColumnName { get; set; }

        [Column("ORDINAL_POSITION")]
        public string OrdinalPosition { get; set; }

        [Column("POSITION_IN_UNIQUE_CONSTRAINT")]
        public string PositionInUniqueConstraint { get; set; }

        [Column("REFERENCED_TABLE_SCHEMA")]
        public string ReferencedTableSchema { get; set; }

        [Column("REFERENCED_TABLE_NAME")]
        public string ReferencedTableName { get; set; }

        [Column("REFERENCED_COLUMN_NAME")]
        public string ReferencedColumnName { get; set; }
    }
}
