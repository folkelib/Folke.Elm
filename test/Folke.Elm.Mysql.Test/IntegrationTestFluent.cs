using System;
using System.Collections.Generic;
using Folke.Elm.Fluent;
using Folke.Elm.Mapping;
using Xunit;
using Folke.Elm.Abstract.Test;

namespace Folke.Elm.Mysql.Test
{
    [Collection("IntegrationTest")]
    public class IntegrationTestFluent : IDisposable
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

        private readonly FolkeConnection connection;

        public IntegrationTestFluent()
        {
            var driver = new MySqlDriver();
            var mapper = new Mapper();
            connection = FolkeConnection.Create(driver, mapper, TestHelpers.ConnectionString);
            connection.CreateTable<TestPoco>(drop: true);
            connection.CreateTable<TestManyPoco>(drop: true);
            connection.CreateTable<TestMultiPoco>(drop: true);
            connection.CreateTable<TestCollectionMember>(drop: true);
            connection.CreateTable<TestCollection>(drop: true);

            var poco = new TestPoco { Boolean = true, Name = "FakePoco" };
            connection.Save(poco);
            var many = new TestManyPoco { Poco = poco, Toto = "FakeMany" };
            connection.Save(many);
        }

        public void Dispose()
        {
            connection.DropTable<TestCollection>();
            connection.DropTable<TestCollectionMember>();
            connection.DropTable<TestMultiPoco>();
            connection.DropTable<TestPoco>();
            connection.DropTable<TestManyPoco>();
            connection.Dispose();
        }

        [Fact]
        public void SelectAll()
        {
            connection.Select<TestPoco>().All().From().ToList();
        }

        [Fact]
        public void SelectAllAll()
        {
            connection.Select<TestManyPoco>().All().All(x => x.Poco).From().LeftJoinOnId(x => x.Poco).ToList();
        }

        [Fact]
        public void SelectValues()
        {
            connection.Select<TestPoco>().Values(x => x.Name, x => x.Boolean).From().ToList();
        }

        [Fact]
        public void SelectAllLeftJoinOnId()
        {
            connection.Select<TestManyPoco>().All().All(x => x.Poco).From().LeftJoin(x => x.Poco).OnId(x => x.Poco).ToList();
        }

        [Fact]
        public void Limit()
        {
            connection.Select<TestManyPoco>().All().From().LeftJoinOnId(x => x.Poco).Limit(0, 10).ToList();
        }
    }
}
