using System.Threading.Tasks;
using Folke.Elm.Abstract.Test;
using Folke.Elm.Mysql.Test;
using Xunit;

namespace Folke.Elm.Sqlite.Test
{
    public class IntegrationTestDelete : IIntegrationTestDelete
    {
        private readonly BaseIntegrationTestDelete test;

        public IntegrationTestDelete()
        {
            test = new BaseIntegrationTestDelete(new SqliteDriver(), TestHelpers.ConnectionString, false);
        }

        public void Dispose()
        {
            test.Dispose();
        }

        [Fact]
        public void Delete_ObjectWithGuid()
        {
            test.Delete_ObjectWithGuid();
        }

        [Fact]
        public async Task DeleteAsync_ObjectWithGuid()
        {
            await test.DeleteAsync_ObjectWithGuid();
        }
    }
}
