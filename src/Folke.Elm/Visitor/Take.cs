namespace Folke.Elm.Visitor
{
    public class Take : IVisitable
    {
        private readonly IVisitable collection;
        private readonly IVisitable count;

        public Take(IVisitable collection, IVisitable count)
        {
            this.collection = collection;
            this.count = count;
        }

        public void Accept(IVisitor visitor)
        {
            collection.Accept(visitor);
            visitor.DuringTake();
            count.Accept(visitor);
            visitor.AfterTake();
        }
    }
}
