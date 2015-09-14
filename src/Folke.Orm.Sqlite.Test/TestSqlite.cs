using System.Configuration;
using Folke.Orm.Mapping;
using NUnit.Framework;

namespace Folke.Orm.Sqlite.Test
{
    [TestFixture]
    public class TestSqlite
    {
        private FolkeConnection connection;
        private TestClass testValue;
        private FolkeTransaction transaction;

        [SetUp]
        public void Setup()
        {
            var sqliteDriver = new SqliteDriver();
            var mapper = new Mapper();
            connection = new FolkeConnection(sqliteDriver, mapper,
                ConfigurationManager.ConnectionStrings["Test"].ConnectionString);
            transaction = connection.BeginTransaction();
            connection.CreateOrUpdateTable<TestClass>();
            connection.CreateOrUpdateTable<TestClass>();

            testValue = new TestClass {Text = "Toto"};
            connection.Save(testValue);
        }

        [TearDown]
        public void Teardown()
        {
            connection.Dispose();
        }

        [Test]
        public void Test()
        {
            transaction.Dispose();
        }

        public class TestClass : IFolkeTable
        {
            public int Id { get; set; }
            public string Text { get; set; }
        }
    }
}
