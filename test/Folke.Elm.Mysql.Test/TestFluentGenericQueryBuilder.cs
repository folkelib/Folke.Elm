using System.Linq;
using Folke.Elm.Fluent;
using Folke.Elm.Mapping;
using Xunit;

namespace Folke.Elm.Mysql.Test
{
    [Collection("IntegrationTest")]
    public class TestFluentGenericQueryBuilder
    {
        private readonly MySqlDriver mySqlDriver;
        private readonly ISelectResult<FakeClass, FolkeTuple> fluentSelectBuilder;
        private readonly BaseQueryBuilder queryBuilder;

        public TestFluentGenericQueryBuilder()
        {
            mySqlDriver = new MySqlDriver();
            var mapper = new Mapper();
            fluentSelectBuilder = FluentBaseBuilder<FakeClass, FolkeTuple>.Select(mySqlDriver, mapper);
            queryBuilder = fluentSelectBuilder.QueryBuilder;
        }

        [Fact]
        public void FluentGenericQueryBuilder_Select_EqualOperator()
        {
            fluentSelectBuilder.Values(x => x.Id == 3);
            Assert.Equal("SELECT( `t`.`Id`= @Item0)", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_Select_EqualsMethod()
        {
            fluentSelectBuilder.Values(x => x.Id.Equals(3));
            Assert.Equal("SELECT( `t`.`Id`= @Item0)", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_Select_PropertyObjectExtension()
        {
            var propertyInfo = typeof (FakeClass).GetProperty("Id");
            fluentSelectBuilder.Values(x => x.Property(propertyInfo).Equals(3));
            Assert.Equal("SELECT( `t0`.`Id`= @Item0)", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_Select_LikeExtension()
        {
            fluentSelectBuilder.Values(x => x.Text.Like("toto"));
            Assert.Equal("SELECT( `t`.`Text` LIKE @Item0)", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_Select_StringStartsWith()
        {
            fluentSelectBuilder.Values(x => x.Text.StartsWith("toto"));
            Assert.Equal("SELECT( `t`.`Text` LIKE @Item0)", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_Select_StringContains()
        {
            fluentSelectBuilder.Values(x => x.Text.Contains("toto"));
            Assert.Equal("SELECT( `t`.`Text` LIKE @Item0)", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_Select_ListOfExpressionsFromDefaultTable()
        {
            fluentSelectBuilder.Values(x => x.Id, x => x.Text);
            Assert.Equal("SELECT `t`.`Id`, `t`.`Text`", queryBuilder.Sql);
            Assert.Equal(2, queryBuilder.SelectedFields.Count);
            Assert.True(queryBuilder.SelectedFields.Any(x => x.PropertyMapping.PropertyInfo == typeof(FakeClass).GetProperty("Id")));
            Assert.True(queryBuilder.SelectedFields.Any(x => x.PropertyMapping.PropertyInfo == typeof(FakeClass).GetProperty("Text")));
        }

        [Fact]
        public void FluentGenericQueryBuilder_Select_ListOfExpressionsFromDefaultTableAndJoin()
        {
            fluentSelectBuilder.Values(x => x.Id, x => x.Text, x => x.Child.Value);
            Assert.Equal("SELECT `t`.`Id`, `t`.`Text`, `t1`.`Value`", queryBuilder.Sql);
            Assert.Equal(3, queryBuilder.SelectedFields.Count);
            Assert.True(queryBuilder.SelectedFields.Any(x => x.PropertyMapping.PropertyInfo == typeof(FakeClass).GetProperty("Id")));
            Assert.True(queryBuilder.SelectedFields.Any(x => x.PropertyMapping.PropertyInfo == typeof(FakeClass).GetProperty("Text")));
            Assert.True(queryBuilder.SelectedFields.Any(x => x.PropertyMapping.PropertyInfo == typeof(FakeChildClass).GetProperty("Value")));
        }

        [Fact]
        public void FluentGenericQueryBuilder_Select_Max()
        {
            fluentSelectBuilder.Values(x => SqlFunctions.Max(x.Id));
            Assert.Equal("SELECT MAX( `t`.`Id`)", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_Select_Max2()
        {
            fluentSelectBuilder.Max(x => x.Id);
            Assert.Equal("SELECT MAX( `t`.`Id`)", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_Select_Sum()
        {
            fluentSelectBuilder.Values(x => SqlFunctions.Sum(x.Id));
            Assert.Equal("SELECT SUM( `t`.`Id`)", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_Select_Sum2()
        {
            fluentSelectBuilder.Sum(x => x.Id);
            Assert.Equal("SELECT SUM( `t`.`Id`)", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_Select_Count()
        {
            fluentSelectBuilder.Count(x => x.Id);
            Assert.Equal("SELECT COUNT( `t`.`Id`)", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_WhereSubAfterWhere()
        {
            fluentSelectBuilder.All()
                .From()
                .Where(x => x.Text == "fake")
                .WhereSub(select => select.Or(x => x.Text == "test").Or(x => x.Text == "other"));
            Assert.Equal("SELECT `t`.`Id`, `t`.`Text`, `t`.`Value`, `t`.`Child_id` FROM `FakeClass` AS t WHERE( `t`.`Text`= @Item0) AND (( `t`.`Text`= @Item1) OR ( `t`.`Text`= @Item2))", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_WhereSub()
        {
            fluentSelectBuilder.All()
                .From()
                .WhereSub(select => select.Or(x => x.Text == "test").Or(x => x.Text == "other"));
            Assert.Equal("SELECT `t`.`Id`, `t`.`Text`, `t`.`Value`, `t`.`Child_id` FROM `FakeClass` AS t WHERE(( `t`.`Text`= @Item0) OR ( `t`.`Text`= @Item1))", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_OrderByExpression()
        {
            fluentSelectBuilder.All().From()
                .OrderBy(x => x.Text + x.Text);
            Assert.Equal("SELECT `t`.`Id`, `t`.`Text`, `t`.`Value`, `t`.`Child_id` FROM `FakeClass` AS t ORDER BY ( `t`.`Text`+ `t`.`Text`)", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_LocalVariableIsTable()
        {
            FakeChildClass child = null;
            fluentSelectBuilder.All().All(x => child).From().LeftJoin(x => child).On(x => x.Child == child);
            Assert.Equal("SELECT `t`.`Id`, `t`.`Text`, `t`.`Value`, `t`.`Child_id`, `t1`.`Id`, `t1`.`Value` FROM `FakeClass` AS t LEFT JOIN `FakeChildClass` AS t1 ON ( `t`.`Child_id`= `t1`.`Id`)", queryBuilder.Sql);
        }


        [Fact]
        public void FluentGenericQueryBuilder_LocalVariableIsTable_WhereNull()
        {
            FakeChildClass child = null;
            fluentSelectBuilder.All().All(x => child).From().LeftJoin(x => child).On(x => x.Child == child).Where(x => child == null);
            Assert.Equal("SELECT `t`.`Id`, `t`.`Text`, `t`.`Value`, `t`.`Child_id`, `t1`.`Id`, `t1`.`Value` FROM `FakeClass` AS t LEFT JOIN `FakeChildClass` AS t1 ON ( `t`.`Child_id`= `t1`.`Id`) WHERE `t1`.`Id` IS NULL", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_LocalVariableIsParameter()
        {
            FakeChildClass child = new FakeChildClass { Id = 25 };
            fluentSelectBuilder.CountAll().From().Where(x => x.Child == child);
            Assert.Equal("SELECT COUNT(*) FROM `FakeClass` AS t WHERE( `t`.`Child_id`= @Item0)", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_LocalVariableIsTable_WhereProperty()
        {
            FakeChildClass child = new FakeChildClass { Id = 18 };
            fluentSelectBuilder.All().All(x => child).From().LeftJoin(x => child).On(x => x.Child.Id == child.Id);
            Assert.Equal("SELECT `t`.`Id`, `t`.`Text`, `t`.`Value`, `t`.`Child_id`, `t1`.`Id`, `t1`.`Value` FROM `FakeClass` AS t LEFT JOIN `FakeChildClass` AS t1 ON ( `t`.`Child_id`= `t1`.`Id`)", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_Between()
        {
            fluentSelectBuilder.CountAll().From().Where(x => x.Value.Between(3, 4));
            Assert.Equal("SELECT COUNT(*) FROM `FakeClass` AS t WHERE `t`.`Value` BETWEEN @Item0 AND @Item1", fluentSelectBuilder.QueryBuilder.Sql);
        }

        public class FakeClass : IFolkeTable
        {
            public int Id { get; set; }
            public string Text { get; set; }
            public int Value { get; set; }
            public FakeChildClass Child { get; set; }
        }

        public class FakeChildClass : IFolkeTable
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }
    }
}
