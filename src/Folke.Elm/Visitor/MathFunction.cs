namespace Folke.Elm.Visitor
{
    public class MathFunction : IVisitable
    {
        private readonly IVisitable[] parameters;
        public MathFunctionType Type { get; set; }

        public MathFunction(MathFunctionType type, params IVisitable[] parameters)
        {
            this.parameters = parameters;
            Type = type;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.BeforeMathFunction(this.Type);
            bool first = true;
            foreach (var parameter in parameters)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    visitor.DuringMathFunction();
                }
                parameter.Accept(visitor);
            }
            visitor.AfterMathFunction();
        }
    }
}
