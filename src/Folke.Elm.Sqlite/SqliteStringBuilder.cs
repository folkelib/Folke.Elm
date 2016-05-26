namespace Folke.Elm.Sqlite
{
    public class SqliteStringBuilder : SqlStringBuilder
    {
        public override void DuringPrimaryKey(bool autoIncrement)
        {
            AppendAfterSpace("PRIMARY KEY");
            if (autoIncrement)
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