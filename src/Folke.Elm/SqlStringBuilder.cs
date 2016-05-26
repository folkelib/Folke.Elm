using System;
using System.Text;
using Folke.Elm.Visitor;

namespace Folke.Elm
{
    
    public class SqlStringBuilder : IVisitor
    {
        public bool NoAlias { get; set; }
        protected readonly StringBuilder stringBuilder = new StringBuilder();

        public void Append(string s)
        {
            stringBuilder.Append(s);
        }

        public void AppendAfterSpace(string s)
        {
            AppendSpace();
            stringBuilder.Append(s);
        }

        public virtual void AppendSpace()
        {
            if (stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] != ' ')
                stringBuilder.Append(' ');
        }

        public void Append(char c)
        {
            stringBuilder.Append(c);
        }

        public override string ToString()
        {
            return stringBuilder.ToString();
        }

        public void Clear()
        {
            stringBuilder.Clear();
        }

        public void Append(int i)
        {
            stringBuilder.Append(i);
        }

        public virtual void DuringSymbol(string symbol)
        {
            stringBuilder.Append('"');
            stringBuilder.Append(symbol);
            stringBuilder.Append('"');
        }
        
        public virtual void DuringPrimaryKey(bool isAutomatic)
        {
            stringBuilder.Append(" PRIMARY KEY");
            if (isAutomatic)
                stringBuilder.Append(" AUTO_INCREMENT");
        }

        public virtual void DuringLastInsertedId()
        {
            stringBuilder.Append(" last_insert_id()");
        }

        public virtual void BeforeDropTable()
        {
            Append("DROP TABLE ");
        }

        public virtual void DuringSkip()
        {
            AppendAfterSpace("LIMIT ");
        }

        public virtual void DuringTake()
        {
            Append(",");
        }

        public virtual void AfterTake()
        {
        }

        public void DuringMathFunction()
        {
            Append(",");
        }

        public void BeforeCase()
        {
            AppendAfterSpace("CASE");
        }

        public void AfterCase()
        {
            AppendAfterSpace("END");
        }

        public void BeforeWhen()
        {
            AppendAfterSpace("WHEN");
        }

        public void DuringWhen()
        {
            AppendAfterSpace("THEN");
        }

        public void AfterWhen()
        {
        }

        public void BeforeElse()
        {
            AppendAfterSpace("ELSE");
        }

        public virtual void BeforeAddColumn()
        {
            AppendAfterSpace("ADD COLUMN ");
        }

        public virtual void BeforeAlterColumnType(string previousColumnName)
        {
            Append(" CHANGE COLUMN ");
            DuringSymbol(previousColumnName);
            Append(" ");
            DuringSymbol(previousColumnName);
        }

        public void BeforeUnaryOperator(UnaryOperatorType unaryOperatorType)
        {
            switch (unaryOperatorType)
            {
                case UnaryOperatorType.Negate:
                    Append("-");
                    break;

                case UnaryOperatorType.Not:
                    Append(" NOT ");
                    break;
            }
        }

        public void DuringConstantNumber(int value)
        {
            Append(value);
        }

        public void DuringBinaryOperator(BinaryOperatorType binaryOperatorType)
        {
            switch (binaryOperatorType)
            {
                case BinaryOperatorType.Add:
                    Append("+");
                    break;
                case BinaryOperatorType.AndAlso:
                    Append(" AND ");
                    break;
                case BinaryOperatorType.Divide:
                    Append("/");
                    break;
                case BinaryOperatorType.Equal:
                    Append('=');
                    break;
                case BinaryOperatorType.GreaterThan:
                    Append(">");
                    break;
                case BinaryOperatorType.GreaterThanOrEqual:
                    Append(">=");
                    break;
                case BinaryOperatorType.In:
                    Append(" IN ");
                    break;
                case BinaryOperatorType.LessThan:
                    Append("<");
                    break;
                case BinaryOperatorType.LessThanOrEqual:
                    Append("<=");
                    break;
                case BinaryOperatorType.Like:
                    Append(" LIKE");
                    break;
                case BinaryOperatorType.Modulo:
                    Append("%");
                    break;
                case BinaryOperatorType.Multiply:
                    Append('*');
                    break;
                case BinaryOperatorType.NotEqual:
                    Append("<>");
                    break;
                case BinaryOperatorType.OrElse:
                    Append(" OR ");
                    break;
                case BinaryOperatorType.Subtract:
                    Append('-');
                    break;
                case BinaryOperatorType.Or:
                    Append("|");
                    break;
                case BinaryOperatorType.And:
                    Append("&");
                    break;
                default:
                    throw new Exception("Expression type not supported");
            }
        }

        public void BeforeBinaryOperator()
        {
            Append("(");
        }

        public void AfterBinaryOperator()
        {
            Append(")");
        }

        public void AfterUnaryOperator(UnaryOperatorType unaryOperatorType)
        {
            switch (unaryOperatorType)
            {
                case UnaryOperatorType.IsNull:
                    AppendAfterSpace("IS NULL");
                    break;
                case UnaryOperatorType.IsNotNull:
                    AppendAfterSpace("IS NOT NULL");
                    break;
            }
        }

        public void DuringParameter(int index)
        {
            AppendAfterSpace("@Item" + index);
        }

        public void DuringNamedParameter(string name)
        {
            AppendAfterSpace("@" + name);
        }

        public void DuringColumn(string tableName, string columnName)
        {
            Append(' ');
            if (tableName != null && !NoAlias)
            {
                DuringSymbol(tableName);
                Append(".");
            }
            DuringSymbol(columnName);
        }

        public void BeforeWhere()
        {
            AppendAfterSpace("WHERE");
        }
        
        public void BeforeOrderBy()
        {
            AppendAfterSpace("ORDER BY ");
        }

        public void DuringOrderBy()
        {
            Append(',');
        }

        public void DuringValues()
        {
            Append(",");
        }

        public void BeforeValues()
        {
            Append("(");
        }

        public void AfterValues()
        {
            Append(')');
        }

        public void BeforeBetween()
        {
            AppendAfterSpace("BETWEEN");
        }

        public void DuringBetween()
        {
            AppendAfterSpace("AND");
        }

        public void BeforeMathFunction(MathFunctionType mathFunctionType)
        {
            switch (mathFunctionType)
            {
                case MathFunctionType.Abs:
                    AppendAfterSpace("ABS(");
                    break;
                case MathFunctionType.Cos:
                    AppendAfterSpace("COS(");
                    break;
                case MathFunctionType.Max:
                    AppendAfterSpace("MAX(");
                    break;
                case MathFunctionType.Sin:
                    AppendAfterSpace("SIN(");
                    break;
                case MathFunctionType.Sum:
                    AppendAfterSpace("SUM(");
                    break;
                case MathFunctionType.IsNull:
                    AppendAfterSpace("ISNULL(");
                    break;
            }
        }

        public void AfterMathFunction()
        {
            Append(")");
        }
        
        public void DuringFields()
        {
            Append(",");
        }

        public void DuringAliasDefinition(string aliasName)
        {
            if (!NoAlias)
            {
                AppendAfterSpace("AS ");
                Append(aliasName);
            }
        }

        public void BeforeSelect()
        {
            Append("SELECT");
        }

        public void DuringSelect()
        {
            AppendAfterSpace("FROM");
        }

        public void DuringTable(string schema, string tableName)
        {
            AppendSpace();
            if (!string.IsNullOrEmpty(schema))
            {
                DuringSymbol(schema);
                Append('.');
            }
            DuringSymbol(tableName);
        }

        public void BeforeSubExpression()
        {
            Append("(");
        }

        public void AfterSubExpression()
        {
            Append(")");
        }

        public void BeforeGroupBy()
        {
            Append("GROUP BY ");
        }

        public void DuringGroupBy()
        {
            Append(',');
        }

        public void BeforeSet()
        {
            AppendAfterSpace("SET ");
        }

        public void DuringSet()
        {
            Append(", ");
        }

        public void BeforeUpdate()
        {
            NoAlias = true;
            Append("UPDATE");
        }

        public void BeforeDelete()
        {
            NoAlias = true;
            Append("DELETE");
        }

        public void BeforeColumnTypeDefinition()
        {
            Append(" ");
        }
    }
}
