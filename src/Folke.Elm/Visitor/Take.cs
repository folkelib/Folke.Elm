namespace Folke.Elm.Visitor
{
    public class Take : IVisitable
    {
        private readonly IVisitable collection;
        public int Count { get; set; }

        public Take(IVisitable collection, int count)
        {
            this.collection = collection;
            Count = count;
        }

        public void Accept(IVisitor visitor)
        {
            collection.Accept(visitor);
            visitor.During(this);
        }
    }
}
