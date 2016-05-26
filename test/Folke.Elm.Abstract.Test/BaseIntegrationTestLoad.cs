using System;
using System.Threading.Tasks;
using Folke.Elm.Mapping;
using Xunit;
using System.Linq;

namespace Folke.Elm.Abstract.Test
{
    public class BaseIntegrationTestLoad : BaseIntegrationTest, IIntegrationTestLoad
    {
        public BaseIntegrationTestLoad(IDatabaseDriver databaseDriver, string connectionString, bool drop) : base(databaseDriver, connectionString, drop)
        {
        }

        public void Load_WithAutoJoin()
        {
            var onePoco = new TestPoco { Name = "One" };
            connection.Save(onePoco);
            var twoPoco = new TestPoco { Name = "Two" };
            connection.Save(twoPoco);
            var three = new TestManyPoco { Toto = "Three", Poco = onePoco };
            connection.Save(three);
            var all = new TestMultiPoco { Name = "All", One = onePoco, Three = three, Two = twoPoco };
            connection.Save(all);

            connection.Cache.Clear();

            var multi = connection.Load<TestMultiPoco>(all.Id, x => x.One, x => x.Two, x => x.Three);
            Assert.Equal(all.Name, multi.Name);
            Assert.Equal(onePoco.Name, multi.One.Name);
            Assert.Equal(twoPoco.Name, multi.Two.Name);
            Assert.Equal(three.Toto, multi.Three.Toto);
        }

        public void Load_ObjectWithCollection()
        {
            var collection = new TestCollection { Name = "Collection" };
            connection.Save(collection);
            for (var i = 0; i < 10; i++)
            {
                var member = new TestCollectionMember { Collection = collection, Name = "Member" + i };
                connection.Save(member);
            }

            connection.Cache.Clear();

            var coll = connection.Load<TestCollection>(collection.Id);
            Assert.Equal(collection.Name, coll.Name);
            Assert.Equal(10, coll.Members.Count);
            var j = 0;
            foreach (var member in coll.Members.OrderBy(x => x.Name))
            {
                Assert.Equal(coll, member.Collection);
                Assert.Equal("Member" + j++, member.Name);
            }
        }

        public void Load_ObjectWithGuid()
        {
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            connection.Save(value);
            var result = connection.Load<TableWithGuid>(value.Id);
            Assert.Equal(value.Id, result.Id);
            Assert.Equal(value.Text, result.Text);
        }

        public void Load_ObjectWithGuid_WithAutoJoin()
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
            Assert.Equal(parent.Key, result.Key);
            Assert.Equal(parent.Text, result.Text);
            Assert.Equal(value.Id, parent.Reference.Id);
            Assert.Equal(value.Text, parent.Reference.Text);
        }

        public void Get_ObjectWithGuid()
        {
            var result = connection.Get<TableWithGuid>(testValue.Id);
            Assert.Equal(testValue.Id, result.Id);
            Assert.Equal(testValue.Text, result.Text);
        }

        public async Task LoadAsync_ObjectWithGuid()
        {
            var result = await connection.LoadAsync<TableWithGuid>(testValue.Id);
            Assert.Equal(testValue.Id, result.Id);
            Assert.Equal(testValue.Text, result.Text);
        }

        public async Task GetAsync_ObjectWithGuid()
        {
            var result = await connection.GetAsync<TableWithGuid>(testValue.Id);
            Assert.Equal(testValue.Id, result.Id);
            Assert.Equal(testValue.Text, result.Text);
        }

    }
}
