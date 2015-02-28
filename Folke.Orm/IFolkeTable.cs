namespace Folke.Orm
{
    public interface IFolkeTable : IFolkeTable<int>
    {
    }

    public interface IFolkeTable<T>
    {
        T Id { get; set; }
    }
}
