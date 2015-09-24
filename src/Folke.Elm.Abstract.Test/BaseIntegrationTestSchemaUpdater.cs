using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Folke.Elm.Mapping;

namespace Folke.Elm.Abstract.Test
{
    public class BaseIntegrationTestSchemaUpdater : IIntegrationTestSchemaUpdater
    {
        public class FirstClass
        {
            public int Id { get; set; }
            public string Text { get; set; }
            public Guid Guid { get; set; }
            public decimal Decimal { get; set; }
        }

        public class SecondClass
        {
            [Key]
            public int Id { get; set; }
            public string Text { get; set; }
        }

        public class ChangeColumnTypeClass
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public class FirstClass
            {
                public int Id { get; set; }
                public int Text { get; set; }
                public Guid Guid { get; set; }
                public decimal Decimal { get; set; }
            }
        }

        private readonly FolkeConnection connection;
        private readonly FolkeTransaction transaction;

        public BaseIntegrationTestSchemaUpdater(IDatabaseDriver driver, string connectionString, bool drop)
        {
            var mapper = new Mapper();
            connection = new FolkeConnection(driver, mapper, connectionString);
            transaction = connection.BeginTransaction();
            connection.CreateTable<FirstClass>(drop);
            connection.CreateTable<SecondClass>(drop);
        }

        public void Dispose()
        {
            connection.DropTable<FirstClass>();
            transaction.Dispose();
            connection.Dispose();
        }

        public class AddColumnClass
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public class FirstClass
            {
                public int Id { get; set; }
                public string Text { get; set; }
                public Guid Guid { get; set; }
                public decimal Decimal { get; set; }
                public int Int { get; set; }
                public SecondClass SecondClass { get; set; }
            }
        }
        
        public void SchemaUpdater_AddColumn()
        {
            connection.CreateOrUpdateTable<AddColumnClass.FirstClass>();
        }
        
        public void SchemaUpdater_ChangeColumnType()
        {
            connection.CreateOrUpdateTable<ChangeColumnTypeClass.FirstClass>();
        }
    }
}
