namespace Folke.Orm.Mysql
{
    internal class MysqlStringBuilder : SqlStringBuilder
    {
        public override void AppendSymbol(string symbol)
        {
            stringBuilder.Append('`');
            stringBuilder.Append(symbol);
            stringBuilder.Append('`');
        }
    }
}
