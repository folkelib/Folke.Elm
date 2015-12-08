using Folke.Elm.Abstract.Test;
using System.Threading.Tasks;
using Xunit;

namespace Folke.Elm.MicrosoftSqlServer.Test
{
    [Collection("Sqlite")]
    public class IntegrationTestUpdate : IIntegrationTestUpdate
    {
        private readonly BaseIntegrationTestUpdate test;

        public IntegrationTestUpdate()
        {
            test = new BaseIntegrationTestUpdate(new MicrosoftSqlServerDriver(), TestHelpers.ConnectionString, false);
        }

        public void Dispose()
        {
            test.Dispose();
        }

        [Fact]
        public void Update_Set()
        {
            test.Update_Set();
        }

        [Fact]
        public void Update_ObjectWithGuid()
        {
            test.Update_ObjectWithGuid();
        }

        [Fact]
        public async Task UpdateAsync_ObjectWithGuid()
        {
            await test.UpdateAsync_ObjectWithGuid();
        }
    }
}
