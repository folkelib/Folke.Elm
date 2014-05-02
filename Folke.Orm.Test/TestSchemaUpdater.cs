using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Folke.Orm.Test
{
    [TestClass]
    public class TestSchemaUpdater
    {
        private class FirstClass
        {
            public int Id { get; set; }
            public string Text { get; set; }
        }

        private FolkeConnection connection;
        
        [TestInitialize]
        public void Initialize()
        {
            var driver = new MySqlDriver(new DatabaseSettings { Database = "folketest", Host = "localhost", Password = "toto", User = "checked" });
            connection = new FolkeConnection(driver);
            connection.CreateTable<FirstClass>(true);
        }

        [TestCleanup]
        public void Cleanup()
        {
            connection.DropTable<FirstClass>();
        }

        private class AddColumnClass
        {
            public class FirstClass
            {
                public int Id { get; set; }
                public string Text { get; set; }
                public int Int { get; set; }
            }
        }

        [TestMethod]
        public void AddColumn()
        {
            connection.CreateOrUpdateTable<AddColumnClass.FirstClass>();
        }

        private class ChangeColumnTypeClass
        {
            public class FirstClass
            {
                public int Id { get; set; }
                public int Text { get; set; }
            }
        }

        [TestMethod]    
        public void ChangeColumnType()
        {
            connection.CreateOrUpdateTable<ChangeColumnTypeClass.FirstClass>();
        }
    }
}

