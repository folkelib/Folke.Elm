namespace Folke.Elm.Visitor
{
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
            visitor.During(this);
        }
    }
}
