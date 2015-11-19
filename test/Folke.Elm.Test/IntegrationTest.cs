﻿using System.ComponentModel.DataAnnotations;
using Folke.Elm.Fluent;
using Folke.Elm.Mapping;
using Xunit;

namespace Folke.Elm.Test
{
    public class IntegrationTest
    {
        public class TestPoco
        {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Decimal { get; set; }
        }

        public class TestOtherPoco 
        {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Decimal { get; set; }
        }

        public class TestLinkTable
        {
            [Key]
            public int Id { get; set; }
            public TestPoco Group { get; set; }
            public TestOtherPoco User { get; set; }
        }
        
        [Fact(Skip = "Need fix")]
        public void FromSubQuery()
        {
            var query =  FluentBaseBuilder<TestLinkTable, FolkeTuple>.Select(null, new Mapper()).Values(x => x.Group).From(q => q.Values(x => x.Group).From().Where(x => x.User.Id == 1).GroupBy(x => x.Group));
            Assert.Equal("SELECT `t`.`Group_id` FROM (SELECT `t`.`Group_id` FROM `TestLinkTable` as t WHERE( `t`.`User_id`= @Item0) GROUP BY  `t`.`Group_id`) AS t", query.QueryBuilder.Sql);
        }

        [Fact(Skip = "Need fix")]
        public void InnerJoinSubQuery()
        {
            TestLinkTable a = null;
            var query = FluentBaseBuilder<TestLinkTable, FolkeTuple>.Select(null, new Mapper()).Values(x => x.Group).From(q => q.Values(x => x.Group).From().Where(x => x.User.Id == 1).GroupBy(x => x.Group))
               .InnerJoin(q => q.Values(x => x.Group).From().Where(x => x.User.Id == 2), () => a).On(x => a.Group == x.Group);
            Assert.Equal("SELECT `t`.`Group_id` FROM (SELECT `t`.`Group_id` FROM `TestLinkTable` as t WHERE( `t`.`User_id`= @Item0) GROUP BY  `t`.`Group_id`) AS t INNER JOIN (SELECT `t`.`Group_id` FROM `UserInGroup` as t WHERE( `t`.`User_id`= @Item1)) AS t1 ON ( `t1`.`Group_id`= `t`.`Group_id`)", query.QueryBuilder.Sql);
        }
    }
}