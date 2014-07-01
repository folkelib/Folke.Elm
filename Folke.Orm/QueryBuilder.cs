namespace Folke.Orm
{
    public class QueryBuilder<T> : BaseQueryBuilder<T, FolkeTuple>
        where T : class, new()
    {
        public QueryBuilder(FolkeConnection connection)
            : base(connection)
        {
        }
    }
}