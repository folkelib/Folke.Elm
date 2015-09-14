using System;
using System.Collections.Generic;
using Folke.Orm.Mapping;
using Folke.Orm.Fluent;
using Xunit;

namespace Folke.Orm.Mysql.Test
{
    [Collection("Integration tests")]
    public class IntegrationTestWithFolkeTable : IDisposable
    {
        public class TestPoco : IFolkeTable
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool Boolean { get; set; }
        }

        public class TestManyPoco : IFolkeTable
        {
            public int Id { get; set; }
            public string Toto { get; set; }
            public TestPoco Poco { get; set; }
        }

        public class TestMultiPoco : IFolkeTable
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public TestPoco One { get; set; }
            public TestPoco Two { get; set; }
            public TestManyPoco Three { get; set; }
        }

        public class TestCollectionMember : IFolkeTable
        {
            public int Id {get;set;}
            public TestCollection Collection {get;set;}
            public string Name {get;set;}
        }

        public class TestCollection : IFolkeTable
        {
            public int Id { get; set; }
            public IReadOnlyList<TestCollectionMember> Members { get; set; }
            public string Name { get; set; }
        }

        public class TestCreatePoco : IFolkeTable
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class AnonymousType
        {
            public TestPoco Poco { get; set; }
            public TestManyPoco Many { get; set; }
        }
        
        public class Group : IFolkeTable
        {
            public int Id { get; set; }
        }

        public class User : IFolkeTable
        {
            public int Id { get; set; }
        }

        public class UserInGroup : IFolkeTable
        {
            public int Id { get; set; }
            public Group Group { get; set; }
            public User User { get; set; }
        }

        private FolkeConnection connection;

        public IntegrationTestWithFolkeTable()
        {
            var driver = new MySqlDriver();
            var mapper = new Mapper();
            connection = new FolkeConnection(driver, mapper, TestHelpers.ConnectionString);
            connection.CreateTable<TestPoco>(drop: true);
            connection.CreateTable<TestManyPoco>(drop: true);
            connection.CreateTable<TestMultiPoco>(drop: true);
            connection.CreateTable<TestCollectionMember>(drop: true);
            connection.CreateTable<TestCollection>(drop: true);
        }

        public void Dispose()
        {
            connection.DropTable<TestCollection>();
            connection.DropTable<TestCollectionMember>();
            connection.DropTable<TestMultiPoco>();
            connection.DropTable<TestPoco>();
            connection.DropTable<TestManyPoco>();
            connection.Dispose();
        }


        [Fact]
        public void Create()
        {
            connection.CreateTable<TestCreatePoco>();
            connection.DropTable<TestCreatePoco>();
        }

        [Fact]
        public void Save()
        {
            var newPoco = new TestPoco { Name = "Tutu "};
            connection.Save(newPoco);
            Assert.NotEqual(0, newPoco.Id);
        }

        [Fact]
        public void Query()
        {
            var newPoco = new TestPoco { Name = "Tutu " };
            connection.Save(newPoco);
            var foundPoco = connection.QueryOver<TestPoco>().Where(t => t.Name == newPoco.Name).Single();
            Assert.Equal(newPoco.Name, foundPoco.Name);
        }

        [Fact]
        public void Boolean()
        {
            var newPocoFalse = new TestPoco { Name = "Hihi" };
            connection.Save(newPocoFalse);
            var newPocoTrue = new TestPoco { Name = "Huhu", Boolean = true };
            connection.Save(newPocoTrue);

            var foundTrue = connection.QueryOver<TestPoco>().Where(t => t.Boolean).List();
            Assert.Equal(1, foundTrue.Count);
            Assert.Equal(newPocoTrue.Name, foundTrue[0].Name);
            var foundFalse = connection.QueryOver<TestPoco>().Where(t => !t.Boolean).List();
            Assert.Equal(1, foundFalse.Count);
            Assert.Equal(newPocoFalse.Name, foundFalse[0].Name);
        }

        [Fact]
        public void IsNull()
        {
            var newPoco = new TestPoco { Name = null };
            connection.Save(newPoco);
            var foundPoco = connection.QueryOver<TestPoco>().Where(t => t.Name == null).Single();
            Assert.Equal(newPoco.Id, foundPoco.Id);
        }

        [Fact]
        public void Many()
        {
            var newPoco = new TestPoco { Name = null };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);
            var manies = connection.QueryOver<TestManyPoco>().Where(t => t.Poco == newPoco).List();
            Assert.Equal(1, manies.Count);
            Assert.Equal(newPoco, manies[0].Poco);
        }

        [Fact]
        public void Select_MultipleColumns()
        {
            var newPoco = new TestPoco { Name = "Ihihi" };
            connection.Save(newPoco);
            connection.Cache.Clear();
            var poco = connection.Query<TestPoco>().Values(x => x.Id, x => x.Name).From().List();
            Assert.Equal(newPoco.Id, poco[0].Id);
            Assert.Equal(newPoco.Name, poco[0].Name);
        }

        [Fact]
        public void ManyNoJoin()
        {
            var newPoco = new TestPoco { Name = null };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);

            connection.Cache.Clear();

            var manies = connection.QueryOver<TestManyPoco>().Where(t => t.Poco == newPoco).List();
            Assert.Equal(1, manies.Count);
            Assert.Equal(newPoco.Id, manies[0].Poco.Id);
        }

        [Fact]
        public void ManyNoJoinNameNotRetreived()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);

            connection.Cache.Clear();

            var manies = connection.QueryOver<TestManyPoco>().Where(t => t.Poco == newPoco).List();
            Assert.Equal(1, manies.Count);
            Assert.Equal(newPoco.Id, manies[0].Poco.Id);
            Assert.Null(manies[0].Poco.Name);
        }

        [Fact]
        public void ManyJoin()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);

            connection.Cache.Clear();

            var manies = connection.Query<TestManyPoco>().All().All(x => x.Poco).From().LeftJoinOnId(x => x.Poco).Where(t => t.Toto == "Toto").List();
            Assert.Equal(1, manies.Count);
            Assert.Equal(newPoco.Id, manies[0].Poco.Id);
            Assert.Equal(newPoco.Name, manies[0].Poco.Name);
        }

        [Fact]
        public void ManyFetch()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);

            connection.Cache.Clear();

            var manies = connection.QueryOver<TestManyPoco>(t => t.Poco).Where(t => t.Toto == "Toto").List();
            Assert.Equal(1, manies.Count);
            Assert.Equal(newPoco.Id, manies[0].Poco.Id);
            Assert.Equal(newPoco.Name, manies[0].Poco.Name);
        }

        [Fact]
        public void Anonymous()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);
            
            var manies = connection.Query<AnonymousType>().All(x => x.Poco).All(x => x.Many).From(x => x.Many)
                .LeftJoin(x => x.Poco).On(x => x.Many.Poco == x.Poco).List();
            Assert.Equal(newPoco.Name, manies[0].Poco.Name);
            Assert.Equal(newMany.Toto, manies[0].Many.Toto);
        }
        
        [Fact]
        public void AnonymousWithCriteria()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);
            var newMany2 = new TestManyPoco { Toto = "Tutu", Poco = newPoco };
            connection.Save(newMany2);

            connection.Cache.Clear();

            var manies = connection.Query<AnonymousType>().All(x => x.Poco).All(x => x.Many).From(x => x.Poco)
                .LeftJoin(x => x.Many).On(x => x.Many.Poco == x.Poco).AndOn(x => x.Many.Toto == "Toto").List();
            Assert.Equal(1, manies.Count);
            Assert.Equal(newPoco.Name, manies[0].Poco.Name);
            Assert.Equal(newMany.Toto, manies[0].Many.Toto);
        }

        [Fact]
        public void AnonymousWithCriteria2()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);
            var newMany2 = new TestManyPoco { Toto = "Tutu", Poco = newPoco };
            connection.Save(newMany2);

            connection.Cache.Clear();

            var manies = connection.Query<AnonymousType>().All(x => x.Poco).All(x => x.Many).From(x => x.Poco)
                .LeftJoin(x => x.Many).On(x => x.Many.Poco == x.Poco).AndOn(x => x.Many.Toto == "Titi").OrderBy(x => x.Poco.Name).List();
            Assert.Equal(1, manies.Count);
            Assert.Equal(newPoco.Name, manies[0].Poco.Name);
            Assert.Equal(null, manies[0].Many);
        }

        [Fact]
        public void LimitAndOrder()
        {
            for (var i = 0; i < 10; i++)
            {
                var newPoco = new TestPoco { Name = "Name" + i };
                connection.Save(newPoco);
            }

            var pocos = connection.QueryOver<TestPoco>().OrderBy(x => x.Name).Desc().Limit(1, 2).List();
            Assert.Equal(2, pocos.Count);
            Assert.Equal("Name8", pocos[0].Name);
            Assert.Equal("Name7", pocos[1].Name);
        }

        [Fact]
        public void Subquery()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);
            var otherPoco = new TestPoco { Name = "OtherName" };
            connection.Save(otherPoco);

            var pocos = connection.Query<FolkeTuple<TestPoco, TestManyPoco>>().All(x => x.Item0).From(x => x.Item0)
                .WhereExists(sub => sub.All(x => x.Item1).From(x => x.Item1).Where(x => x.Item1.Poco == x.Item0)).List();
            Assert.Equal(1, pocos.Count);
            Assert.Equal(newPoco.Name, pocos[0].Item0.Name);
        }

        [Fact]
        public void AndFrom()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);
            var pocos =
                connection.Query<FolkeTuple<TestPoco, TestManyPoco>>()
                    .All(x => x.Item0)
                    .All(x => x.Item1)
                    .From(x => x.Item0)
                    .From(x => x.Item1).List();
            Assert.Equal(1, pocos.Count);
            Assert.Equal(newPoco.Name, pocos[0].Item0.Name);
            Assert.Equal(newMany.Toto, pocos[0].Item1.Toto);
            Assert.Equal(newPoco, pocos[0].Item1.Poco);
        }

        [Fact]
        public void RightJoin()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);
            var otherMany = new TestManyPoco {Toto = "OtherToto", Poco = newPoco};
            connection.Save(otherMany);
            var pocos =
                connection.Query<FolkeTuple<TestPoco, TestManyPoco>>()
                    .All(x => x.Item0)
                    .All(x => x.Item1)
                    .From(x => x.Item0)
                    .RightJoin(x => x.Item1)
                    .On(x => x.Item1.Poco == x.Item0)
                    .List();
            Assert.Equal(2, pocos.Count);
            Assert.Equal(newPoco.Name, pocos[0].Item0.Name);
            Assert.Equal(newMany.Toto, pocos[0].Item1.Toto);
            Assert.Equal(newPoco, pocos[0].Item1.Poco);
            Assert.Equal(newPoco.Name, pocos[1].Item0.Name);
            Assert.Equal(otherMany.Toto, pocos[1].Item1.Toto);
            Assert.Equal(newPoco, pocos[1].Item1.Poco);
        }

        [Fact]
        public void InnerJoin()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);
            var otherMany = new TestManyPoco { Toto = "OtherToto" };
            connection.Save(otherMany);
            var pocos =
                connection.Query<TestManyPoco>()
                    .All()
                    .All(x => x.Poco)
                    .From()
                    .InnerJoin(x => x.Poco).OnId(x => x.Poco).List();
            Assert.Equal(1, pocos.Count);
            Assert.Equal(newPoco.Name, pocos[0].Poco.Name);
            Assert.Equal(newMany.Toto, pocos[0].Toto);
        }

        [Fact]
        public void LoadWithFetch()
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

        [Fact]
        public void Prepare()
        {
            var onePoco = new TestPoco { Name = "One" };
            connection.Save(onePoco);
            var twoPoco = new TestPoco { Name = "Two" };
            connection.Save(twoPoco);

            var query = new PreparedQueryBuilder<TestPoco, string>(q => q.All().From().Where((x, y) => x.Name == y.Item0));
            var result = query.List(connection, "Two");
            Assert.Equal(1, result.Count);
            Assert.Equal("Two", result[0].Name);
            result = query.List(connection, "One");
            Assert.Equal(1, result.Count);
            Assert.Equal("One", result[0].Name);
        }

        private readonly PreparedQueryBuilder<TestPoco, string> staticQuery = new PreparedQueryBuilder<TestPoco, string>(q => q.All().From().Where((x, y) => x.Name == y.Item0));

        [Fact]
        public void PrepareStatic()
        {
            var onePoco = new TestPoco { Name = "One" };
            connection.Save(onePoco);
            var twoPoco = new TestPoco { Name = "Two" };
            connection.Save(twoPoco);

            var result = staticQuery.List(connection, "Two");
            Assert.Equal(1, result.Count);
            Assert.Equal("Two", result[0].Name);
            result = staticQuery.List(connection, "One");
            Assert.Equal(1, result.Count);
            Assert.Equal("One", result[0].Name);
        }

        [Fact]
        public void Like()
        {
            var onePoco = new TestPoco { Name = "One" };
            connection.Save(onePoco);
            var twoPoco = new TestPoco { Name = "Two" };
            connection.Save(twoPoco);

            var result = connection.QueryOver<TestPoco>().Where(x => x.Name.Like("On%")).List();
            Assert.Equal(1, result.Count);
            Assert.Equal("One", result[0].Name);
        }

        [Fact]
        public void CollectionAuto()
        {
            var collection = new TestCollection { Name = "Collection" };
            connection.Save(collection);
            for (var i = 0; i< 10; i++)
            {
                var member = new TestCollectionMember { Collection = collection, Name = "Member" + i };
                connection.Save(member);
            }

            connection.Cache.Clear();

            var coll = connection.Load<TestCollection>(collection.Id);
            Assert.Equal(collection.Name, coll.Name);
            Assert.Equal(10, coll.Members.Count);
            var j = 0;
            foreach (var member in coll.Members)
            {
                Assert.Equal(coll, member.Collection);
                Assert.Equal("Member" + j++, member.Name);
            }
        }

        [Fact]
        public void FromSubQuery()
        {
            var query = connection.Query<UserInGroup>().Values(x => x.Group).FromSubQuery(q => q.Values(x => x.Group).From().Where(x => x.User.Id == 1).GroupBy(x => x.Group));
            Assert.Equal("SELECT `t`.`Group_id` FROM (SELECT `t`.`Group_id` FROM `UserInGroup` as t WHERE( `t`.`User_id`= @Item0) GROUP BY  `t`.`Group_id`) AS t", query.QueryBuilder.Sql);
        }

        [Fact]
        public void InnerJoinSubQuery()
        {
            UserInGroup a = null;
            var query = connection.Query<UserInGroup>().Values(x => x.Group).FromSubQuery(q => q.Values(x => x.Group).From().Where(x => x.User.Id == 1).GroupBy(x => x.Group))
               .InnerJoinSubQuery(q => q.Values(x => x.Group).From().Where(x => x.User.Id == 2), () => a).On(x => a.Group == x.Group);
            Assert.Equal("SELECT `t`.`Group_id` FROM (SELECT `t`.`Group_id` FROM `UserInGroup` as t WHERE( `t`.`User_id`= @Item0) GROUP BY  `t`.`Group_id`) AS t INNER JOIN (SELECT `t`.`Group_id` FROM `UserInGroup` as t WHERE( `t`.`User_id`= @Item1)) AS t1 ON ( `t1`.`Group_id`= `t`.`Group_id`)", query.QueryBuilder.Sql);
        }

        [Fact]
        public void SelectOneColumnInTableWithForeignKeys()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);

            connection.Cache.Clear();

            var response = connection.Query<TestManyPoco>().Values(x => x.Toto).From().List();
            Assert.Equal(newMany.Toto, response[0].Toto);
        }

        [Fact]
        public void SelectOneColumnAndIdInTableWithForeignKeys()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);

            connection.Cache.Clear();

            var response = connection.Query<TestManyPoco>().Values(x => x.Id).Values(x => x.Toto).From().List();
            Assert.Equal(newMany.Toto, response[0].Toto);
        }

        [Fact]
        public void Set()
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

        [Fact]
        public void LimitWithVariable()
        {
            // Arrange
            for (var i = 0; i < 10; i++)
            {
                var newPoco = new TestPoco { Name = "Name" + i };
                connection.Save(newPoco);
            }

            // Act
            var results = connection.QueryOver<TestPoco>().OrderBy(x => x.Name).Asc().Limit(x => 5, 5).List();

            // Assert
            Assert.Equal(5, results.Count);
            Assert.Equal("Name5", results[0].Name);
        }

        [Fact]
        public void LimitWithParameter()
        {
            // Arrange
            for (var i = 0; i < 10; i++)
            {
                var newPoco = new TestPoco { Name = "Name" + i };
                connection.Save(newPoco);
            }

            // Act
            var results =
                new FluentSelectBuilder<TestPoco, FolkeTuple<int>>(connection.Driver, connection.Mapper).All()
                    .From()
                    .OrderBy(x => x.Name)
                    .Limit((x, y) => y.Item0, 5).List(connection, 5);
            
            // Assert
            Assert.Equal(5, results.Count);
            Assert.Equal("Name5", results[0].Name);
        }

        [Fact]
        public void SelectCountAll()
        {
            // Arrange
            for (var i = 0; i < 10; i++)
            {
                var newPoco = new TestPoco { Name = "Name" + i };
                connection.Save(newPoco);
            }

            // Act
            var result = connection.Query<TestPoco>().CountAll().From().Scalar<int>();

            // Assert
            Assert.Equal(10, result);
        }

        [Fact]
        public void TestLinq()
        {
            var newPoco = new TestPoco { Name = Guid.NewGuid().ToString() };
            connection.Save(newPoco);
            var otherPoco = new TestPoco {Name = Guid.NewGuid().ToString()};
            connection.Save(otherPoco);

            FluentWhereBuilder<TestPoco, FolkeTuple> query = from toto in connection.QueryOver<TestPoco>() where toto.Name == newPoco.Name select toto;
            var result = query.List();
            Assert.Equal(1, result.Count);
            Assert.Equal(newPoco.Name, result[0].Name);
        }

/*        [Fact]
        public void TestLinqWithJoin()
        {
            var newPoco = new TestPoco { Name = Guid.NewGuid().ToString() };
            connection.Save(newPoco);
            var linked = new TestManyPoco {Poco = newPoco, Toto = Guid.NewGuid().ToString()};
            connection.Save(linked);

            FluentWhereBuilder<TestPoco, FolkeTuple> query = from toto in connection.QueryOver<TestPoco>() 
                                                             join link in toto.
                                                             where toto.Name == newPoco.Name select toto;
            var result = query.List();
            Assert.Equal(1, result.Count);
            Assert.Equal(newPoco.Name, result[0].Name);
        }*/
    }
}
