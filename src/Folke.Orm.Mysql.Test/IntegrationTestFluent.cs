using System.Collections.Generic;
using System.Configuration;

using Folke.Orm.Fluent;
using Folke.Orm.Mapping;
using System;
using Xunit;

namespace Folke.Orm.Mysql.Test
{
    [Collection("Integration tests")]
    public class IntegrationTestFluent : IDisposable
    {
        public class TestTable : IFolkeTable
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool Boolean { get; set; }
        }

        public class TestReferenceToTable : IFolkeTable
        {
            public int Id { get; set; }
            public string Toto { get; set; }
            public TestTable Poco { get; set; }
        }
        
        private FolkeConnection connection;

        public IntegrationTestFluent()
        {
            var driver = new MySqlDriver();
            var mapper = new Mapper();
            connection = new FolkeConnection(driver, mapper, TestHelpers.ConnectionString);
            connection.CreateTable<TestTable>(drop: true);
            connection.CreateTable<TestReferenceToTable>(drop: true);
            
            var poco = new TestTable { Boolean = true, Name = "FakePoco" };
            connection.Save(poco);
            var many = new TestReferenceToTable { Poco = poco, Toto = "FakeMany" };
            connection.Save(many);
        }

        public void Dispose()
        {
            connection.DropTable<TestReferenceToTable>();
            connection.DropTable<TestTable>();
            connection.Dispose();
        }

        [Fact]
        public void SelectAll()
        {
            connection.Select<TestTable>().All().From().List();
        }
        
        [Fact]
        public void SelectAllAll()
        {
            connection.Select<TestReferenceToTable>().All().All(x => x.Poco).From().From(x => x.Poco).List();
        }

        [Fact]
        public void SelectValues()
        {
            connection.Select<TestTable>().Values(x => x.Name, x => x.Boolean).From().List();
        }

        [Fact]
        public void SelectAllLeftJoinOnId()
        {
            connection.Select<TestReferenceToTable>().All().All(x => x.Poco).From().LeftJoin(x => x.Poco).OnId(x => x.Poco).List();
        }

        [Fact]
        public void Limit()
        {
            connection.Select<TestReferenceToTable>().All().From().LeftJoinOnId(x => x.Poco).Limit(0, 10).List();
        }
    }
}
