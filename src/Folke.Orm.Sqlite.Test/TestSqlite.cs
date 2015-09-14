using System;
using System.Configuration;
using Folke.Orm.Mapping;
using Xunit;

namespace Folke.Orm.Sqlite.Test
{
    [Collection("SqliteIntegrationTest")]
    public class TestSqlite : IDisposable
    {
        private readonly FolkeConnection connection;
        private readonly FolkeTransaction transaction;

        public TestSqlite()
        {
            var sqliteDriver = new SqliteDriver();
            var mapper = new Mapper();
            connection = new FolkeConnection(sqliteDriver, mapper,
                ConfigurationManager.ConnectionStrings["Test"].ConnectionString);
            transaction = connection.BeginTransaction();
            connection.CreateOrUpdateTable<TestClass>();
            connection.CreateOrUpdateTable<TestClass>();

            var testValue = new TestClass {Text = "Toto"};
            connection.Save(testValue);
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        [Fact]
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
