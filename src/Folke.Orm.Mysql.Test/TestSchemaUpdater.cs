using System.Configuration;
using Folke.Orm.Mapping;
using NUnit.Framework;

namespace Folke.Orm.Mysql.Test
{
    [TestFixture]
    public class TestSchemaUpdater
    {
        public class FirstClass
        {
            public int Id { get; set; }
            public string Text { get; set; }
        }

        private FolkeConnection connection;
        
        [SetUp]
        public void Initialize()
        {
            var driver = new MySqlDriver();
            var mapper = new Mapper();
            connection = new FolkeConnection(driver, mapper, TestHelpers.ConnectionString);
            connection.CreateTable<FirstClass>(true);
        }

        [TearDown]
        public void Cleanup()
        {
            connection.DropTable<FirstClass>();
            connection.Dispose();
        }

        public class AddColumnClass
        {
// ReSharper disable once MemberHidesStaticFromOuterClass
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

        public class ChangeColumnTypeClass
        {
// ReSharper disable once MemberHidesStaticFromOuterClass
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

