namespace Folke.Elm.Visitor
{
    public class Join : IVisitable
    {
        private readonly IVisitable @from;
        private readonly IVisitable @join;
        private readonly IVisitable selectSelector;
        private readonly IVisitable joinSelector;
        private readonly IVisitable @select;

        public Join(IVisitable from, IVisitable join, IVisitable selectSelector, IVisitable joinSelector, IVisitable select)
        {
            this.@from = @from;
            this.@join = @join;
            this.selectSelector = selectSelector;
            this.joinSelector = joinSelector;
            this.@select = @select;
        }

        public void Accept(IVisitor visitor)
        {
            @from.Accept(visitor);

        }
    }
}
