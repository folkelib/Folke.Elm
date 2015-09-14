using System.Configuration;
using Folke.Orm.Mapping;
using System;
using Xunit;

namespace Folke.Orm.Mysql.Test
{
    [Collection("Integration tests")]
    public class TestSchemaUpdater : IDisposable
    {
        public class FirstClass
        {
            public int Id { get; set; }
            public string Text { get; set; }
        }

        private FolkeConnection connection;
        
        public TestSchemaUpdater()
        {
            var driver = new MySqlDriver();
            var mapper = new Mapper();
            connection = new FolkeConnection(driver, mapper, TestHelpers.ConnectionString);
            connection.CreateTable<FirstClass>(true);
        }

        public void Dispose()
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

        [Fact]
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

        [Fact]    
        public void ChangeColumnType()
        {
            connection.CreateOrUpdateTable<ChangeColumnTypeClass.FirstClass>();
        }
    }
}

