using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Folke.Elm.Abstract.Test;
using Folke.Elm.Mysql.Test;
using Xunit;

namespace Folke.Elm.Sqlite.Test
{
    [Collection("Sqlite")]
    public class IntegrationTestLoad : IIntegrationTestLoad
    {
        private BaseIntegrationTestLoad test;

        public IntegrationTestLoad()
        {
            test = new BaseIntegrationTestLoad(new SqliteDriver(), TestHelpers.ConnectionString, false);
        }

        public void Dispose()
        {
            test.Dispose();
        }

        [Fact]
        public void Load_WithAutoJoin()
        {
            test.Load_WithAutoJoin();
        }

        [Fact]
        public void Load_ObjectWithCollection()
        {
            test.Load_ObjectWithCollection();
        }

        [Fact]
        public void Load_ObjectWithGuid()
        {
            test.Load_ObjectWithGuid();
        }

        [Fact]
        public void Load_ObjectWithGuid_WithAutoJoin()
        {
            test.Load_ObjectWithGuid_WithAutoJoin();
        }

        [Fact]
        public void Get_ObjectWithGuid()
        {
            test.Get_ObjectWithGuid();
        }

        [Fact]
        public async Task LoadAsync_ObjectWithGuid()
        {
            await test.LoadAsync_ObjectWithGuid();
        }

        [Fact]
        public async Task GetAsync_ObjectWithGuid()
        {
            await test.GetAsync_ObjectWithGuid();
        }
    }
}
