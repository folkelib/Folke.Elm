using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Orm.InformationSchema
{
    [Schema("INFORMATION_SCHEMA")]
    internal class Tables
    {
        public string TABLE_NAME { get; set; }
        public string TABLE_SCHEMA { get; set; }
    }
}
