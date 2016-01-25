namespace Folke.Elm.Visitor
{
    public interface IVisitable
    {
        void Accept(IVisitor visitor);
    }
}
