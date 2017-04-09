using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Folke.Elm.Fluent;
using Folke.Elm.InformationSchema;
using Folke.Elm.Mapping;
using Moq;
using Xunit;

namespace Folke.Elm.Test
{
    public class IntegrationTestComplexType
    {
        private readonly ISelectResult<TestComposed, FolkeTuple> select;
        private readonly Mock<IDatabaseDriver> driverMock;

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
            driverMock = new Mock<IDatabaseDriver>();
            driverMock.Setup(x => x.GetSqlType(It.IsAny<PropertyMapping>(), It.IsAny<bool>())).Returns("TEST");
            driverMock.Setup(x => x.CreateSqlStringBuilder()).Returns(new SqlStringBuilder());
            select = FluentBaseBuilder<TestComposed, FolkeTuple>.Select(driverMock.Object, new Mapper());
        }
        
        [Fact]
        public void SelectAllFrom()
        {
            var query = @select.All().From();
            Assert.Equal("SELECT \"t\".\"Id\", \"t\".\"Text\", \"t\".\"Composed_id_Text\", \"t\".\"Composed_id_Number\" FROM \"TestComposed\" AS t", query.Sql);
        }

        [Fact]
        public void SelectAllFromWhere()
        {
            var query = @select.All().From().Where(x => x.Composed.Text == "Test");
            Assert.Equal("SELECT \"t\".\"Id\", \"t\".\"Text\", \"t\".\"Composed_id_Text\", \"t\".\"Composed_id_Number\" FROM \"TestComposed\" AS t WHERE( \"t\".\"Composed_id_Text\"= @Item0)", query.Sql);
        }

        [Fact]
        public void Create()
        {
            var schemaQueryBuilder = new SchemaQueryBuilder<TestComposed>(new Mapper(), driverMock.Object);
            schemaQueryBuilder.CreateTable();
            Assert.Equal("CREATE TABLE \"TestComposed\" ( \"Id\" TEST PRIMARY KEY AUTO_INCREMENT,\"Text\" TEST,\"Composed_id_Text\" TEST,\"Composed_id_Number\" TEST)", schemaQueryBuilder.Sql);
        }
        
        [Fact]
        public void Update()
        {
            var schemaQueryBuilder = new SchemaQueryBuilder<TestComposed>(new Mapper(), driverMock.Object);
            var existingDefinitions = new List<IColumnDefinition>();
            schemaQueryBuilder.AlterTable().AlterColumns(existingDefinitions);
            Assert.Equal("ALTER TABLE \"TestComposed\" ADD COLUMN \"Id\" TEST PRIMARY KEY AUTO_INCREMENT; ALTER TABLE \"TestComposed\" ADD COLUMN \"Text\" TEST; ALTER TABLE \"Composed\" ADD COLUMN \"Composed_id_Text\" TEST; ALTER TABLE \"Composed\" ADD COLUMN \"Composed_id_Number\" TEST", schemaQueryBuilder.Sql);
        }
    }
}
