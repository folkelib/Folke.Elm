using NUnit.Framework;

namespace Folke.Orm.Test
{
    [TestFixture]
    public class TestBaseQueryBuilder
    {
        private MySqlDriver mySqlDriver;
        private BaseQueryBuilder<FakeClass, FolkeTuple> queryBuilder;

        [SetUp]
        public void Setup()
        {
            mySqlDriver = new MySqlDriver();
            queryBuilder = new BaseQueryBuilder<FakeClass, FolkeTuple>(mySqlDriver);
            queryBuilder.RegisterTable();
        }

        [Test]
        public void BaseQueryBuilder_AddExpression_EqualOperator()
        {
            queryBuilder.AddExpression(x => x.Id == 3);
            Assert.AreEqual("( t.`Id`=@Item0)", queryBuilder.Sql);
        }

        [Test]
        public void BaseQueryBuilder_AddExpression_EqualsMethod()
        {
            queryBuilder.AddExpression(x => x.Id.Equals(3));
            Assert.AreEqual("( t.`Id`=@Item0)", queryBuilder.Sql);
        }

        [Test]
        public void BaseQueryBuilder_AddExpression_PropertyObjectExtension()
        {
            var propertyInfo = typeof (FakeClass).GetProperty("Id");
            queryBuilder.AddExpression(x => x.Property(propertyInfo).Equals(3));
            Assert.AreEqual("( t.`Id`=@Item0)", queryBuilder.Sql);
        }

        [Test]
        public void BaseQueryBuilder_AddExpression_LikeExtension()
        {
            queryBuilder.AddExpression(x => x.Text.Like("toto"));
            Assert.AreEqual(" t.`Text` LIKE @Item0", queryBuilder.Sql);
        }

        [Test]
        public void BaseQueryBuilder_AddExpression_StringStartsWith()
        {
            queryBuilder.AddExpression(x => x.Text.StartsWith("toto"));
            Assert.AreEqual(" t.`Text` LIKE @Item0", queryBuilder.Sql);
        }

        private class FakeClass : IFolkeTable
        {
            public int Id { get; set; }
            public string Text { get; set; }
        }
    }
}
