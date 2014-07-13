using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Orm
{
    public interface IManyToManyTable<TParent, TChild> : IFolkeTable
    {
        TParent Parent { get; set;  }
        TChild Child { get; set; }
    }
}
