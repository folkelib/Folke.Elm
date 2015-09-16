namespace Folke.Elm
{
    public interface IManyToManyTable<TParent, TChild> : IFolkeTable
    {
        TParent Parent { get; set;  }
        TChild Child { get; set; }
    }
}
