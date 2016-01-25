namespace Folke.Elm.Visitor
{
    public interface IVisitor
    {
        void Before(UnaryOperator unaryOperator);
        void During(ConstantNumber binaryOperator);
        void During(BinaryOperator binaryOperator);
        void Before(BinaryOperator unaryOperator);
        void After(BinaryOperator binaryOperator);
        void After(UnaryOperator binaryOperator);
        void During(Parameter binaryOperator);
        void During(NamedParameter binaryOperator);
        void During(Column binaryOperator);
        void During(Where binaryOperator);
        void During(Skip binaryOperator);
        void During(Take binaryOperator);
        void During(OrderBy binaryOperator);
        void During(Values binaryOperator);
        void Before(Values unaryOperator);
        void After(Values binaryOperator);
        void Before(Between unaryOperator);
        void During(Between binaryOperator);
        void Before(MathFunction unaryOperator);
        void After(MathFunction binaryOperator);
        void During(LastInsertedId binaryOperator);
    }
}