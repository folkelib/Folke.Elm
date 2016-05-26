using Folke.Elm.Abstract.Test;
using Xunit;

namespace Folke.Elm.PostgreSql.Test
{
    [Collection("Sqlite")]
    public class IntegrationTestSchemaUpdater : IIntegrationTestSchemaUpdater
    {
        private readonly BaseIntegrationTestSchemaUpdater test;

        public IntegrationTestSchemaUpdater()
        {
            test = new BaseIntegrationTestSchemaUpdater(new PostgreSqlDriver(), TestHelpers.ConnectionString, false);
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

        [Fact(Skip = "This type of change is not supported")]
        public void SchemaUpdater_ChangeColumnType()
        {
            test.SchemaUpdater_ChangeColumnType();
        }
    }
}
