namespace Folke.Elm.Fluent
{
    public interface IAscTarget<T, TMe> : IFluentBuilder
    {
    }

    public static class AscTargetExtensions
    {
        public static IAscResult<T, TMe> Desc<T, TMe>(this IAscTarget<T, TMe> ascTarget)
        {
            ascTarget.QueryBuilder.Append("DESC");
            return (IAscResult<T, TMe>)ascTarget;
        }

        public static IAscResult<T, TMe> Asc<T, TMe>(this IAscTarget<T, TMe> ascTarget)
        {
            ascTarget.QueryBuilder.Append("ASC");
            return (IAscResult<T, TMe>)ascTarget;
        }
    }

    public interface IAscResult<T, TMe> : IFluentBuilder, IOrderByTarget<T, TMe>, IQueryableCommand<T>, ILimitTarget<T, TMe>
    {
    }
}
