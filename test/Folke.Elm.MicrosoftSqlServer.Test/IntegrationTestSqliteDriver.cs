using Folke.Elm.Abstract.Test;
using Folke.Elm.InformationSchema;
using Folke.Elm.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Folke.Elm.MicrosoftSqlServer.Test
{
    [Collection("Sqlite")]
    public class IntegrationTestMicrosoftSqlServerDriver : IDisposable
    {
        private readonly MicrosoftSqlServerDriver driver;
        private readonly FolkeConnection connection;
        private readonly FolkeTransaction transaction;

        public IntegrationTestMicrosoftSqlServerDriver()
        {
            driver = new MicrosoftSqlServerDriver();
            connection = FolkeConnection.Create(driver, new Mapper(), TestHelpers.ConnectionString);
            transaction = connection.BeginTransaction();
        }

        public void Dispose()
        {
            transaction.Dispose();
            connection.Dispose();
        }

        [Fact]
        public void MicrosoftSqlServerDriver_GetTableDefinitions()
        {
            // Arrange
            connection.CreateTable<TestPoco>();

            // Act
            IList<TableDefinition> tableDefinitions = driver.GetTableDefinitions(connection);

            // Assert
            Assert.True(tableDefinitions.Count >= 1, "At least on table is defined");
            Assert.True(tableDefinitions.Any(x => x.Name == "TestPoco"), "The TestPoco table is present");
        }
    }
}
