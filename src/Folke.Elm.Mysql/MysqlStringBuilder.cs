namespace Folke.Elm.Mysql
{
    internal class MysqlStringBuilder : SqlStringBuilder
    {
        public override void DuringSymbol(string symbol)
        {
            stringBuilder.Append('`');
            stringBuilder.Append(symbol);
            stringBuilder.Append('`');
        }

        public override void BeforeDropTable()
        {
            Append("SET FOREIGN_KEY_CHECKS = 0 ;");
            Append("DROP TABLE ");
        }
    }
}
