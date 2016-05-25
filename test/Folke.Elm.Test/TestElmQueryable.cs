using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
//using Folke.Elm.Fluent;
//using Folke.Elm.Mapping;
//using Moq;
using Xunit;
using System.Linq;

namespace Folke.Elm.Test
{
    public class TestElmQueryable
    {
        //private readonly ElmQueryable<TestPoco> queryable;
        //private readonly Mock<IFolkeCommand> commandMock;
        //private ElmQueryProvider provider;

        public class TestPoco
        {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Decimal { get; set; }
        }

        public class TestOther
        {
            [Key]
            public int Id { get; set; }
            public TestPoco TestPoco { get; set; }
            public string Value { get; set; }
        }

        public TestElmQueryable()
        {
            //var driverMock = new Mock<IDatabaseDriver>();
            //var mapper = new Mapper();
            //var connection = new Mock<IFolkeConnection>(); //  FolkeConnection.Create(driverMock.Object, mapper);
            //connection.Setup(x => x.Mapper).Returns(mapper);
            //connection.Setup(x => x.Driver).Returns(driverMock.Object);
            //commandMock = new Mock<IFolkeCommand>();
            //var dbReaderMock = new Mock<DbDataReader>();
            //commandMock.Setup(x => x.ExecuteReader()).Returns(dbReaderMock.Object);
            //commandMock.SetupProperty(x => x.CommandText);
            //connection.Setup(x => x.CreateCommand(It.IsAny<string>(), It.IsAny<object[]>())).Returns(
            //    (string text, object[] parameters) =>
            //    {
            //        commandMock.Object.CommandText = text;
            //        return commandMock.Object;
            //    });
            //provider = new ElmQueryProvider(connection.Object);
            //queryable = new ElmQueryable<TestPoco>(null, provider);
        }

        [Fact]
        public void Where()
        {
            // Arrange
            
            //// Act
            //List<TestPoco> result = queryable.Where(x => x.Name == "Toto").ToList();

            //// Assert
            //Assert.Empty(result);
            //Assert.Equal("SELECT \"t\".\"Id\", \"t\".\"Name\", \"t\".\"Decimal\" FROM \"TestPoco\" AS t WHERE( \"t\".\"Name\"= @Item0)", commandMock.Object.CommandText);
        }

        [Fact]
        public void ToList()
        {
            // Arrange

            // Act
            //List<TestPoco> result = queryable.ToList();

            //// Assert
            //Assert.Empty(result);
            //Assert.Equal("SELECT \"t\".\"Id\", \"t\".\"Name\", \"t\".\"Decimal\" FROM \"TestPoco\" AS t", commandMock.Object.CommandText);
        }

        [Fact]
        public void SkipAndTake()
        {
            // Arrange

            //// Act
            //List<TestPoco> result = queryable.OrderBy(x => x.Name).Skip(10).Take(15).ToList();

            //// Assert
            //Assert.Empty(result);
            //Assert.Equal("SELECT \"t\".\"Id\", \"t\".\"Name\", \"t\".\"Decimal\" FROM \"TestPoco\" AS t ORDER BY  \"t\".\"Name\" LIMIT @Item0, @Item1", commandMock.Object.CommandText);
        }

        [Fact]
        public void Join()
        {
            //// Arrange
            //var otherQueryable = new ElmQueryable<TestOther>(provider);

            //// Act
            //List<TestPoco> result = (from testPoco in queryable
            //              join other in otherQueryable on testPoco.Id equals other.TestPoco.Id
            //        select testPoco).ToList();

            //// Assert
            //Assert.Empty(result);
            //Assert.Equal("", commandMock.Object.CommandText);
        }
    }
}
