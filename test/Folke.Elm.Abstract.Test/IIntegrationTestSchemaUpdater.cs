using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Folke.Elm.Abstract.Test
{
    public interface IIntegrationTestSchemaUpdater : IDisposable
    {
        void SchemaUpdater_AddColumn();
        void SchemaUpdater_ChangeColumnType();
    }
}
