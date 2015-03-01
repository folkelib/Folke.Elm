﻿using System.Collections.Generic;
using System.Configuration;
using NUnit.Framework;

namespace Folke.Orm.Mysql.Test
{
    [TestFixture]
    public class IntegrationTestWithFolkeTable
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

        private FolkeConnection connection;

        [SetUp]
        public void Initialize()
        {
            var driver = new MySqlDriver();
            connection = new FolkeConnection(driver, ConfigurationManager.ConnectionStrings["Test"].ConnectionString);
            connection.CreateTable<TestPoco>(drop: true);
            connection.CreateTable<TestManyPoco>(drop: true);
            connection.CreateTable<TestMultiPoco>(drop: true);
            connection.CreateTable<TestCollectionMember>(drop: true);
            connection.CreateTable<TestCollection>(drop: true);
        }

        [TearDown]
        public void Cleanup()
        {
            connection.DropTable<TestCollection>();
            connection.DropTable<TestCollectionMember>();
            connection.DropTable<TestMultiPoco>();
            connection.DropTable<TestPoco>();
            connection.DropTable<TestManyPoco>();
            connection.Dispose();
        }


        [Test]
        public void TestCreate()
        {
            connection.CreateTable<TestCreatePoco>();
            connection.DropTable<TestCreatePoco>();
        }

        [Test]
        public void TestSave()
        {
            var newPoco = new TestPoco { Name = "Tutu "};
            connection.Save(newPoco);
            Assert.AreNotEqual(0, newPoco.Id);
        }

        [Test]
        public void TestQuery()
        {
            var newPoco = new TestPoco { Name = "Tutu " };
            connection.Save(newPoco);
            var foundPoco = connection.QueryOver<TestPoco>().Where(t => t.Name == newPoco.Name).Single();
            Assert.AreEqual(newPoco.Name, foundPoco.Name);
        }

        [Test]
        public void TestBoolean()
        {
            var newPocoFalse = new TestPoco { Name = "Hihi" };
            connection.Save(newPocoFalse);
            var newPocoTrue = new TestPoco { Name = "Huhu", Boolean = true };
            connection.Save(newPocoTrue);

            var foundTrue = connection.QueryOver<TestPoco>().Where(t => t.Boolean).List();
            Assert.AreEqual(1, foundTrue.Count);
            Assert.AreEqual(newPocoTrue.Name, foundTrue[0].Name);
            var foundFalse = connection.QueryOver<TestPoco>().Where(t => !t.Boolean).List();
            Assert.AreEqual(1, foundFalse.Count);
            Assert.AreEqual(newPocoFalse.Name, foundFalse[0].Name);
        }

        [Test]
        public void TestIsNull()
        {
            var newPoco = new TestPoco { Name = null };
            connection.Save(newPoco);
            var foundPoco = connection.QueryOver<TestPoco>().Where(t => t.Name == null).Single();
            Assert.AreEqual(newPoco.Id, foundPoco.Id);
        }

        [Test]
        public void TestMany()
        {
            var newPoco = new TestPoco { Name = null };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);
            var manies = connection.QueryOver<TestManyPoco>().Where(t => t.Poco == newPoco).List();
            Assert.AreEqual(1, manies.Count);
            Assert.AreEqual(newPoco, manies[0].Poco);
        }

        [Test]
        public void TestSelectColumns()
        {
            var newPoco = new TestPoco { Name = "Ihihi" };
            connection.Save(newPoco);
            connection.Cache.Clear();
            var poco = connection.Query<TestPoco>().Select(x => x.Id, x => x.Name).From().List();
            Assert.AreEqual(newPoco.Id, poco[0].Id);
            Assert.AreEqual(newPoco.Name, poco[0].Name);
        }

        [Test]
        public void TestManyNoJoin()
        {
            var newPoco = new TestPoco { Name = null };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);

            connection.Cache.Clear();

            var manies = connection.QueryOver<TestManyPoco>().Where(t => t.Poco == newPoco).List();
            Assert.AreEqual(1, manies.Count);
            Assert.AreEqual(newPoco.Id, manies[0].Poco.Id);
        }

        [Test]
        public void TestManyNoJoinNameNotRetreived()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);

            connection.Cache.Clear();

            var manies = connection.QueryOver<TestManyPoco>().Where(t => t.Poco == newPoco).List();
            Assert.AreEqual(1, manies.Count);
            Assert.AreEqual(newPoco.Id, manies[0].Poco.Id);
            Assert.IsNull(manies[0].Poco.Name);
        }

        [Test]
        public void TestManyJoin()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);

            connection.Cache.Clear();

            var manies = connection.Query<TestManyPoco>().SelectAll().AndAll(x => x.Poco).From().LeftJoinOnId(x => x.Poco).Where(t => t.Toto == "Toto").List();
            Assert.AreEqual(1, manies.Count);
            Assert.AreEqual(newPoco.Id, manies[0].Poco.Id);
            Assert.AreEqual(newPoco.Name, manies[0].Poco.Name);
        }

        [Test]
        public void TestManyFetch()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);

            connection.Cache.Clear();

            var manies = connection.QueryOver<TestManyPoco>(t => t.Poco).Where(t => t.Toto == "Toto").List();
            Assert.AreEqual(1, manies.Count);
            Assert.AreEqual(newPoco.Id, manies[0].Poco.Id);
            Assert.AreEqual(newPoco.Name, manies[0].Poco.Name);
        }

        private class AnonymousType
        {
            public TestPoco Poco { get; set; }
            public TestManyPoco Many { get; set; }
        }

        [Test]
        public void TestAnonymous()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);
            
            var manies = connection.Query<AnonymousType>().SelectAll(x => x.Poco).AndAll(x => x.Many).From(x => x.Many)
                .LeftJoin(x => x.Poco).On(x => x.Many.Poco, x => x.Poco).List();
            Assert.AreEqual(newPoco.Name, manies[0].Poco.Name);
            Assert.AreEqual(newMany.Toto, manies[0].Many.Toto);
        }
        
        [Test]
        public void TestAnonymousWithCriteria()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);
            var newMany2 = new TestManyPoco { Toto = "Tutu", Poco = newPoco };
            connection.Save(newMany2);

            connection.Cache.Clear();

            var manies = connection.Query<AnonymousType>().SelectAll(x => x.Poco).AndAll(x => x.Many).From(x => x.Poco)
                .LeftJoin(x => x.Many).On(x => x.Many.Poco, x => x.Poco).AndOn(x => x.Many.Toto == "Toto").List();
            Assert.AreEqual(1, manies.Count);
            Assert.AreEqual(newPoco.Name, manies[0].Poco.Name);
            Assert.AreEqual(newMany.Toto, manies[0].Many.Toto);
        }

        [Test]
        public void TestAnonymousWithCriteria2()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);
            var newMany2 = new TestManyPoco { Toto = "Tutu", Poco = newPoco };
            connection.Save(newMany2);

            connection.Cache.Clear();

            var manies = connection.Query<AnonymousType>().SelectAll(x => x.Poco).AndAll(x => x.Many).From(x => x.Poco)
                .LeftJoin(x => x.Many).On(x => x.Many.Poco, x => x.Poco).AndOn(x => x.Many.Toto == "Titi").OrderBy(x => x.Poco.Name).List();
            Assert.AreEqual(1, manies.Count);
            Assert.AreEqual(newPoco.Name, manies[0].Poco.Name);
            Assert.AreEqual(null, manies[0].Many);
        }

        [Test]
        public void TestLimitAndOrder()
        {
            for (var i = 0; i<10; i++)
            {
                var newPoco = new TestPoco { Name = "Name" + i };
                connection.Save(newPoco);
            }
            var pocos = connection.QueryOver<TestPoco>().OrderBy(x => x.Name).Desc().Limit(1, 2).List();
            Assert.AreEqual(2, pocos.Count);
            Assert.AreEqual("Name8", pocos[0].Name);
            Assert.AreEqual("Name7", pocos[1].Name);
        }

        [Test]
        public void TestSubquery()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);
            var otherPoco = new TestPoco { Name = "OtherName" };
            connection.Save(otherPoco);
            
            TestManyPoco manyPoco = null;
            var pocos = connection.QueryOver<TestPoco>().Where().Exists().BeginSub().SelectAll(() => manyPoco).From(() => manyPoco).Where(x => manyPoco.Poco == x).EndSub().List();
            Assert.AreEqual(1, pocos.Count);
            Assert.AreEqual(newPoco.Name, pocos[0].Name);
        }

        [Test]
        public void TestLoadWithFetch()
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
            Assert.AreEqual(all.Name, multi.Name);
            Assert.AreEqual(onePoco.Name, multi.One.Name);
            Assert.AreEqual(twoPoco.Name, multi.Two.Name);
            Assert.AreEqual(three.Toto, multi.Three.Toto);
        }

        [Test]
        public void TestPrepare()
        {
            var onePoco = new TestPoco { Name = "One" };
            connection.Save(onePoco);
            var twoPoco = new TestPoco { Name = "Two" };
            connection.Save(twoPoco);

            var query = new PreparedQueryBuilder<TestPoco, string>(q => q.SelectAll().From().Where((x, y) => x.Name == y.Item0));
            var result = query.List(connection, "Two");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Two", result[0].Name);
            result = query.List(connection, "One");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("One", result[0].Name);
        }

        private PreparedQueryBuilder<TestPoco, string> staticQuery = new PreparedQueryBuilder<TestPoco, string>(q => q.SelectAll().From().Where((x, y) => x.Name == y.Item0));

        [Test]
        public void TestPrepareStatic()
        {
            var onePoco = new TestPoco { Name = "One" };
            connection.Save(onePoco);
            var twoPoco = new TestPoco { Name = "Two" };
            connection.Save(twoPoco);

            var result = staticQuery.List(connection, "Two");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Two", result[0].Name);
            result = staticQuery.List(connection, "One");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("One", result[0].Name);
        }

        [Test]
        public void TestLike()
        {
            var onePoco = new TestPoco { Name = "One" };
            connection.Save(onePoco);
            var twoPoco = new TestPoco { Name = "Two" };
            connection.Save(twoPoco);

            var result = connection.QueryOver<TestPoco>().Where(x => x.Name.Like("On%")).List();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("One", result[0].Name);
        }

        [Test]
        public void TestCollectionAuto()
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
            Assert.AreEqual(collection.Name, coll.Name);
            Assert.AreEqual(10, coll.Members.Count);
            var j = 0;
            foreach (var member in coll.Members)
            {
                Assert.AreEqual(coll, member.Collection);
                Assert.AreEqual("Member" + j++, member.Name);
            }
        }

        class Group : IFolkeTable
        {
            public int Id { get; set; }
        }
        
        class User : IFolkeTable
        {
            public int Id { get; set; }
        }

        class UserInGroup : IFolkeTable
        {
            public int Id { get; set; }
            public Group Group { get; set; }
            public User User { get; set; }
        }

       /* [Test]
        public void TestSelectFromSubQuery()
        {
            connection.Query<UserInGroup>().Select(u => u.Group).FromSubQuery<UserInGroup>(x => x.Select(x => x.)
        }*/

        [Test]
        public void TestFromSubQuery()
        {
            var query = connection.Query<UserInGroup>().Select(x => x.Group).FromSubQuery(q => q.Select(x => x.Group).From().Where(x => x.User.Id == 1).GroupBy(x => x.Group));
            Assert.AreEqual("SELECT  t.`Group_id` FROM (SELECT  t.`Group_id` FROM `UserInGroup` as t WHERE( t.`User_id`=@Item0) GROUP BY  t.`Group_id`) AS t", query.Sql);
        }

        [Test]
        public void TestInnerJoinSubQuery()
        {
            UserInGroup a = null;
            var query = connection.Query<UserInGroup>().Select(x => x.Group).FromSubQuery(q => q.Select(x => x.Group).From().Where(x => x.User.Id == 1).GroupBy(x => x.Group))
               .InnerJoinSubQuery(q => q.Select(x => x.Group).From().Where(x => x.User.Id == 2), () => a).On(x => a.Group, x => x.Group);
            Assert.AreEqual("SELECT  t.`Group_id` FROM (SELECT  t.`Group_id` FROM `UserInGroup` as t WHERE( t.`User_id`=@Item0) GROUP BY  t.`Group_id`) AS t  INNER JOIN (SELECT  t.`Group_id` FROM `UserInGroup` as t WHERE( t.`User_id`=@Item1)) AS t1 ON  t1.`Group_id`= t.`Group_id`", query.Sql);
        }

        [Test]
        public void SelectOneColumnInTableWithForeignKeys()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);

            connection.Cache.Clear();

            var response = connection.Query<TestManyPoco>().Select(x => x.Toto).From().List();
            Assert.AreEqual(newMany.Toto, response[0].Toto);
        }

        [Test]
        public void SelectOneColumnAndIdInTableWithForeignKeys()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);

            connection.Cache.Clear();

            var response = connection.Query<TestManyPoco>().Select(x => x.Id).Select(x => x.Toto).From().List();
            Assert.AreEqual(newMany.Toto, response[0].Toto);
        }
    }
}