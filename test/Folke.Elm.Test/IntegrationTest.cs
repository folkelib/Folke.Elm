using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using Folke.Elm.Fluent;
using Folke.Elm.Mapping;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Folke.Elm.Test
{
    public class IntegrationTest
    {
        private readonly ISelectResult<TestLinkTable, FolkeTuple> select;
        private readonly Mock<IDatabaseDriver> driverMock;
        private Mapper mapper = new Mapper();

        [Table("TestPoco")]
        public class TestPoco
        {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Decimal { get; set; }
        }

        [Table("TestOtherPoco")]
        public class TestOtherPoco 
        {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Decimal { get; set; }
        }

        [Table("TestLinkTable")]
        public class TestLinkTable
        {
            [Key]
            public int Id { get; set; }
            public TestPoco Group { get; set; }
            public TestOtherPoco User { get; set; }
        }

        public IntegrationTest()
        {
            driverMock = new Mock<IDatabaseDriver>();
            select = FluentBaseBuilder<TestLinkTable, FolkeTuple>.Select(driverMock.Object, mapper);
        }
        
        [Fact(Skip = "Need fix")]
        public void FromSubQuery()
        {
            var query = select.Values(x => x.Group).From(q => q.Values(x => x.Group).From().Where(x => x.User.Id == 1).GroupBy(x => x.Group));
            Assert.Equal("SELECT `t`.`Group_id` FROM (SELECT `t`.`Group_id` FROM `TestLinkTable` as t WHERE( `t`.`User_id`= @Item0) GROUP BY  `t`.`Group_id`) AS t", query.QueryBuilder.Sql);
        }

        [Fact(Skip = "Need fix")]
        public void InnerJoinSubQuery()
        {
            TestLinkTable a = null;
            var query = select.Values(x => x.Group).From(q => q.Values(x => x.Group).From().Where(x => x.User.Id == 1).GroupBy(x => x.Group))
               .InnerJoin(q => q.Values(x => x.Group).From().Where(x => x.User.Id == 2), () => a).On(x => a.Group == x.Group);
            Assert.Equal("SELECT `t`.`Group_id` FROM (SELECT `t`.`Group_id` FROM `TestLinkTable` as t WHERE( `t`.`User_id`= @Item0) GROUP BY  `t`.`Group_id`) AS t INNER JOIN (SELECT `t`.`Group_id` FROM `UserInGroup` as t WHERE( `t`.`User_id`= @Item1)) AS t1 ON ( `t1`.`Group_id`= `t`.`Group_id`)", query.QueryBuilder.Sql);
        }

        [Fact]
        public void SkipAndTake()
        {
            var query = select.All().From().OrderBy(x => x.Id).Skip(4).Take(2);
            Assert.Equal("SELECT \"t\".\"Id\", \"t\".\"Group_id\", \"t\".\"User_id\" FROM \"TestLinkTable\" AS t ORDER BY  \"t\".\"Id\" LIMIT 4,2", query.Sql);
        }

        [Fact]
        public void LeftJoin_ParameterIsNotAValidExpression()
        {
            TestPoco poco = null;
            Assert.Throws<Exception>(() => @select.All().From().LeftJoin(x => poco).On(x => x.Id.Equals(poco.Id)));
        }

        [Fact(Skip = "Not implemented")]
        public void LeftJoin_OnItemsSubWithId()
        {
            var selectTuple = FluentBaseBuilder<FolkeTuple<TestLinkTable, TestPoco>, FolkeTuple>.Select(driverMock.Object, mapper);
            selectTuple.All(x => x.Item0).From(x => x.Item0).LeftJoin(x => x.Item1).On(x => x.Item0.User.Id.Equals(x.Item1.Id));
        }

        [Fact]
        public void LeftJoin_OnItemsWithReferenceEqualOperator()
        {
            var selectTuple = FluentBaseBuilder<FolkeTuple<TestLinkTable, TestOtherPoco>, FolkeTuple>.Select(driverMock.Object, mapper);
            selectTuple.All(x => x.Item0).From(x => x.Item0).LeftJoin(x => x.Item1).On(x => x.Item0.User == x.Item1);
        }

        [Fact(Skip = "Not implemented")]
        public void LeftJoin_OnItemsWithReferenceEqualsMethod()
        {
            var selectTuple = FluentBaseBuilder<FolkeTuple<TestLinkTable, TestOtherPoco>, FolkeTuple>.Select(driverMock.Object, mapper);
            selectTuple.All(x => x.Item0).From(x => x.Item0).LeftJoin(x => x.Item1).On(x => x.Item0.User.Equals(x.Item1));
        }

        [Fact]
        public void AttemptToSelectASameColumnMultipleTime_ThrowsAnException()
        {
            Assert.Throws<ElmException>(() => @select.Value(x => x.Group).Value(x => x.Group.Name).From());
        }
    }
}
