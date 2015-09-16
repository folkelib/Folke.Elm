using System;
using Folke.Elm.Mapping;
using Xunit;

namespace Folke.Elm.Mysql.Test
{
    [Collection("IntegrationTest")]
    public class TestQueryBuilderExtensions : IDisposable
    {
        private FolkeConnection connection;
        private FolkeTransaction transaction;
        private TestPoco testPoco;

        public TestQueryBuilderExtensions()
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

        public void Dispose()
        {
            connection.DropTable<TestManyPoco>();
            connection.DropTable<TestPoco>();
            transaction.Dispose();
            connection.Dispose();
        }

        [Fact]
        public async void ExecuteAsync()
        {
            var poco = new TestPoco();
            var queryBuilder = connection.InsertInto<TestPoco>().Values(poco); // TODO utiliser un fake
            await queryBuilder.ExecuteAsync();
        }

        [Fact]
        public async void ListAsync()
        {
            var queryBuilder = connection.SelectAllFrom<TestPoco>(); // TODO utiliser un fake
            var list = await queryBuilder.ListAsync();
            Assert.Equal(1, list.Count);
            Assert.Equal(testPoco.Id, list[0].Id);
            Assert.Equal(testPoco.Name, list[0].Name);
        }

        [Fact]
        public async void SingleAsync()
        {
            var queryBuilder = connection.SelectAllFrom<TestPoco>().Where(x => x.Id == testPoco.Id); // TODO utiliser un fake
            var result = await queryBuilder.SingleAsync();
            Assert.Equal(testPoco.Id, result.Id);
            Assert.Equal(testPoco.Name, result.Name);
        }

        [Fact]
        public async void SingleOrDefaultAsync()
        {
            var queryBuilder = connection.SelectAllFrom<TestPoco>().Where(x => x.Id == testPoco.Id); // TODO utiliser un fake
            var result = await queryBuilder.SingleOrDefaultAsync();
            Assert.Equal(testPoco.Id, result.Id);
            Assert.Equal(testPoco.Name, result.Name);
        }

        [Fact]
        public async void ScalarAsync()
        {
            var queryBuilder = connection.Select<TestPoco>().Values(x => x.Name).From().Where(x => x.Id == testPoco.Id); // TODO utiliser un fake
            var result = await queryBuilder.ScalarAsync<string>();
            Assert.Equal(testPoco.Name, result);
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
