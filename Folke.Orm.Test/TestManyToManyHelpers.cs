﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Orm.Test
{
    [TestFixture]
    public class TestManyToManyHelpers
    {
        private class ParentClass : IFolkeTable
        {
            public int Id { get; set; }

            [FolkeList(Join="Child")]
            public IReadOnlyList<LinkClass> Children { get; set; }
        }

        private class ChildClass : IFolkeTable
        {
            public int Id { get; set; }
            public string Test { get; set; }
        }

        private class ChildDto : IFolkeTable
        {
            public int Id { get; set; }
            public string Test { get; set; }
        }

        private class LinkClass : IManyToManyTable<ParentClass, ChildClass>
        {
            public ParentClass Parent { get; set; }

            public ChildClass Child { get; set; }

            public int Id { get; set; }
        }

        private IFolkeConnection connection;
        private Func<ChildDto, ChildClass> mapper = dto => new ChildClass { Test = dto.Test };
        private ParentClass parent;

        [SetUp]
        public void Initialize()
        {
            var driver = new MySqlDriver(new DatabaseSettings { Database = "folkeormtest", Host = "localhost", Password = "test", User = "test" });
            connection = new FolkeConnection(driver);
            connection.CreateOrUpdateTable<ParentClass>();
            connection.CreateOrUpdateTable<ChildClass>();
            connection.CreateOrUpdateTable<LinkClass>();
        }
        
        [TearDown]
        public void Cleanup()
        {
            connection.DropTable<LinkClass>();
            connection.DropTable<ParentClass>();
            connection.DropTable<ChildClass>();
        }

        [Test]
        public void UpdateManyToMany_NoExisting_AddedElements()
        {
            using (var transaction = connection.BeginTransaction())
            {
                parent = new ParentClass();
                connection.Save(parent);

                var newChildren = new[] { new ChildDto { Test = "First" }, new ChildDto { Test = "Second" } };
                connection.UpdateManyToMany(parent, parent.Children, newChildren, mapper);

                connection.Cache.Clear();

                var newParent = connection.Load<ParentClass>(parent.Id);

                Assert.AreEqual(2, newParent.Children.Count);
                Assert.IsTrue(newParent.Children.Any(c => c.Child.Test == "First"));
                Assert.IsTrue(newParent.Children.Any(c => c.Child.Test == "Second"));
            }
        }

        [Test]
        public void UpdateManyToMany_TwoExistingElement_AddOneElement()
        {
            using (var transaction = connection.BeginTransaction())
            {
                parent = new ParentClass();
                connection.Save(parent);

                var newChildren = new[] { new ChildDto { Test = "First" }, new ChildDto { Test = "Second" } };
                connection.UpdateManyToMany(parent, parent.Children, newChildren, mapper);

                connection.Cache.Clear();

                var first = connection.QueryOver<ChildClass>().Where(c => c.Test == "First").Single();
                var second = connection.QueryOver<ChildClass>().Where(c => c.Test == "Second").Single();

                var newParent = connection.Load<ParentClass>(parent.Id);
                var modifiedChildren = new[] { new ChildDto { Test = "First", Id = first.Id }, new ChildDto { Test = "Second", Id = second.Id }, new ChildDto { Test = "Third" } };
                connection.UpdateManyToMany(newParent, newParent.Children, modifiedChildren, mapper);

                connection.Cache.Clear();

                var final = connection.Load<ParentClass>(parent.Id);
                Assert.AreEqual(3, final.Children.Count);
                Assert.IsTrue(final.Children.Any(c => c.Child.Test == "First"));
                Assert.IsTrue(final.Children.Any(c => c.Child.Test == "Second"));
                Assert.IsTrue(final.Children.Any(c => c.Child.Test == "Third"));
            }
        }

        [Test]
        public void UpdateManyToMany_TwoExistingElement_RemoveOneElement()
        {
            using (var transaction = connection.BeginTransaction())
            {
                parent = new ParentClass();
                connection.Save(parent);

                var newChildren = new[] { new ChildDto { Test = "First" }, new ChildDto { Test = "Second" } };
                connection.UpdateManyToMany(parent, parent.Children, newChildren, mapper);

                connection.Cache.Clear();

                var first = connection.QueryOver<ChildClass>().Where(c => c.Test == "First").Single();
                var second = connection.QueryOver<ChildClass>().Where(c => c.Test == "Second").Single();

                var newParent = connection.Load<ParentClass>(parent.Id);
                var modifiedChildren = new[] { new ChildDto { Test = "Second", Id = second.Id } };
                connection.UpdateManyToMany(newParent, newParent.Children, modifiedChildren, mapper);

                connection.Cache.Clear();

                var final = connection.Load<ParentClass>(parent.Id);
                Assert.AreEqual(1, final.Children.Count);
                Assert.IsTrue(final.Children.Any(c => c.Child.Test == "Second"));
            }
        }

        [Test]
        public void UpdateManyToMany_TwoExistingElement_RemoveAndOneElement()
        {
            using (var transaction = connection.BeginTransaction())
            {
                parent = new ParentClass();
                connection.Save(parent);

                var newChildren = new[] { new ChildDto { Test = "First" }, new ChildDto { Test = "Second" } };
                connection.UpdateManyToMany(parent, parent.Children, newChildren, mapper);

                connection.Cache.Clear();

                var first = connection.QueryOver<ChildClass>().Where(c => c.Test == "First").Single();
                var second = connection.QueryOver<ChildClass>().Where(c => c.Test == "Second").Single();

                var newParent = connection.Load<ParentClass>(parent.Id);
                var modifiedChildren = new[] { new ChildDto { Test = "Second", Id = second.Id }, new ChildDto { Test = "Third" } };
                connection.UpdateManyToMany(newParent, newParent.Children, modifiedChildren, mapper);

                connection.Cache.Clear();

                var final = connection.Load<ParentClass>(parent.Id);
                Assert.AreEqual(2, final.Children.Count);
                Assert.IsTrue(final.Children.Any(c => c.Child.Test == "Second"));
                Assert.IsTrue(final.Children.Any(c => c.Child.Test == "Third"));
            }
        }
    }
}
