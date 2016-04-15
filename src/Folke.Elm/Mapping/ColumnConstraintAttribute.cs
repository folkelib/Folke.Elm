using System;

namespace Folke.Elm.Mapping
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnConstraintAttribute : Attribute
    {
        /// <summary>
        /// What to do when the referenced row is deleted. Cascade means that the row with this constraint is deleted too.
        /// </summary>
        public ConstraintEventEnum OnDelete { get; set; }

        /// <summary>
        /// What to do when the referenced row key is updated. Cascade means that this property is updated too.
        /// </summary>
        public ConstraintEventEnum OnUpdate { get; set; }
    }
}
