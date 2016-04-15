namespace Folke.Elm.Fluent
{
    public interface IFluentBuilder
    {
        BaseQueryBuilder QueryBuilder { get; }

        QueryContext CurrentContext { get; set; }
    }
}
