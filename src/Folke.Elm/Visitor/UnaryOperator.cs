using System;

namespace Folke.Elm.Visitor
{
    public class UnaryOperator : IVisitable
    {
        public UnaryOperatorType OperatorType { get; set; }
        public IVisitable Child { get; set; }

        public UnaryOperator(UnaryOperatorType operatorType, IVisitable child)
        {
            OperatorType = operatorType;
            Child = child;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.BeforeUnaryOperator(this.OperatorType);
            Child.Accept(visitor);
            visitor.AfterUnaryOperator(this.OperatorType);
        }
    }
}
