using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Folke.Elm.Abstract.Test
{
    public interface IIntegrationTestPreparedQueryBuilder : IDisposable
    {
        void PreparedQueryBuilder_AllFromWhereString_List();
        void PreparedQueryBuilder_Static_AllFromWhereString_List();
    }
}
