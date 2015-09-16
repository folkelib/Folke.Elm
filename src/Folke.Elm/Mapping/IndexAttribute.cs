using System;

namespace Folke.Elm.Mapping
{
    /// <summary>
    /// Create an index on this column
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IndexAttribute : Attribute
    {
        /// <summary>
        /// The index name (default to the table and column name)
        /// </summary>
        public string Name { get; set; }
    }
}
