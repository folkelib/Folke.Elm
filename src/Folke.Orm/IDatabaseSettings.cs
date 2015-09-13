using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Orm
{
    public interface IDatabaseSettings
    {
        string Host { get; }
        string Database { get; }
        string User { get; }
        string Password { get; }
    }
}
