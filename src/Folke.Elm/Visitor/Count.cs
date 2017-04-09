namespace Folke.Elm.Visitor
{
    public class Count : IVisitable
    {
        public void Accept(IVisitor visitor)
        {
            visitor.DuringCount();
        }
    }
}
