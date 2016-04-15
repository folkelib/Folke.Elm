using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Folke.Elm.Visitor
{
    public class Between : IVisitable
    {
        private readonly IVisitable value;
        private readonly IVisitable min;
        private readonly IVisitable max;

        public Between(IVisitable value, IVisitable min, IVisitable max)
        {
            this.value = value;
            this.min = min;
            this.max = max;
        }

        public void Accept(IVisitor visitor)
        {
            value.Accept(visitor);
            visitor.BeforeBetween();
            min.Accept(visitor);
            visitor.DuringBetween();
            max.Accept(visitor);
        }
    }
}
