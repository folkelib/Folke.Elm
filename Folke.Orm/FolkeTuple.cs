using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Orm
{
    public class FolkeTuple
    {
    }

    public class FolkeTuple<T0>
    {
        public T0 Item0 { get; set; }
    }

    public class FolkeTuple<T0, T1>
    {
        public T0 Item0 { get; set; }
        public T1 Item1 { get; set; }
    }
}
