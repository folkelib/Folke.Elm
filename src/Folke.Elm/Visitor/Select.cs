namespace Folke.Elm.Visitor
{
    public class Select : IVisitable
    {
        private readonly IVisitable selection;
        private readonly IVisitable @from;

        public Select(IVisitable selection, IVisitable from)
        {
            this.selection = selection;
            this.@from = @from;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Before(this);
            selection.Accept(visitor);
            visitor.During(this);
            @from.Accept(visitor);
        }
    }
}
