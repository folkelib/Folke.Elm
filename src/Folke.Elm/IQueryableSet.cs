using Folke.Elm.Mapping;

namespace Folke.Elm
{
    public interface IQueryableSet<T> : IQueryableSet
    {
    }

    public interface IQueryableSet
    {
        TypeMapping TypeMapping { get; }
    }
}
