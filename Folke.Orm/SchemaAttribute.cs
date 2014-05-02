using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Orm
{
    [System.AttributeUsage(AttributeTargets.Class)]
    public class SchemaAttribute : Attribute
    {
        public string Name { get; set; }

        public SchemaAttribute(string name)
        {
            Name = name;
        }
    }
}
