namespace Folke.Elm.Visitor
{
    public class Skip : IVisitable
    {
        public int Count { get; set; }
        private readonly IVisitable collection;

        public Skip(IVisitable collection, int count)
        {
            Count = count;
            this.collection = collection;
        }

        public void Accept(IVisitor visitor)
        {
            collection.Accept(visitor);
            visitor.During(this);
        }
    }
}
