using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Folke.Elm.Abstract.Test
{
    public interface IIntegrationTestLoad : IDisposable
    {
        void Load_WithAutoJoin();
        void Load_ObjectWithCollection();
        void Load_ObjectWithGuid();
        void Load_ObjectWithGuid_WithAutoJoin();
        void Get_ObjectWithGuid();
        Task LoadAsync_ObjectWithGuid();
        Task GetAsync_ObjectWithGuid();
    }
}
