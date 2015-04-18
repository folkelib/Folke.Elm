using System;

namespace Folke.Orm.Mapping
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnConstraintAttribute : Attribute
    {
        /// <summary>
        /// What to do when the referenced line is deleted
        /// </summary>
        public ConstraintEventEnum OnDelete { get; set; }
        /// <summary>
        /// What to do when the referenced line key is updated
        /// </summary>
        public ConstraintEventEnum OnUpdate { get; set; }
    }
}
