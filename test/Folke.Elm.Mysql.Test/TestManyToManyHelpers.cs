using System;
using System.Collections.Generic;
using System.Linq;
using Folke.Elm.Mapping;
using Xunit;
using Folke.Elm.Fluent;

namespace Folke.Elm.Mysql.Test
{
    [Collection("IntegrationTest")]
    public class TestManyToManyHelpers : IDisposable
    {
        public class ParentClass : IFolkeTable
        {
            public int Id { get; set; }

            [Select(IncludeReference="Child")]
            public IReadOnlyList<LinkClass> Children { get; set; }
        }

        public class ChildClass : IFolkeTable
        {
            public int Id { get; set; }
            public string Test { get; set; }
        }

        public class ChildDto : IFolkeTable
        {
            public int Id { get; set; }
            public string Test { get; set; }
        }

        public class LinkClass : IManyToManyTable<ParentClass, ChildClass>
        {
            public ParentClass Parent { get; set; }

            public ChildClass Child { get; set; }

            public int Id { get; set; }
        }

        private readonly IFolkeConnection connection;
        private readonly Func<ChildDto, ChildClass> mapper = dto => new ChildClass { Test = dto.Test };
        private ParentClass parent;

        public TestManyToManyHelpers()
        {
            var driver = new MySqlDriver();
            var newMapper = new Mapper();
            connection = FolkeConnection.Create(driver, newMapper, TestHelpers.ConnectionString);
            connection.CreateOrUpdateTable<ParentClass>();
            connection.CreateOrUpdateTable<ChildClass>();
            connection.CreateOrUpdateTable<LinkClass>();
        }
        
        public void Dispose()
        {
            connection.DropTable<LinkClass>();
            connection.DropTable<ParentClass>();
            connection.DropTable<ChildClass>();
            connection.Dispose();
        }

        [Fact]
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

                Assert.Equal(2, newParent.Children.Count);
                var children = newParent.Children;
                Assert.True(children.Any(c => c.Child.Test == "First"));
                Assert.True(children.Any(c => c.Child.Test == "Second"));
                transaction.Rollback();
            }
        }

        [Fact]
        public void UpdateManyToMany_TwoExistingElement_AddOneElement()
        {
            using (var transaction = connection.BeginTransaction())
            {
                parent = new ParentClass();
                connection.Save(parent);

                var newChildren = new[] { new ChildDto { Test = "First" }, new ChildDto { Test = "Second" } };
                connection.UpdateManyToMany(parent, parent.Children, newChildren, mapper);

                connection.Cache.Clear();

                var first = connection.SelectAllFrom<ChildClass>().Where(c => c.Test == "First").Single();
                var second = connection.SelectAllFrom<ChildClass>().Where(c => c.Test == "Second").Single();

                var newParent = connection.Load<ParentClass>(parent.Id);
                var modifiedChildren = new[] { new ChildDto { Test = "First", Id = first.Id }, new ChildDto { Test = "Second", Id = second.Id }, new ChildDto { Test = "Third" } };
                connection.UpdateManyToMany(newParent, newParent.Children, modifiedChildren, mapper);

                connection.Cache.Clear();

                var final = connection.Load<ParentClass>(parent.Id);
                Assert.Equal(3, final.Children.Count);
                Assert.True(final.Children.Any(c => c.Child.Test == "First"));
                Assert.True(final.Children.Any(c => c.Child.Test == "Second"));
                Assert.True(final.Children.Any(c => c.Child.Test == "Third"));
                transaction.Rollback();
            }
        }

        [Fact]
        public void UpdateManyToMany_TwoExistingElement_RemoveOneElement()
        {
            using (var transaction = connection.BeginTransaction())
            {
                parent = new ParentClass();
                connection.Save(parent);

                var newChildren = new[] { new ChildDto { Test = "First" }, new ChildDto { Test = "Second" } };
                connection.UpdateManyToMany(parent, parent.Children, newChildren, mapper);

                connection.Cache.Clear();

                var first = connection.SelectAllFrom<ChildClass>().Where(c => c.Test == "First").Single();
                var second = connection.SelectAllFrom<ChildClass>().Where(c => c.Test == "Second").Single();

                var newParent = connection.Load<ParentClass>(parent.Id);
                var modifiedChildren = new[] { new ChildDto { Test = "Second", Id = second.Id } };
                connection.UpdateManyToMany(newParent, newParent.Children, modifiedChildren, mapper);

                connection.Cache.Clear();

                Assert.NotNull(first);
                var final = connection.Load<ParentClass>(parent.Id);
                Assert.Equal(1, final.Children.Count);
                Assert.True(final.Children.Any(c => c.Child.Test == "Second"));
                transaction.Rollback();
            }
        }

        [Fact]
        public void UpdateManyToMany_TwoExistingElement_RemoveAndOneElement()
        {
            using (var transaction = connection.BeginTransaction())
            {
                parent = new ParentClass();
                connection.Save(parent);

                var newChildren = new[] { new ChildDto { Test = "First" }, new ChildDto { Test = "Second" } };
                connection.UpdateManyToMany(parent, parent.Children, newChildren, mapper);

                connection.Cache.Clear();

                var first = connection.SelectAllFrom<ChildClass>().Where(c => c.Test == "First").Single();
                var second = connection.SelectAllFrom<ChildClass>().Where(c => c.Test == "Second").Single();

                var newParent = connection.Load<ParentClass>(parent.Id);
                var modifiedChildren = new[] { new ChildDto { Test = "Second", Id = second.Id }, new ChildDto { Test = "Third" } };
                connection.UpdateManyToMany(newParent, newParent.Children, modifiedChildren, mapper);

                connection.Cache.Clear();

                Assert.NotNull(first);
                var final = connection.Load<ParentClass>(parent.Id);
                Assert.Equal(2, final.Children.Count);
                Assert.True(final.Children.Any(c => c.Child.Test == "Second"));
                Assert.True(final.Children.Any(c => c.Child.Test == "Third"));
                transaction.Rollback();
            }
        }
    }
}
