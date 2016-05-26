using Folke.Elm.Abstract.Test;
using System.Threading.Tasks;
using Xunit;

namespace Folke.Elm.PostgreSql.Test
{
    public class IntegrationTestLoad : IIntegrationTestLoad
    {
        private readonly BaseIntegrationTestLoad test;

        public IntegrationTestLoad()
        {
            test = new BaseIntegrationTestLoad(new PostgreSqlDriver(), TestHelpers.ConnectionString, false);
        }

        public void Dispose()
        {
            test.Dispose();
        }

        [Fact]
        public Task GetAsync_ObjectWithGuid()
        {
            return test.GetAsync_ObjectWithGuid();
        }

        [Fact]
        public void Get_ObjectWithGuid()
        {
            test.Get_ObjectWithGuid();
        }

        [Fact]
        public Task LoadAsync_ObjectWithGuid()
        {
            return test.LoadAsync_ObjectWithGuid();
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
        public void Load_WithAutoJoin()
        {
            test.Load_WithAutoJoin();
        }
    }
}
