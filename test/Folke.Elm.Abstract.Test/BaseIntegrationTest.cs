using System;
using System.Threading.Tasks;
using Folke.Elm.Mapping;
using Xunit;

namespace Folke.Elm.Abstract.Test
{
    public class BaseIntegrationTest
    {
        protected readonly FolkeConnection connection;
        protected readonly FolkeTransaction transaction;
        protected TableWithGuid testValue;

        public BaseIntegrationTest(IDatabaseDriver databaseDriver, string connectionString, bool drop)
        {
            var mapper = new Mapper();
            connection = FolkeConnection.Create(databaseDriver, mapper, connectionString);
            transaction = connection.BeginTransaction();
            connection.CreateTable<TestPoco>(drop: drop);
            connection.CreateTable<TestManyPoco>(drop: drop);
            connection.CreateTable<TestCollection>(drop: drop);
            connection.CreateTable<TestCollectionMember>(drop: drop);
            connection.CreateTable<TestOtherPoco>(drop: drop);
            connection.CreateTable<TestMultiPoco>(drop);

            var parentTableWithGuidMapping = mapper.GetTypeMapping<ParentTableWithGuid>();
            parentTableWithGuidMapping.Property(x => x.Key).HasColumnName("KeyColumn");
            parentTableWithGuidMapping.HasKey(x => x.Key);
            connection.CreateOrUpdateTable<TableWithGuid>();
            connection.CreateOrUpdateTable<ParentTableWithGuid>();

            testValue = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            connection.Save(testValue);
        }

        public void Dispose()
        {
            connection.DropTable<TestMultiPoco>();
            connection.DropTable<TestOtherPoco>();
            connection.DropTable<ParentTableWithGuid>();
            connection.DropTable<TableWithGuid>();
            connection.DropTable<TestCollectionMember>();
            connection.DropTable<TestCollection>();
            connection.DropTable<TestManyPoco>();
            connection.DropTable<TestPoco>();
            transaction.Dispose();
            connection.Dispose();
        }
    }
}
