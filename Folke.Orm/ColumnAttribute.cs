using System;

namespace Folke.Orm
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string Name { get; private set; }
        public string Index { get; set; }
        public int MaxLength { get; set; }
        /// <summary>
        /// What to do when the referenced line is deleted
        /// </summary>
        public ConstraintEventEnum OnDelete { get; set; }
        /// <summary>
        /// What to do when the referenced line key is updated
        /// </summary>
        public ConstraintEventEnum OnUpdate { get; set; }

        public ColumnAttribute()
        {
        }

        public ColumnAttribute(string name)
        {
            Name = name;
        }
    }
}
