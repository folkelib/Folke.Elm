using Folke.Orm.Mapping;

namespace Folke.Orm.Mysql.Test
{
    using System.Collections.Generic;
    using System.Configuration;

    using Folke.Orm.Fluent;

    using NUnit.Framework;

    [TestFixture]
    public class IntegrationTestFluent
    {
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

        public class TestMultiPoco : IFolkeTable
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public TestPoco One { get; set; }
            public TestPoco Two { get; set; }
            public TestManyPoco Three { get; set; }
        }

        public class TestCollectionMember : IFolkeTable
        {
            public int Id { get; set; }
            public TestCollection Collection { get; set; }
            public string Name { get; set; }
        }

        public class TestCollection : IFolkeTable
        {
            public int Id { get; set; }
            public IReadOnlyList<TestCollectionMember> Members { get; set; }
            public string Name { get; set; }
        }

        private FolkeConnection connection;

        [SetUp]
        public void Initialize()
        {
            var driver = new MySqlDriver();
            var mapper = new Mapper();
            connection = new FolkeConnection(driver, mapper, TestHelpers.ConnectionString);
            connection.CreateTable<IntegrationTestWithFolkeTable.TestPoco>(drop: true);
            connection.CreateTable<IntegrationTestWithFolkeTable.TestManyPoco>(drop: true);
            connection.CreateTable<IntegrationTestWithFolkeTable.TestMultiPoco>(drop: true);
            connection.CreateTable<IntegrationTestWithFolkeTable.TestCollectionMember>(drop: true);
            connection.CreateTable<IntegrationTestWithFolkeTable.TestCollection>(drop: true);

            var poco = new TestPoco { Boolean = true, Name = "FakePoco" };
            connection.Save(poco);
            var many = new TestManyPoco { Poco = poco, Toto = "FakeMany" };
            connection.Save(many);
        }

        [TearDown]
        public void Cleanup()
        {
            connection.DropTable<IntegrationTestWithFolkeTable.TestCollection>();
            connection.DropTable<IntegrationTestWithFolkeTable.TestCollectionMember>();
            connection.DropTable<IntegrationTestWithFolkeTable.TestMultiPoco>();
            connection.DropTable<IntegrationTestWithFolkeTable.TestPoco>();
            connection.DropTable<IntegrationTestWithFolkeTable.TestManyPoco>();
            connection.Dispose();
        }

        [Test]
        public void SelectAll()
        {
            connection.Select<TestPoco>().All().From().List();
        }

        [Test]
        public void SelectAllAll()
        {
            connection.Select<TestManyPoco>().All().All(x => x.Poco).From().From(x => x.Poco).List();
        }

        [Test]
        public void SelectValues()
        {
            connection.Select<TestPoco>().Values(x => x.Name, x => x.Boolean).From().List();
        }

        [Test]
        public void SelectAllLeftJoinOnId()
        {
            connection.Select<TestManyPoco>().All().All(x => x.Poco).From().LeftJoin(x => x.Poco).OnId(x => x.Poco).List();
        }
    }
}
