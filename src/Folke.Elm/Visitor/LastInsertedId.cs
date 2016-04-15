namespace Folke.Elm.Visitor
{
    public class LastInsertedId : IVisitable
    {
        public void Accept(IVisitor visitor)
        {
            visitor.DuringLastInsertedId();
        }
    }
}
