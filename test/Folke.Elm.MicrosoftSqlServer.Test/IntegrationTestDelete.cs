using System.Threading.Tasks;
using Folke.Elm.Abstract.Test;
using Xunit;

namespace Folke.Elm.MicrosoftSqlServer.Test
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class IntegrationTestDelete : IIntegrationTestDelete
    {
        private readonly BaseIntegrationTestDelete test;

        public IntegrationTestDelete()
        {
            test = new BaseIntegrationTestDelete(new MicrosoftSqlServerDriver(), TestHelpers.ConnectionString, false);
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
