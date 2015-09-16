using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Folke.Elm.Abstract.Test
{
    public interface IIntegrationTestDelete : IDisposable
    {
        void Delete_ObjectWithGuid();
        Task DeleteAsync_ObjectWithGuid();
    }
}
