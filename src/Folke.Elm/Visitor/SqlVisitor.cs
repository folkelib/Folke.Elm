using System;
using System.Linq.Expressions;

namespace Folke.Elm.Visitor
{
    public class SqlVisitor : IVisitor
    {
        private readonly SqlStringBuilder query;
        private readonly bool noAlias;

        public SqlVisitor(SqlStringBuilder query, bool noAlias)
        {
            this.query = query;
            this.noAlias = noAlias;
        }

        public void Before(UnaryOperator unaryOperator)
        {
            switch (unaryOperator.OperatorType)
            {
                case UnaryOperatorType.Negate:
                    query.Append("-");
                    break;

                case UnaryOperatorType.Not:
                    query.Append(" NOT ");
                    break;
            }
        }

        public void During(ConstantNumber binaryOperator)
        {
            query.Append(binaryOperator.Value);
        }

        public void During(BinaryOperator binaryOperator)
        {
            switch (binaryOperator.Type)
            {
                case BinaryOperatorType.Add:
                    query.Append("+");
                    break;
                case BinaryOperatorType.AndAlso:
                    query.Append(" AND ");
                    break;
                case BinaryOperatorType.Divide:
                    query.Append("/");
                    break;
                case BinaryOperatorType.Equal:
                    query.Append('=');
                    break;
                case BinaryOperatorType.GreaterThan:
                    query.Append(">");
                    break;
                case BinaryOperatorType.GreaterThanOrEqual:
                    query.Append(">=");
                    break;
                case BinaryOperatorType.In:
                    query.Append(" IN ");
                    break;
                case BinaryOperatorType.LessThan:
                    query.Append("<");
                    break;
                case BinaryOperatorType.LessThanOrEqual:
                    query.Append("<=");
                    break;
                case BinaryOperatorType.Like:
                    query.Append(" LIKE");
                    break;
                case BinaryOperatorType.Modulo:
                    query.Append("%");
                    break;
                case BinaryOperatorType.Multiply:
                    query.Append('*');
                    break;
                case BinaryOperatorType.NotEqual:
                    query.Append("<>");
                    break;
                case BinaryOperatorType.OrElse:
                    query.Append(" OR ");
                    break;
                case BinaryOperatorType.Subtract:
                    query.Append('-');
                    break;
                default:
                    throw new Exception("Expression type not supported");
            }
        }

        public void Before(BinaryOperator unaryOperator)
        {
            query.Append("(");
        }

        public void After(BinaryOperator binaryOperator)
        {
            query.Append(")");
        }

        public void After(UnaryOperator binaryOperator)
        {
            switch (binaryOperator.OperatorType)
            {
                case UnaryOperatorType.IsNull:
                    query.Append(" IS NULL");
                    break;
                case UnaryOperatorType.IsNotNull:
                    query.Append(" IS NOT NULL");
                    break;
            }
        }

        public void During(Parameter binaryOperator)
        {
            query.Append(" @Item" + binaryOperator.Index);
        }

        public void During(NamedParameter binaryOperator)
        {
            query.Append(" @" + binaryOperator.Name);
        }

        public void During(Column binaryOperator)
        {
            query.Append(' ');
            if (binaryOperator.TableName != null && !noAlias)
            {
                query.AppendSymbol(binaryOperator.TableName);
                query.Append(".");
            }
            query.AppendSymbol(binaryOperator.ColumnName);
        }

        public void During(Where binaryOperator)
        {
            query.Append("WHERE");
        }

        public void During(Skip binaryOperator)
        {
            query.BeforeLimit();
            query.Append(binaryOperator.Count);
        }

        public void During(Take binaryOperator)
        {
            query.DuringLimit();
            query.Append(binaryOperator.Count);
            query.AfterLimit();
        }

        public void During(OrderBy binaryOperator)
        {
            query.Append("ORDER BY ");
        }

        public void During(Values binaryOperator)
        {
            query.Append(",");
        }

        public void Before(Values unaryOperator)
        {
            query.Append("(");
        }

        public void After(Values binaryOperator)
        {
            query.Append(')');
        }

        public void Before(Between unaryOperator)
        {
            query.Append(" BETWEEN ");
        }

        public void During(Between binaryOperator)
        {
            query.Append(" AND ");
        }

        public void Before(MathFunction unaryOperator)
        {
            switch (unaryOperator.Type)
            {
                case MathFunctionType.Abs:
                    query.Append(" ABS(");
                    break;
                case MathFunctionType.Cos:
                    query.Append(" COS(");
                    break;
                case MathFunctionType.Max:
                    query.Append(" MAX(");
                    break;
                case MathFunctionType.Sin:
                    query.Append(" SIN(");
                    break;
                case MathFunctionType.Sum:
                    query.Append(" SUM(");
                    break;
            }
        }

        public void After(MathFunction binaryOperator)
        {
            query.Append(")");
        }

        public void During(LastInsertedId binaryOperator)
        {
            query.AppendLastInsertedId();
        }
    }
}
