namespace Folke.Elm.Visitor
{
    public class NamedParameter : IVisitable
    {
        public string Name { get; set; }

        public NamedParameter(string name)
        {
            Name = name;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.DuringNamedParameter(this.Name);
        }
    }
}
