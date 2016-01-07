using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using Folke.Elm.Fluent;
using Folke.Elm.Mapping;
using Moq;
using Xunit;
using System.Linq;

namespace Folke.Elm.Test
{
    public class TestElmQueryable
    {
        private readonly ElmQueryable<TestPoco> queryable;
        private readonly Mock<IFolkeCommand> commandMock;

        public class TestPoco
        {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Decimal { get; set; }
        }

        public TestElmQueryable()
        {
            var driverMock = new Mock<IDatabaseDriver>();
            var mapper = new Mapper();
            var connection = new Mock<IFolkeConnection>(); //  FolkeConnection.Create(driverMock.Object, mapper);
            connection.Setup(x => x.Mapper).Returns(mapper);
            connection.Setup(x => x.Driver).Returns(driverMock.Object);
            commandMock = new Mock<IFolkeCommand>();
            var dbReaderMock = new Mock<DbDataReader>();
            commandMock.Setup(x => x.ExecuteReader()).Returns(dbReaderMock.Object);
            commandMock.SetupProperty(x => x.CommandText);
            connection.Setup(x => x.CreateCommand(It.IsAny<string>(), It.IsAny<object[]>())).Returns(
                (string text, object[] parameters) =>
                {
                    commandMock.Object.CommandText = text;
                    return commandMock.Object;
                });
            var provider = new ElmQueryProvider(connection.Object);
            queryable = new ElmQueryable<TestPoco>(null, provider);
        }

        [Fact]
        public void Where()
        {
            // Arrange
            
            // Act
            List<TestPoco> result = queryable.Where(x => x.Name == "Toto").ToList();

            // Assert
            Assert.Empty(result);
            Assert.Equal("SELECT  \"t\".\"Id\", \"t\".\"Name\", \"t\".\"Decimal\" FROM \"TestPoco\" as t WHERE( \"t\".\"Name\"= @Item0)", commandMock.Object.CommandText);
        }

        [Fact]
        public void ToList()
        {
            // Arrange

            // Act
            List<TestPoco> result = queryable.ToList();

            // Assert
            Assert.Empty(result);
            Assert.Equal("SELECT  \"t\".\"Id\", \"t\".\"Name\", \"t\".\"Decimal\" FROM \"TestPoco\" as t", commandMock.Object.CommandText);
        }

        [Fact]
        public void SkipAndTake()
        {
            // Arrange

            // Act
            List<TestPoco> result = queryable.OrderBy(x => x.Name).Skip(10).Take(15).ToList();

            // Assert
            Assert.Empty(result);
            Assert.Equal("SELECT  \"t\".\"Id\", \"t\".\"Name\", \"t\".\"Decimal\" FROM \"TestPoco\" as t ORDER BY  \"t\".\"Name\" LIMIT 10,15", commandMock.Object.CommandText);
        }
    }
}
