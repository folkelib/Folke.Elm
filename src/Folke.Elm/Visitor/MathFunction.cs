namespace Folke.Elm.Visitor
{
    public class MathFunction : IVisitable
    {
        private readonly IVisitable parameter;
        public MathFunctionType Type { get; set; }

        public MathFunction(MathFunctionType type, IVisitable parameter)
        {
            this.parameter = parameter;
            Type = type;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.BeforeMathFunction(this.Type);
            parameter.Accept(visitor);
            visitor.AfterMathFunction();
        }
    }
}
