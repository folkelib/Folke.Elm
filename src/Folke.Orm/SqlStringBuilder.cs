using System.Text;

namespace Folke.Orm
{
    public class SqlStringBuilder
    {
        protected readonly StringBuilder stringBuilder = new StringBuilder();

        public SqlStringBuilder Append(string s)
        {
            stringBuilder.Append(s);
            return this;
        }

        public virtual void AppendSpace()
        {
            if (stringBuilder.Length > 0)
                stringBuilder.Append(' ');
        }

        public SqlStringBuilder Append(char c)
        {
            stringBuilder.Append(c);
            return this;
        }

        public override string ToString()
        {
            return stringBuilder.ToString();
        }

        public void Clear()
        {
            stringBuilder.Clear();
        }

        public SqlStringBuilder Append(int i)
        {
            stringBuilder.Append(i);
            return this;
        }

        public virtual void AppendSymbol(string symbol)
        {
            stringBuilder.Append('"');
            stringBuilder.Append(symbol);
            stringBuilder.Append('"');
        }

        public virtual void AppendAutoIncrement()
        {
            stringBuilder.Append(" AUTO_INCREMENT");
        }

        public virtual void AppendLastInsertedId()
        {
            stringBuilder.Append(" last_insert_id()");
        }
    }
}
