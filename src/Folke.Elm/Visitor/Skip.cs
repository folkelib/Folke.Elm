namespace Folke.Elm.Visitor
{
    public class Skip : IVisitable
    {
        private readonly IVisitable collection;
        private readonly IVisitable count;

        public Skip(IVisitable collection, IVisitable count)
        {
            this.collection = collection;
            this.count = count;
        }

        public void Accept(IVisitor visitor)
        {
            collection.Accept(visitor);
            visitor.DuringSkip();
            count.Accept(visitor);
        }
    }
}
