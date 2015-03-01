namespace Folke.Orm
{
    public class QueryBuilder<T> : FluentGenericQueryBuilder<T, FolkeTuple>
        where T : class, new()
    {
        public QueryBuilder(FolkeConnection connection)
            : base(connection)
        {
        }
    }
}