using System.Linq;
using NUnit.Framework;

namespace Folke.Orm.Mysql.Test
{
    [TestFixture]
    public class TestFluentGenericQueryBuilder
    {
        private MySqlDriver mySqlDriver;
        private FluentGenericQueryBuilder<FakeClass, FolkeTuple> queryBuilder;

        [SetUp]
        public void Setup()
        {
            mySqlDriver = new MySqlDriver();
            queryBuilder = new FluentGenericQueryBuilder<FakeClass, FolkeTuple>(mySqlDriver);
            queryBuilder.RegisterTable();
        }

        [Test]
        public void FluentGenericQueryBuilder_Select_EqualOperator()
        {
            queryBuilder.Select(x => x.Id == 3);
            Assert.AreEqual("SELECT( `t`.`Id`= @Item0)", queryBuilder.Sql);
        }

        [Test]
        public void FluentGenericQueryBuilder_Select_EqualsMethod()
        {
            queryBuilder.Select(x => x.Id.Equals(3));
            Assert.AreEqual("SELECT( `t`.`Id`= @Item0)", queryBuilder.Sql);
        }

        [Test]
        public void FluentGenericQueryBuilder_Select_PropertyObjectExtension()
        {
            var propertyInfo = typeof (FakeClass).GetProperty("Id");
            queryBuilder.Select(x => x.Property(propertyInfo).Equals(3));
            Assert.AreEqual("SELECT( `t`.`Id`= @Item0)", queryBuilder.Sql);
        }

        [Test]
        public void FluentGenericQueryBuilder_Select_LikeExtension()
        {
            queryBuilder.Select(x => x.Text.Like("toto"));
            Assert.AreEqual("SELECT `t`.`Text` LIKE @Item0", queryBuilder.Sql);
        }

        [Test]
        public void FluentGenericQueryBuilder_Select_StringStartsWith()
        {
            queryBuilder.Select(x => x.Text.StartsWith("toto"));
            Assert.AreEqual("SELECT `t`.`Text` LIKE @Item0", queryBuilder.Sql);
        }

        [Test]
        public void FluentGenericQueryBuilder_Select_ListOfExpressionsFromDefaultTable()
        {
            queryBuilder.Select(x => x.Id, x => x.Text);
            Assert.AreEqual("SELECT `t`.`Id` , `t`.`Text`", queryBuilder.Sql);
            Assert.AreEqual(2, queryBuilder.SelectedFields.Count);
            Assert.IsTrue(queryBuilder.SelectedFields.Any(x => x.propertyInfo == typeof(FakeClass).GetProperty("Id")));
            Assert.IsTrue(queryBuilder.SelectedFields.Any(x => x.propertyInfo == typeof(FakeClass).GetProperty("Text")));
        }

        [Test]
        public void FluentGenericQueryBuilder_Select_ListOfExpressionsFromDefaultTableAndJoin()
        {
            queryBuilder.Select(x => x.Id, x => x.Text, x => x.Child.Value);
            Assert.AreEqual("SELECT `t`.`Id` , `t`.`Text` , `t1`.`Value`", queryBuilder.Sql);
            Assert.AreEqual(3, queryBuilder.SelectedFields.Count);
            Assert.IsTrue(queryBuilder.SelectedFields.Any(x => x.propertyInfo == typeof(FakeClass).GetProperty("Id")));
            Assert.IsTrue(queryBuilder.SelectedFields.Any(x => x.propertyInfo == typeof(FakeClass).GetProperty("Text")));
            Assert.IsTrue(queryBuilder.SelectedFields.Any(x => x.propertyInfo == typeof(FakeChildClass).GetProperty("Value")));
        }

        [Test]
        public void FluentGenericQueryBuilder_Select_Max()
        {
            queryBuilder.Select(x => SqlFunctions.Max(x.Id));
            Assert.AreEqual("SELECT MAX( `t`.`Id`)", queryBuilder.Sql);
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
