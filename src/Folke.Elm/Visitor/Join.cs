namespace Folke.Elm.Visitor
{
    public class Join : IVisitable
    {
        public Join(IVisitable from, IVisitable join, IVisitable selectSelector, IVisitable joinSelector, IVisitable select)
        {
        }

        public void Accept(IVisitor visitor)
        {
            throw new System.NotImplementedException();
        }
    }
}
