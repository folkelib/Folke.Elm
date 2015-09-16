using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Folke.Elm.Abstract.Test
{
    public class BaseIntegrationTestDelete : BaseIntegrationTest, IIntegrationTestDelete
    {
        public BaseIntegrationTestDelete(IDatabaseDriver databaseDriver, string connectionString, bool drop) : base(databaseDriver, connectionString, drop)
        {
        }

        public void Delete_ObjectWithGuid()
        {
            connection.Delete(testValue);
            var result = connection.Get<TableWithGuid>(testValue.Id);
            Assert.Null(result);
        }

        public async Task DeleteAsync_ObjectWithGuid()
        {
            await connection.DeleteAsync(testValue);
            var result = connection.Get<TableWithGuid>(testValue.Id);
            Assert.Null(result);
        }
    }
}
