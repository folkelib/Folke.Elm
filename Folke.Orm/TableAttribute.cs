using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Orm
{
    public class TableAttribute : Attribute
    {
        public ConstraintEventEnum OnDelete { get; set; }
        public ConstraintEventEnum OnUpdate { get; set; }
    }
}
