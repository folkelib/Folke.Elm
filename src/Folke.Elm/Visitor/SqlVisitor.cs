using System;

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
                    query.AppendAfterSpace("IS NULL");
                    break;
                case UnaryOperatorType.IsNotNull:
                    query.AppendAfterSpace("IS NOT NULL");
                    break;
            }
        }

        public void During(Parameter binaryOperator)
        {
            query.AppendAfterSpace("@Item" + binaryOperator.Index);
        }

        public void During(NamedParameter binaryOperator)
        {
            query.AppendAfterSpace("@" + binaryOperator.Name);
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
            query.AppendAfterSpace("WHERE");
        }

        public void During(Skip binaryOperator)
        {
            query.BeforeLimit();
        }

        public void During(Take binaryOperator)
        {
            query.DuringLimit();
        }

        public void During(OrderBy binaryOperator)
        {
            query.AppendAfterSpace("ORDER BY ");
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
            query.AppendAfterSpace("BETWEEN");
        }

        public void During(Between binaryOperator)
        {
            query.AppendAfterSpace("AND");
        }

        public void Before(MathFunction unaryOperator)
        {
            switch (unaryOperator.Type)
            {
                case MathFunctionType.Abs:
                    query.AppendAfterSpace("ABS(");
                    break;
                case MathFunctionType.Cos:
                    query.AppendAfterSpace("COS(");
                    break;
                case MathFunctionType.Max:
                    query.AppendAfterSpace("MAX(");
                    break;
                case MathFunctionType.Sin:
                    query.AppendAfterSpace("SIN(");
                    break;
                case MathFunctionType.Sum:
                    query.AppendAfterSpace("SUM(");
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

        public void During(Fields fields)
        {
            query.Append(",");
        }

        public void During(AliasDefinition aliasDefinition)
        {
            if (!noAlias)
            {
                query.AppendAfterSpace("AS ");
                query.Append(aliasDefinition.Alias);
            }
        }

        public void Before(Select selectNode)
        {
            query.Append("SELECT");
        }

        public void During(Select selectNode)
        {
            query.AppendAfterSpace("FROM");
        }

        public void During(Table table)
        {
            query.AppendSpace();
            if (!string.IsNullOrEmpty(table.Schema))
            {
                query.AppendSymbol(table.Schema);
                query.Append('.');
            }
            query.AppendSymbol(table.Name);
        }

        public void After(Take binaryOperator)
        {
            query.AfterLimit();
        }
    }
}
