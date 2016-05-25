using System;
using System.Collections.Generic;
using System.Linq;
using Folke.Elm.InformationSchema;
using Folke.Elm.Mapping;
using Xunit;

namespace Folke.Elm.Sqlite.Test
{
    [Collection("Sqlite")]
    public class IntegrationTestSqliteDriver : IDisposable
    {
        private readonly SqliteDriver driver;
        private readonly FolkeConnection connection;
        private readonly FolkeTransaction transaction;

        public IntegrationTestSqliteDriver()
        {
            driver = new SqliteDriver();
            connection = FolkeConnection.Create(driver, new Mapper(), TestHelpers.ConnectionString);
            transaction = connection.BeginTransaction();
        }

        public void Dispose()
        {
            transaction.Dispose();
            connection.Dispose();
        }

        [Fact]
        public void SqliteDriver_GetTableDefinitions()
        {
            // Arrange
            connection.CreateTable<TestPoco>();

            // Act
            IList<TableDefinition> tableDefinitions = driver.GetTableDefinitions(connection);

            // Assert
            Assert.True(tableDefinitions.Count >= 1);
            Assert.True(tableDefinitions.Any(x => x.Name == "TestPoco"));
        }
    }
}
