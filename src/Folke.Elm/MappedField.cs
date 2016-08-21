using System.Reflection;

namespace Folke.Elm
{
    /// <summary>A field that has been selected and mapped from a class</summary>
    public class MappedField
    {
        /// <summary>Gets or sets the matching select field (allow to retrieve the data from the reader by its index)</summary>
        public SelectedField SelectedField;

        /// <summary>Gets or sets the property info that will allow to write the result in the class</summary>
        public PropertyInfo PropertyInfo;

        /// <summary>Gets or sets the mapped class if this is a property that holds an object (or null if it is not)</summary>
        public MappedClass MappedClass;
    }
}