using System.Collections.Generic;

namespace Folke.Elm.Visitor
{
    public class Case : IVisitable
    {
        private readonly IEnumerable<IVisitable> cases;

        public Case(IEnumerable<IVisitable> cases)
        {
            this.cases = cases;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.BeforeCase();
            foreach (var c in cases)
            {
                c.Accept(visitor);
            }
            visitor.AfterCase();
        }
    }
}
