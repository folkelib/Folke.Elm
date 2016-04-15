namespace Folke.Elm.Sqlite
{
    public class SqliteStringBuilder : SqlStringBuilder
    {
        public override void DuringAutoIncrement()
        {
            AppendAfterSpace("AUTOINCREMENT");
        }

        public override void DuringSymbol(string symbol)
        {
            AppendAfterSpace(symbol);
        }

        public override void DuringLastInsertedId()
        {
            AppendAfterSpace("last_insert_rowid()");
        }
    }
}