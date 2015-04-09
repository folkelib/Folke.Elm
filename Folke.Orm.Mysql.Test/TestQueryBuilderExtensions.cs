using System.Configuration;
using Folke.Orm.Mapping;
using NUnit.Framework;

namespace Folke.Orm.Mysql.Test
{
    [TestFixture]
    public class TestQueryBuilderExtensions
    {
        private FolkeConnection connection;
        private FolkeTransaction transaction;
        private TestPoco testPoco;

        [SetUp]
        public void Setup()
        {
            var driver = new MySqlDriver();
            var mapper = new Mapper();
            connection = new FolkeConnection(driver, mapper, TestHelpers.ConnectionString);
            transaction = connection.BeginTransaction();
            connection.CreateOrUpdateTable<TestPoco>();
            connection.CreateOrUpdateTable<TestManyPoco>();
            testPoco = new TestPoco { Name = "FakeName" };
            connection.Save(testPoco); 
        }

        [TearDown]
        public void Teardown()
        {
            connection.DropTable<TestManyPoco>();
            connection.DropTable<TestPoco>();
            transaction.Dispose();
            connection.Dispose();
        }

        [Test]
        public async void ExecuteAsync()
        {
            var poco = new TestPoco();
            var queryBuilder = connection.InsertInto<TestPoco>().Values(poco); // TODO utiliser un fake
            await queryBuilder.ExecuteAsync();
        }

        [Test]
        public async void ListAsync()
        {
            var queryBuilder = connection.QueryOver<TestPoco>(); // TODO utiliser un fake
            var list = await queryBuilder.ListAsync();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(testPoco.Id, list[0].Id);
            Assert.AreEqual(testPoco.Name, list[0].Name);
        }

        [Test]
        public async void SingleAsync()
        {
            var queryBuilder = connection.QueryOver<TestPoco>().Where(x => x.Id == testPoco.Id); // TODO utiliser un fake
            var result = await queryBuilder.SingleAsync();
            Assert.AreEqual(testPoco.Id, result.Id);
            Assert.AreEqual(testPoco.Name, result.Name);
        }

        [Test]
        public async void SingleOrDefaultAsync()
        {
            var queryBuilder = connection.QueryOver<TestPoco>().Where(x => x.Id == testPoco.Id); // TODO utiliser un fake
            var result = await queryBuilder.SingleOrDefaultAsync();
            Assert.AreEqual(testPoco.Id, result.Id);
            Assert.AreEqual(testPoco.Name, result.Name);
        }

        [Test]
        public async void ScalarAsync()
        {
            var queryBuilder = connection.Select<TestPoco>().Values(x => x.Name).From().Where(x => x.Id == testPoco.Id); // TODO utiliser un fake
            var result = await queryBuilder.ScalarAsync<string>();
            Assert.AreEqual(testPoco.Name, result);
        }

        public class TestPoco : IFolkeTable
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool Boolean { get; set; }
        }

        public class TestManyPoco : IFolkeTable
        {
            public int Id { get; set; }
            public string Toto { get; set; }
            public TestPoco Poco { get; set; }
        }
    }
}
