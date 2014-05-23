namespace Folke.Orm.InformationSchema
{
    [Table("TABLES", Schema = "INFORMATION_SCHEMA")]
    internal class Tables
    {
        public string TABLE_NAME { get; set; }
        public string TABLE_SCHEMA { get; set; }
    }
}
