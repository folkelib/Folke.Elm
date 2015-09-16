namespace Folke.Elm.Mysql
{
    internal class MysqlStringBuilder : SqlStringBuilder
    {
        public override void AppendSymbol(string symbol)
        {
            stringBuilder.Append('`');
            stringBuilder.Append(symbol);
            stringBuilder.Append('`');
        }

        public override void AppendDropTable(string tableName)
        {
            Append("SET FOREIGN_KEY_CHECKS = 0 ;");
            Append("DROP TABLE ");
            AppendSymbol(tableName);
        }
    }
}
