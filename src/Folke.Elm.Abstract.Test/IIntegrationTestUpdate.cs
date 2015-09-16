using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Folke.Elm.Abstract.Test
{
    public interface IIntegrationTestUpdate : IDisposable
    {
        void Update_Set();
        void Update_ObjectWithGuid();
        Task UpdateAsync_ObjectWithGuid();
    }
}
