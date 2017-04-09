using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Folke.Elm.Fluent;
using Folke.Elm.Mapping;
using Moq;
using Xunit;

namespace Folke.Elm.Test
{
    public class IntegrationTestComplexType
    {
        private readonly ISelectResult<TestComposed, FolkeTuple> select;

        [ComplexType]
        public class Composed
        {
            public string Text { get; set; }
            public int Number { get; set; }
        }

        public class TestComposed
        {
            [Key]
            public int Id { get; set; }

            public string Text { get; set; }
            public Composed Composed { get; set; }
        }

        public IntegrationTestComplexType()
        {
            var driverMock = new Mock<IDatabaseDriver>();
            select = FluentBaseBuilder<TestComposed, FolkeTuple>.Select(driverMock.Object, new Mapper());
        }


        [Fact(Skip = "Not implented")]
        public void SelectAllFrom()
        {
            var query = @select.All().From();
            Assert.Equal("SELECT \"t\".\"Id\", \"t\".\"Group\", \"t\".\"User\" FROM \"TestLinkTable\" AS t ORDER BY  \"t\".\"Id\" LIMIT 4,2", query.Sql);
        }
    }
}
