using System;
using Folke.Elm.Mapping;
using Xunit;
using Folke.Elm.Abstract.Test;

namespace Folke.Elm.Mysql.Test
{
    [Collection("IntegrationTest")]
    public class IntegrationTestSchemaUpdater : IIntegrationTestSchemaUpdater
    {
        private readonly BaseIntegrationTestSchemaUpdater test;

        public IntegrationTestSchemaUpdater()
        {
            test = new BaseIntegrationTestSchemaUpdater(new MySqlDriver(), TestHelpers.ConnectionString, true);
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

