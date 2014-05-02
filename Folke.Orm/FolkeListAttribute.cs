using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Orm
{
    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple=true)]
    public class FolkeListAttribute : Attribute
    {
        public string Join { get; set; }
    }
}
