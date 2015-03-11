namespace Folke.Orm.Fluent
{
    public class FluentDeleteBuilder<T, TMe>
    {
        private readonly BaseQueryBuilder baseQueryBuilder;

        public FluentDeleteBuilder(BaseQueryBuilder baseQueryBuilder)
        {
            this.baseQueryBuilder = baseQueryBuilder;
            baseQueryBuilder.AppendDelete();
        }

        /// <summary>Chose the bean table as the table to select from</summary>
        /// <returns> The <see cref="FluentFromBuilder{T,TMe,TU}"/>. </returns>
        public FluentFromBuilder<T, TMe, T> From()
        {
            return new FluentFromBuilder<T, TMe, T>(baseQueryBuilder);
        }
    }
}
