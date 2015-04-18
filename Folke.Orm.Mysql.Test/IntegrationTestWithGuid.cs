using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Folke.Orm.Mapping;
using NUnit.Framework;

namespace Folke.Orm.Mysql.Test
{
    public class IntegrationTestWithGuid
    {
        private FolkeConnection connection;
        private FolkeTransaction transaction;
        private TableWithGuid testValue;

        [SetUp]
        public void Initialize()
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

        [TearDown]
        public void Teardown()
        {
            connection.DropTable<TableWithGuid>();  
            connection.DropTable<ParentTableWithGuid>();
            transaction.Dispose();
            connection.Dispose();
        }

        [Test]
        public void CreateDatabaseWithGuid()
        {
        }

        [Test]
        public void InsertInDatabaseWithGuid()
        {
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            connection.InsertInto<TableWithGuid>().Values(value).Execute();
        }

        [Test]
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
            Assert.IsNotEmpty(values);
            Assert.AreEqual(value.Id, values[0].Id);
            Assert.AreEqual(value.Text, values[0].Text);
        }

        [Test]
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
            Assert.IsNotEmpty(values);
            Assert.AreEqual(value.Id, values[0].Id);
            Assert.AreEqual(value.Text, values[0].Text);
        }

        [Test]
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
            Assert.IsNotEmpty(values);
            Assert.AreEqual(value.Id, values[0].Id);
            Assert.AreEqual(value.Text, values[0].Text);
        }

        [Test]
        public void SelectInDatabaseWithGuidUsingShortcuts()
        {
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            connection.Save(value);
            var result = connection.Load<TableWithGuid>(value.Id);
            Assert.AreEqual(value.Id, result.Id);
            Assert.AreEqual(value.Text, result.Text);
        }

        [Test]
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
            Assert.AreEqual(parent.Key, result.Key);
            Assert.AreEqual(parent.Text, result.Text);
            Assert.AreEqual(value.Id, parent.Reference.Id);
            Assert.AreEqual(value.Text, parent.Reference.Text);
        }

        [Test]
        public void Get()
        {
            var result = connection.Get<TableWithGuid>(testValue.Id);
            Assert.AreEqual(testValue.Id, result.Id);
            Assert.AreEqual(testValue.Text, result.Text);
        }

        [Test]
        public async void LoadAsync()
        {
            var result = await connection.LoadAsync<TableWithGuid>(testValue.Id);
            Assert.AreEqual(testValue.Id, result.Id);
            Assert.AreEqual(testValue.Text, result.Text);
        }

        [Test]
        public async void GetAsync()
        {
            var result = await connection.GetAsync<TableWithGuid>(testValue.Id);
            Assert.AreEqual(testValue.Id, result.Id);
            Assert.AreEqual(testValue.Text, result.Text);
        }

        [Test]
        public async void SaveAsync()
        {
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            await connection.SaveAsync(value);
            var result = await connection.GetAsync<TableWithGuid>(value.Id);
            Assert.AreEqual(value.Id, result.Id);
            Assert.AreEqual(value.Text, result.Text);
        }

        [Test]
        public void Delete()
        {
            connection.Delete(testValue);
            var result = connection.Get<TableWithGuid>(testValue.Id);
            Assert.IsNull(result);
        }

        [Test]
        public async Task DeleteAsync()
        {
            await connection.DeleteAsync(testValue);
            var result = connection.Get<TableWithGuid>(testValue.Id);
            Assert.IsNull(result);
        }

        [Test]
        public void Update()
        {
            testValue.Text = "Brocoli";
            connection.Update(testValue);
            var result = connection.Get<TableWithGuid>(testValue.Id);
            Assert.AreEqual(testValue.Id, result.Id);
            Assert.AreEqual(testValue.Text, result.Text);
        }

        [Test]
        public async Task UpdateAsync()
        {
            testValue.Text = "Brocoli";
            await connection.UpdateAsync(testValue);
            var result = connection.Get<TableWithGuid>(testValue.Id);
            Assert.AreEqual(testValue.Id, result.Id);
            Assert.AreEqual(testValue.Text, result.Text);
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
