using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Orm
{
    [System.AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string Name { get; private set; }
        public string Index { get; set; }
        public int MaxLength { get; set; }
        public ConstraintEventEnum OnDelete { get; set; }
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
