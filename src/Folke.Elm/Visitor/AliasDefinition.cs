namespace Folke.Elm.Visitor
{
    public class AliasDefinition : IVisitable
    {
        public string Alias { get; set; }
        private readonly IVisitable value;
        
        public AliasDefinition(IVisitable value, string alias)
        {
            Alias = alias;
            this.value = value;
        }

        public void Accept(IVisitor visitor)
        {
            value.Accept(visitor);
            visitor.DuringAliasDefinition(this.Alias);
        }
    }
}
