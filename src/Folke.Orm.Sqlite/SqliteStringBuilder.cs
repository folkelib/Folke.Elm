namespace Folke.Orm.Sqlite
{
    public class SqliteStringBuilder : SqlStringBuilder
    {
        public override void AppendAutoIncrement()
        {
        }

        public override void AppendSymbol(string symbol)
        {
            stringBuilder.Append(symbol);
        }

        public override void AppendLastInsertedId()
        {
            stringBuilder.Append(" last_insert_rowid()");
        }
    }
}