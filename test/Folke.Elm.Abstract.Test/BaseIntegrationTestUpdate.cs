using System.Threading.Tasks;
using Xunit;
using Folke.Elm.Fluent;

namespace Folke.Elm.Abstract.Test
{
    public class BaseIntegrationTestUpdate : BaseIntegrationTest, IIntegrationTestUpdate
    {
        public BaseIntegrationTestUpdate(IDatabaseDriver databaseDriver, string connectionString, bool drop) : base(databaseDriver, connectionString, drop)
        {
        }
        
        public void Update_Set()
        {
            // Arrange
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);

            // Act
            connection.Update<TestPoco>().Set(x => x.Name, x => "Test").Execute();

            // Assert
            var result = connection.Load<TestPoco>(newPoco.Id);
            Assert.Equal("Test", result.Name);
        }

        public void Update_ObjectWithGuid()
        {
            testValue.Text = "Brocoli";
            connection.Update(testValue);
            var result = connection.Get<TableWithGuid>(testValue.Id);
            Assert.Equal(testValue.Id, result.Id);
            Assert.Equal(testValue.Text, result.Text);
        }

        public async Task UpdateAsync_ObjectWithGuid()
        {
            testValue.Text = "Brocoli";
            await connection.UpdateAsync(testValue);
            var result = connection.Get<TableWithGuid>(testValue.Id);
            Assert.Equal(testValue.Id, result.Id);
            Assert.Equal(testValue.Text, result.Text);
        }
    }
}
