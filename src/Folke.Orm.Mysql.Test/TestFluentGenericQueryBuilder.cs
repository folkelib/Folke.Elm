using System.Linq;
using Folke.Orm.Fluent;
using Folke.Orm.Mapping;
using System;
using Xunit;

namespace Folke.Orm.Mysql.Test
{
    public class TestFluentGenericQueryBuilder : IDisposable
    {
        private MySqlDriver mySqlDriver;
        private FluentSelectBuilder<FakeClass, FolkeTuple> fluentSelectBuilder;
        private BaseQueryBuilder queryBuilder;

        public TestFluentGenericQueryBuilder()
        {
            mySqlDriver = new MySqlDriver();
            var mapper = new Mapper();
            fluentSelectBuilder = new FluentSelectBuilder<FakeClass, FolkeTuple>(mySqlDriver, mapper);
            queryBuilder = fluentSelectBuilder.QueryBuilder;
        }

        public void Dispose()
        {
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
            Assert.Equal("SELECT( `t`.`Id`= @Item0)", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_Select_LikeExtension()
        {
            fluentSelectBuilder.Values(x => x.Text.Like("toto"));
            Assert.Equal("SELECT `t`.`Text` LIKE @Item0", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_Select_StringStartsWith()
        {
            fluentSelectBuilder.Values(x => x.Text.StartsWith("toto"));
            Assert.Equal("SELECT `t`.`Text` LIKE @Item0", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_Select_ListOfExpressionsFromDefaultTable()
        {
            fluentSelectBuilder.Values(x => x.Id, x => x.Text);
            Assert.Equal("SELECT `t`.`Id` , `t`.`Text`", queryBuilder.Sql);
            Assert.Equal(2, queryBuilder.SelectedFields.Count);
            Assert.True(queryBuilder.SelectedFields.Any(x => x.PropertyMapping.PropertyInfo == typeof(FakeClass).GetProperty("Id")));
            Assert.True(queryBuilder.SelectedFields.Any(x => x.PropertyMapping.PropertyInfo == typeof(FakeClass).GetProperty("Text")));
        }

        [Fact]
        public void FluentGenericQueryBuilder_Select_ListOfExpressionsFromDefaultTableAndJoin()
        {
            fluentSelectBuilder.Values(x => x.Id, x => x.Text, x => x.Child.Value);
            Assert.Equal("SELECT `t`.`Id` , `t`.`Text` , `t1`.`Value`", queryBuilder.Sql);
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
        public void FluentGenericQueryBuilder_WhereSubAfterWhere()
        {
            fluentSelectBuilder.All()
                .From()
                .Where(x => x.Text == "fake")
                .WhereSub(select => select.Or(x => x.Text == "test").Or(x => x.Text == "other"));
            Assert.Equal("SELECT  `t`.`Id`, `t`.`Text`, `t`.`Child_id` FROM `FakeClass` as t WHERE( `t`.`Text`= @Item0) AND (( `t`.`Text`= @Item1) OR ( `t`.`Text`= @Item2) )", queryBuilder.Sql);
        }

        [Fact]
        public void FluentGenericQueryBuilder_WhereSub()
        {
            fluentSelectBuilder.All()
                .From()
                .WhereSub(select => select.Or(x => x.Text == "test").Or(x => x.Text == "other"));
            Assert.Equal("SELECT  `t`.`Id`, `t`.`Text`, `t`.`Child_id` FROM `FakeClass` as t WHERE (( `t`.`Text`= @Item0) OR ( `t`.`Text`= @Item1) )", queryBuilder.Sql);
        }
        
        public class FakeClass : IFolkeTable
        {
            public int Id { get; set; }
            public string Text { get; set; }
            public FakeChildClass Child { get; set; }
        }

        public class FakeChildClass : IFolkeTable
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }
    }
}
