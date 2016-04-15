namespace Folke.Elm.Visitor
{
    public class ConstantNumber : IVisitable
    {
        public int Value { get; set; }

        public ConstantNumber(int value)
        {
            Value = value;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.DuringConstantNumber(this.Value);
        }
    }
}
