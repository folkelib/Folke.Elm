using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Folke.Elm.Visitor
{
    public class BinaryOperator : IVisitable
    {
        private readonly IVisitable left;
        private readonly IVisitable right;
        public BinaryOperatorType Type { get; set; }

        public BinaryOperator(BinaryOperatorType type, IVisitable left, IVisitable right)
        {
            this.left = left;
            this.right = right;
            Type = type;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Before(this);
            left.Accept(visitor);
            visitor.During(this);
            right.Accept(visitor);
            visitor.After(this);
        }
    }
}
