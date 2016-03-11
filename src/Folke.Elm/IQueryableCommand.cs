using System.Collections.Generic;
using System.Linq;

namespace Folke.Elm
{
    /// <summary>
    /// A command that returns values mapped to a type
    /// </summary>
    /// <typeparam name="T">The return type</typeparam>
    public interface IQueryableCommand<out T> : IQueryableCommand
    {
        MappedClass MappedClass { get; }

        IEnumerator<T> GetEnumerator();
    }

    /// <summary>
    /// A command that returns values
    /// </summary>
    public interface IQueryableCommand : IBaseCommand
    {
    }
}
