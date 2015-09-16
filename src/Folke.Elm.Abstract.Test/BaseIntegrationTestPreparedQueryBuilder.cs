using Xunit;

namespace Folke.Elm.Abstract.Test
{
    public class BaseIntegrationTestPreparedQueryBuilder : BaseIntegrationTest, IIntegrationTestPreparedQueryBuilder
    {
        private readonly PreparedQueryBuilder<TestPoco, string> staticQuery = new PreparedQueryBuilder<TestPoco, string>(q => q.All().From().Where((x, y) => x.Name == y.Item0));

        public BaseIntegrationTestPreparedQueryBuilder(IDatabaseDriver databaseDriver, string connectionString, bool drop) : base(databaseDriver, connectionString, drop)
        {
        }

        public void PreparedQueryBuilder_AllFromWhereString_List()
        {
            var onePoco = new TestPoco { Name = "One" };
            connection.Save(onePoco);
            var twoPoco = new TestPoco { Name = "Two" };
            connection.Save(twoPoco);

            var query = new PreparedQueryBuilder<TestPoco, string>(q => q.All().From().Where((x, y) => x.Name == y.Item0));
            var result = query.List(connection, "Two");
            Assert.Equal(1, result.Count);
            Assert.Equal("Two", result[0].Name);
            result = query.List(connection, "One");
            Assert.Equal(1, result.Count);
            Assert.Equal("One", result[0].Name);
        }

        public void PreparedQueryBuilder_Static_AllFromWhereString_List()
        {
            var onePoco = new TestPoco { Name = "One" };
            connection.Save(onePoco);
            var twoPoco = new TestPoco { Name = "Two" };
            connection.Save(twoPoco);

            var result = staticQuery.List(connection, "Two");
            Assert.Equal(1, result.Count);
            Assert.Equal("Two", result[0].Name);
            result = staticQuery.List(connection, "One");
            Assert.Equal(1, result.Count);
            Assert.Equal("One", result[0].Name);
        }
    }
}
