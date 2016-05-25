using Xunit;

namespace Folke.Elm.Sqlite.Test
{
    [Collection("Sqlite")]
    public class IntegrationTestSchemaUpdater : IIntegrationTestSchemaUpdater
    {
        private readonly BaseIntegrationTestSchemaUpdater test;

        public IntegrationTestSchemaUpdater()
        {
            test = new BaseIntegrationTestSchemaUpdater(new SqliteDriver(), TestHelpers.ConnectionString, false);
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

        [Fact(Skip = "CHANGE Column type not supported")]
        public void SchemaUpdater_ChangeColumnType()
        {
            test.SchemaUpdater_ChangeColumnType();
        }
    }
}
