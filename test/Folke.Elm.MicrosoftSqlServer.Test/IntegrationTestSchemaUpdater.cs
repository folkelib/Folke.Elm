using Folke.Elm.Abstract.Test;
using Xunit;

namespace Folke.Elm.MicrosoftSqlServer.Test
{
    [Collection("Sqlite")]
    public class IntegrationTestSchemaUpdater : IIntegrationTestSchemaUpdater
    {
        private readonly BaseIntegrationTestSchemaUpdater test;

        public IntegrationTestSchemaUpdater()
        {
            test = new BaseIntegrationTestSchemaUpdater(new MicrosoftSqlServerDriver(), TestHelpers.ConnectionString, false);
        }

        public void Dispose()
        {
            test.Dispose();
        }

        [Fact]
        public void SchemaUpdater_AddColumn()
        {
            test.SchemaUpdater_AddColumn();
        }

        [Fact]
        public void SchemaUpdater_ChangeColumnType()
        {
            test.SchemaUpdater_ChangeColumnType();
        }
    }
}
