namespace Folke.Elm.Visitor
{
    public interface IVisitor
    {
        void BeforeUnaryOperator(UnaryOperatorType unaryOperatorType);
        void DuringConstantNumber(int value);
        void DuringBinaryOperator(BinaryOperatorType binaryOperatorType);
        void BeforeBinaryOperator();
        void AfterBinaryOperator();
        void AfterUnaryOperator(UnaryOperatorType unaryOperatorType);
        void DuringParameter(int index);
        void DuringNamedParameter(string name);
        void DuringColumn(string tableName, string columnName);
        void BeforeWhere();
        void DuringSkip();
        void DuringTake();
        void BeforeOrderBy();
        void DuringValues();
        void BeforeValues();
        void AfterValues();
        void BeforeBetween();
        void DuringBetween();
        void BeforeMathFunction(MathFunctionType mathFunctionType);
        void AfterMathFunction();
        void DuringLastInsertedId();
        void DuringFields();
        void DuringAliasDefinition(string aliasName);
        void BeforeSelect();
        void DuringSelect();
        void DuringTable(string schema, string tableName);
        void AfterTake();
    }
}