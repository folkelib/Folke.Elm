using System;
using System.Threading.Tasks;
using Folke.Elm.Abstract.Test;
using Xunit;

namespace Folke.Elm.PostgreSql.Test
{
    public class IntegrationTestDelete : IIntegrationTestDelete
    {
        private readonly BaseIntegrationTestDelete test;

        public IntegrationTestDelete()
        {
            test = new BaseIntegrationTestDelete(new PostgreSqlDriver(), TestHelpers.ConnectionString, false);
        }

        [Fact]
        public Task DeleteAsync_ObjectWithGuid()
        {
            return test.DeleteAsync_ObjectWithGuid();
        }

        [Fact]
        public void Delete_ObjectWithGuid()
        {
            test.Delete_ObjectWithGuid();
        }

        public void Dispose()
        {
            test.Dispose();
        }
    }
}
