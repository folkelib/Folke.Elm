namespace Folke.Orm.InformationSchema
{
    [Table("TABLES", Schema = "INFORMATION_SCHEMA")]
    internal class Tables : TableDefinition
    {
        [Column("TABLE_NAME")]
        public override string Name { get; set; }
        [Column("TABLE_SCHEMA")]
        public override string Schema { get; set; }
    }
}
