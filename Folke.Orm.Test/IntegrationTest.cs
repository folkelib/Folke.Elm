using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using NUnit.Framework;

namespace Folke.Orm.Test
{
    public class IntegrationTest
    {
        private FolkeConnection connection;
        private FolkeTransaction transaction;

        [SetUp]
        public void Initialize()
        {
            var driver = new MySqlDriver();
            connection = new FolkeConnection(driver, ConfigurationManager.ConnectionStrings["Test"].ConnectionString);
            transaction = connection.BeginTransaction();
            connection.CreateOrUpdateTable<TableWithGuid>();
            connection.CreateOrUpdateTable<ParentTableWithGuid>();
        }

        [TearDown]
        public void Teardown()
        {
            connection.DropTable<TableWithGuid>();  
            connection.DropTable<ParentTableWithGuid>();
            transaction.Dispose();
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
            connection.Query<TableWithGuid>().InsertInto().Values(value).Execute();
        }

        [Test]
        public void SelectInDatabaseWithGuid()
        {
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            connection.Query<TableWithGuid>().InsertInto().Values(value).Execute();
            transaction.Commit();
            var values = connection.Query<TableWithGuid>().SelectAll().From().Where(x => x.Id == value.Id).List();
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
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            connection.Save(value);
            var result = connection.Get<TableWithGuid>(value.Id);
            Assert.AreEqual(value.Id, result.Id);
            Assert.AreEqual(value.Text, result.Text);
        }

        [Test]
        public void Delete()
        {
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            connection.Save(value);
            connection.Delete(value);
            var result = connection.Get<TableWithGuid>(value.Id);
            Assert.IsNull(result);
        }

        [Test]
        public void Update()
        {
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            connection.Save(value);
            value.Text = "Brocoli";
            connection.Update(value);
            var result = connection.Get<TableWithGuid>(value.Id);
            Assert.AreEqual(value.Id, result.Id);
            Assert.AreEqual(value.Text, result.Text);
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
            [Key]
            public Guid Key { get; set; }
            public string Text { get; set; }
            public TableWithGuid Reference { get; set; }
        }
    }
}
