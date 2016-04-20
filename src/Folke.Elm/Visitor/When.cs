namespace Folke.Elm.Visitor
{
    public class When : IVisitable
    {
        private readonly IVisitable condition;
        private readonly IVisitable value;

        public When(IVisitable condition, IVisitable value)
        {
            this.condition = condition;
            this.value = value;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.BeforeWhen();
            condition.Accept(visitor);
            visitor.DuringWhen();
            value.Accept(visitor);
            visitor.AfterWhen();
        }
    }
}
