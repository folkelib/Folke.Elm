using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Folke.Elm.Abstract.Test
{
    public interface IIntegrationTestSave : IDisposable
    {
        void Save();
        void InsertInto_ObjectWithGuid();
        Task SaveAsync_ObjectWithGuid();
    }
}
