namespace Folke.Elm.Mapping
{
    /// <summary>
    /// A class used to define the mapping of a property to a database column
    /// </summary>
    public class FluentPropertyMapping<T>
    {
        private readonly PropertyMapping propertyMapping;

        public FluentPropertyMapping(PropertyMapping propertyMapping)
        {
            this.propertyMapping = propertyMapping;
        }

        /// <summary>
        /// Defines the column name
        /// </summary>
        /// <param name="name">The column name</param>
        /// <returns>The <see cref="FluentPropertyMapping{T}"/> itself</returns>
        public FluentPropertyMapping<T> HasColumnName(string name)
        {
            propertyMapping.ColumnName = name;
            return this;
        }

        public FluentPropertyMapping<T> AsJson()
        {
            propertyMapping.IsJson = true;
            return this;
        }
    }
}