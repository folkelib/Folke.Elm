using Folke.Elm.Abstract.Test;
using Xunit;

namespace Folke.Elm.PostgreSql.Test
{
    [Collection("Sqlite")]
    public class IntegrationTestPreparedQueryBuilder : IIntegrationTestPreparedQueryBuilder
    {
        private readonly BaseIntegrationTestPreparedQueryBuilder test;

        public IntegrationTestPreparedQueryBuilder()
        {
            test = new BaseIntegrationTestPreparedQueryBuilder(new PostgreSqlDriver(), TestHelpers.ConnectionString, false);
        }

        public void Dispose()
        {
            test.Dispose();
        }

        [Fact]
        public void PreparedQueryBuilder_AllFromWhereString_List()
        {
            test.PreparedQueryBuilder_AllFromWhereString_List();
        }

        [Fact]
        public void PreparedQueryBuilder_Static_AllFromWhereString_List()
        {
            test.PreparedQueryBuilder_Static_AllFromWhereString_List();
        }
    }
}
