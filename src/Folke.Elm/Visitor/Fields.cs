using System.Collections.Generic;

namespace Folke.Elm.Visitor
{
    public class Fields : IVisitable
    {
        private readonly IList<IVisitable> fields;

        public Fields(IList<IVisitable> fields)
        {
            this.fields = fields;
        }

        public void Accept(IVisitor visitor)
        {
            foreach (var field in fields)
            {
                if (field != fields[0]) visitor.During(this);

                field.Accept(visitor);
            }
        }
    }
}
