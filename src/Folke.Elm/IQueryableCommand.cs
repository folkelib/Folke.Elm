using System.Collections.Generic;

namespace Folke.Elm
{
    public interface IQueryableCommand<out T> : IQueryableCommand
    {
        MappedClass MappedClass { get; }
        IEnumerator<T> GetEnumerator();
    }

    public interface IQueryableCommand : IBaseCommand
    {
        
    }
    
    public interface IBaseCommand
    {
        IFolkeConnection Connection { get; }
        string Sql { get; }
        object[] Parameters { get; }
    }
}
