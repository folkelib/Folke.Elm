namespace Folke.Elm.PostgreSql
{
    public class PostgreSqlStringBuilder : SqlStringBuilder
    {
        public override void DuringPrimaryKey(bool autoIncrement)
        {
            AppendAfterSpace("PRIMARY KEY");
        }

        public override void DuringLastInsertedId()
        {
            stringBuilder.Append(" LASTVAL()");
        }
        
        public override void DuringSkip()
        {
            AppendAfterSpace("OFFSET ");
        }

        public override void DuringTake()
        {
            AppendAfterSpace("LIMIT ");
        }

        public override void BeforeAlterColumnType(string previousColumnName)
        {
            Append(" ALTER COLUMN ");
            DuringSymbol(previousColumnName);
            Append(" TYPE ");
        }
    }
}