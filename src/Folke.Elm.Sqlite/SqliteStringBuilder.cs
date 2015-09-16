namespace Folke.Elm.Sqlite
{
    public class SqliteStringBuilder : SqlStringBuilder
    {
        public override void AppendAutoIncrement()
        {
            AppendAfterSpace("AUTOINCREMENT");
        }

        public override void AppendSymbol(string symbol)
        {
            AppendAfterSpace(symbol);
        }

        public override void AppendLastInsertedId()
        {
            AppendAfterSpace("last_insert_rowid()");
        }
    }
}