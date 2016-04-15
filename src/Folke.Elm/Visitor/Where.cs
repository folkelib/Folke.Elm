namespace Folke.Elm.Visitor
{
    public class Where : IVisitable
    {
        private readonly IVisitable list;
        private readonly IVisitable expression;

        public Where(IVisitable list, IVisitable expression)
        {
            this.list = list;
            this.expression = expression;
        }

        public void Accept(IVisitor visitor)
        {
            list.Accept(visitor);
            visitor.BeforeWhere();
            expression.Accept(visitor);
        }
    }
}
