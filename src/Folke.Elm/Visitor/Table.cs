namespace Folke.Elm.Visitor
{
    public class Table : IVisitable
    {
        public string Name { get; set; }
        public string Schema { get; set; }
        
        public Table(string name, string schema)
        {
            Name = name;
            Schema = schema;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.DuringTable(Schema, Name);
        }
    }
}
