using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Folke.Orm.Mapping;
using Xunit;

namespace Folke.Orm.Mysql.Test
{
    [Collection("IntegrationTest")]
    public class IntegrationTestWithGuid : IDisposable
    {
        private readonly FolkeConnection connection;
        private readonly FolkeTransaction transaction;
        private readonly TableWithGuid testValue;

        public IntegrationTestWithGuid()
        {
            var driver = new MySqlDriver();
            var mapper = new Mapper();
            var parentTableWithGuidMapping = mapper.GetTypeMapping<ParentTableWithGuid>();
            parentTableWithGuidMapping.Property(x => x.Key).HasColumnName("KeyColumn");
            parentTableWithGuidMapping.HasKey(x => x.Key);
            connection = new FolkeConnection(driver, mapper, TestHelpers.ConnectionString);
            transaction = connection.BeginTransaction();
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
            connection.DropTable<TableWithGuid>();  
            connection.DropTable<ParentTableWithGuid>();
            transaction.Dispose();
            connection.Dispose();
        }

        [Fact]
        public void CreateDatabaseWithGuid()
        {
        }

        [Fact]
        public void InsertInDatabaseWithGuid()
        {
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            connection.InsertInto<TableWithGuid>().Values(value).Execute();
        }

        [Fact]
        public void SelectInDatabaseWithGuid()
        {
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            connection.InsertInto<TableWithGuid>().Values(value).Execute();
            transaction.Commit();
            var values = connection.Select<TableWithGuid>().All().From().Where(x => x.Id == value.Id).List();
            Assert.NotEmpty(values);
            Assert.Equal(value.Id, values[0].Id);
            Assert.Equal(value.Text, values[0].Text);
        }

        [Fact]
        public void SelectInDatabaseWithObject()
        {
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            connection.InsertInto<TableWithGuid>().Values(value).Execute();
            transaction.Commit();
            var values = connection.Select<TableWithGuid>().All().From().Where(x => x == value).List();
            Assert.NotEmpty(values);
            Assert.Equal(value.Id, values[0].Id);
            Assert.Equal(value.Text, values[0].Text);
        }

        [Fact]
        public void SelectInDatabaseWithParameters()
        {
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            connection.InsertInto<TableWithGuid>().Values(value).Execute();
            transaction.Commit();
            var values = connection.Select<TableWithGuid, FolkeTuple<TableWithGuid>>().All().From().Where((x, p) => x == p.Item0).List(connection, value);
            Assert.NotEmpty(values);
            Assert.Equal(value.Id, values[0].Id);
            Assert.Equal(value.Text, values[0].Text);
        }

        [Fact]
        public void SelectInDatabaseWithGuidUsingShortcuts()
        {
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            connection.Save(value);
            var result = connection.Load<TableWithGuid>(value.Id);
            Assert.Equal(value.Id, result.Id);
            Assert.Equal(value.Text, result.Text);
        }

        [Fact]
        public void SelectInDatabaseWithGuidUsingShortcutsAndJoin()
        {
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            connection.Save(value);
            var parent = new ParentTableWithGuid
            {
                Key = Guid.NewGuid(),
                Text = "Parent",
                Reference = value
            };
            connection.Save(parent);
            var result = connection.Load<ParentTableWithGuid>(parent.Key, x => x.Reference);
            Assert.Equal(parent.Key, result.Key);
            Assert.Equal(parent.Text, result.Text);
            Assert.Equal(value.Id, parent.Reference.Id);
            Assert.Equal(value.Text, parent.Reference.Text);
        }

        [Fact]
        public void Get()
        {
            var result = connection.Get<TableWithGuid>(testValue.Id);
            Assert.Equal(testValue.Id, result.Id);
            Assert.Equal(testValue.Text, result.Text);
        }

        [Fact]
        public async void LoadAsync()
        {
            var result = await connection.LoadAsync<TableWithGuid>(testValue.Id);
            Assert.Equal(testValue.Id, result.Id);
            Assert.Equal(testValue.Text, result.Text);
        }

        [Fact]
        public async void GetAsync()
        {
            var result = await connection.GetAsync<TableWithGuid>(testValue.Id);
            Assert.Equal(testValue.Id, result.Id);
            Assert.Equal(testValue.Text, result.Text);
        }

        [Fact]
        public async void SaveAsync()
        {
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            await connection.SaveAsync(value);
            var result = await connection.GetAsync<TableWithGuid>(value.Id);
            Assert.Equal(value.Id, result.Id);
            Assert.Equal(value.Text, result.Text);
        }

        [Fact]
        public void Delete()
        {
            connection.Delete(testValue);
            var result = connection.Get<TableWithGuid>(testValue.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync()
        {
            await connection.DeleteAsync(testValue);
            var result = connection.Get<TableWithGuid>(testValue.Id);
            Assert.Null(result);
        }

        [Fact]
        public void Update()
        {
            testValue.Text = "Brocoli";
            connection.Update(testValue);
            var result = connection.Get<TableWithGuid>(testValue.Id);
            Assert.Equal(testValue.Id, result.Id);
            Assert.Equal(testValue.Text, result.Text);
        }

        [Fact]
        public async Task UpdateAsync()
        {
            testValue.Text = "Brocoli";
            await connection.UpdateAsync(testValue);
            var result = connection.Get<TableWithGuid>(testValue.Id);
            Assert.Equal(testValue.Id, result.Id);
            Assert.Equal(testValue.Text, result.Text);
        }

        [Table("TableWithGuid")]
        private class TableWithGuid
        {
            [Key]
            public Guid Id { get; set; }
            public string Text { get; set; }
        }

        private class ParentTableWithGuid
        {
            // [Key]
            public Guid Key { get; set; }
            public string Text { get; set; }
            public TableWithGuid Reference { get; set; }
        }
    }
}
