using System.Configuration;
using NUnit.Framework;

namespace Folke.Orm.Test
{
    [TestFixture]
    public class TestSchemaUpdater
    {
        private class FirstClass
        {
            public int Id { get; set; }
            public string Text { get; set; }
        }

        private FolkeConnection connection;
        
        [SetUp]
        public void Initialize()
        {
            var driver = new MySqlDriver();
            connection = new FolkeConnection(driver, ConfigurationManager.ConnectionStrings["Test"].ConnectionString);
            connection.CreateTable<FirstClass>(true);
        }

        [TearDown]
        public void Cleanup()
        {
            connection.DropTable<FirstClass>();
            connection.Dispose();
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

        [Test]
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

        [Test]    
        public void ChangeColumnType()
        {
            connection.CreateOrUpdateTable<ChangeColumnTypeClass.FirstClass>();
        }
    }
}

