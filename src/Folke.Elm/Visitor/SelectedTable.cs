using System.Collections.Generic;
using Folke.Elm.Mapping;

namespace Folke.Elm.Visitor
{
    /// <summary>A table that is referenced somewhere in the expression</summary>
    public class SelectedTable : IVisitable
    {
        /// <summary>Gets or sets the mapping between the type and the table</summary>
        public TypeMapping Mapping { get; set; }

        /// <summary>Gets or sets the alias as used in the resulting SQL expression</summary>
        public string Alias { get; set; }

        /// <summary>Gets or sets the selected table whose this table is joined to</summary>
        public SelectedTable Parent { get; set; }

        public PropertyMapping ParentMember { get; set; }
        
        public Dictionary<PropertyMapping, SelectedTable> Children { get; } = new Dictionary<PropertyMapping, SelectedTable>();
        public void Accept(IVisitor visitor)
        {
            throw new System.NotImplementedException();
        }
    }
}