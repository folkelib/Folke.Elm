using Folke.Elm.Abstract.Test;
using System.Threading.Tasks;
using Xunit;

namespace Folke.Elm.MicrosoftSqlServer.Test
{
    [Collection("Sqlite")]
    public class IntegrationTestSave : IIntegrationTestSave
    {
        private BaseIntegrationTestSave test;

        public IntegrationTestSave()
        {
            test = new BaseIntegrationTestSave(new MicrosoftSqlServerDriver(), TestHelpers.ConnectionString, false);
        }

        public void Dispose()
        {
            test.Dispose();
        }

        [Fact]
        public void Save()
        {
            test.Save();
        }

        [Fact]
        public void InsertInto_ObjectWithGuid()
        {
            test.InsertInto_ObjectWithGuid();
        }

        [Fact]
        public async Task SaveAsync_ObjectWithGuid()
        {
            await test.SaveAsync_ObjectWithGuid();
        }
    }
}
