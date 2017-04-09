using System.Collections.Generic;
using System.Reflection;
using Folke.Elm.Mapping;

namespace Folke.Elm
{
    /// <summary>A table that is referenced somewhere in the expression</summary>
    public class SelectedTable
    {
        /// <summary>Gets or sets the mapping between the type and the table</summary>
        public TypeMapping Mapping { get; set; }

        /// <summary>Gets or sets the alias as used in the resulting SQL expression</summary>
        public string Alias { get; set; }

        /// <summary>Gets or sets the selected table whose this table is joined to</summary>
        public SelectedTable Parent { get; set; }

        public MemberInfo ParentMember { get; set; }

        /// <summary>In the case of a root table (<see cref="Parent"/> is null), the expression that points to the table</summary>
        public string Expression { get; set; }

        public Dictionary<MemberInfo, SelectedTable> Children { get; } = new Dictionary<MemberInfo, SelectedTable>();
    }
}