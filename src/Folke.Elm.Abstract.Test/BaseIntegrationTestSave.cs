using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Folke.Elm.Abstract.Test
{
    public class BaseIntegrationTestSave : BaseIntegrationTest, IIntegrationTestSave
    {
        public BaseIntegrationTestSave(IDatabaseDriver databaseDriver, string connectionString, bool drop) : base(databaseDriver, connectionString, drop)
        {
        }

        public void Save()
        {
            var newPoco = new TestPoco { Name = "Tutu " };
            connection.Save(newPoco);
            Assert.NotEqual(0, newPoco.Id);
        }

        public void InsertInto_ObjectWithGuid()
        {
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            connection.InsertInto<TableWithGuid>().Values(value).Execute();
        }

        public async Task SaveAsync_ObjectWithGuid()
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
    }
}
