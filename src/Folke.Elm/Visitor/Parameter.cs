namespace Folke.Elm.Visitor
{
    public class Parameter : IVisitable
    {
        public int Index { get; set; }

        public Parameter(int index)
        {
            Index = index;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.DuringParameter(this.Index);
        }
    }
}
