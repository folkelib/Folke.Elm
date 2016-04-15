namespace Folke.Elm.Visitor
{
    /// <summary>
    /// A column name with the table name.
    /// </summary>
    public class Column : IVisitable
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }

        public Column(string tableName, string columnName)
        {
            TableName = tableName;
            ColumnName = columnName;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.DuringColumn(this.TableName, this.ColumnName);
        }
    }
}
