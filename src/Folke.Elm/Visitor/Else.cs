namespace Folke.Elm.Visitor
{
    public class Else : IVisitable
    {
        private readonly IVisitable value;

        public Else(IVisitable value)
        {
            this.value = value;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.BeforeElse();
            value.Accept(visitor);
        }
    }
}
