using Folke.Elm.Mapping;

namespace Folke.Elm
{
    /// <summary>A selected field</summary>
    public class SelectedField
    {
        /// <summary>Gets or sets the property mapping</summary>
        public PropertyMapping PropertyMapping { get; set; }

        /// <summary>Gets or sets the table whose column it is</summary>
        public SelectedTable Table { get; set; }
        
        /// <summary>Gets or sets the index in the results in the sql data reader</summary>
        public int Index { get; set; }
    }
}