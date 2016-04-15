using System.Collections.Generic;

namespace Folke.Elm.Visitor
{
    public class Values : IVisitable
    {
        private readonly IList<IVisitable> values;

        public Values(IList<IVisitable> values)
        {
            this.values = values;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.BeforeValues();
            bool first = true;
            foreach (var value in values)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    visitor.DuringValues();
                }
                value.Accept(visitor);
            }
            visitor.AfterValues();
        }
    }
}
