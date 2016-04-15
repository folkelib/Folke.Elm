namespace Folke.Elm.Visitor
{
    public class OrderBy : IVisitable
    {
        private readonly IVisitable collection;
        private readonly IVisitable criteria;

        public OrderBy(IVisitable collection, IVisitable criteria)
        {
            this.collection = collection;
            this.criteria = criteria;
        }

        public void Accept(IVisitor visitor)
        {
            collection.Accept(visitor);
            visitor.BeforeOrderBy();
            criteria.Accept(visitor);
        }
    }
}
