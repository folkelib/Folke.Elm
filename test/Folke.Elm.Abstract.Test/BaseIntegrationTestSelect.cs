using System;
using Xunit;
using Folke.Elm.Fluent;

namespace Folke.Elm.Abstract.Test
{
    public class BaseIntegrationTestSelect : BaseIntegrationTest, IIntegrationTestSelect
    {
        public BaseIntegrationTestSelect(IDatabaseDriver databaseDriver, string connectionString, bool drop) : base(databaseDriver, connectionString, drop)
        {
        }

        public void SelectAllFrom_TableType_WhereStringEqual_Single()
        {
            var newPoco = new TestPoco { Name = "Tutu " };
            connection.Save(newPoco);
            var foundPoco = connection.SelectAllFrom<TestPoco>().Single(t => t.Name == newPoco.Name);
            Assert.NotNull(foundPoco);
            Assert.Equal(newPoco.Name, foundPoco.Name);
        }

        public void SelectAllFrom_TableType_WhereBooleanEqual_List()
        {
            var newPocoFalse = new TestPoco { Name = "Hihi" };
            connection.Save(newPocoFalse);
            var newPocoTrue = new TestPoco { Name = "Huhu", Boolean = true };
            connection.Save(newPocoTrue);

            var foundTrue = connection.SelectAllFrom<TestPoco>().Where(t => t.Boolean).ToList();
            Assert.Equal(1, foundTrue.Count);
            Assert.Equal(newPocoTrue.Name, foundTrue[0].Name);
            var foundFalse = connection.SelectAllFrom<TestPoco>().Where(t => !t.Boolean).ToList();
            Assert.Equal(1, foundFalse.Count);
            Assert.Equal(newPocoFalse.Name, foundFalse[0].Name);
        }
        
        public void SelectAllFrom_TableType_WhereStringIsNull_Single()
        {
            var newPoco = new TestPoco { Name = null };
            connection.Save(newPoco);
            var foundPoco = connection.SelectAllFrom<TestPoco>().Single(t => t.Name == null);
            Assert.Equal(newPoco.Id, foundPoco.Id);
        }

        public void SelectAllFrom_TableType_WhereVariableIsNull_Single()
        {
            var newPoco = new TestPoco { Name = null };
            connection.Save(newPoco);
            string variable = null;
            var foundPoco = connection.SelectAllFrom<TestPoco>().Single(t => t.Name == variable);
            Assert.Equal(newPoco.Id, foundPoco.Id);
        }

        public void SelectAllFrom_TableType_WhereObject_ReturnedItemsHaveReferenceToCachedItem_List()
        {
            var newPoco = new TestPoco { Name = null };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);

            var manies = connection.SelectAllFrom<TestManyPoco>().Where(t => t.Poco == newPoco).ToList();
            Assert.Equal(1, manies.Count);
            Assert.Equal(newPoco, manies[0].Poco);
        }

        public void SelectAllFrom_TableType_WhereObject_ReturnedItemsHaveReferenceToNotCachedItem_List()
        {
            var newPoco = new TestPoco { Name = null };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);

            connection.Cache.Clear();

            var manies = connection.SelectAllFrom<TestManyPoco>().Where(t => t.Poco == newPoco).ToList();
            Assert.Equal(1, manies.Count);
            Assert.Equal(newPoco.Id, manies[0].Poco.Id);
            Assert.Null(manies[0].Poco.Name);
        }
        
        public void Select_TableType_LeftJoinOnIdWhereString_List()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);

            connection.Cache.Clear();

            var manies = connection.Select<TestManyPoco>().All().All(x => x.Poco).From().LeftJoinOnId(x => x.Poco).Where(t => t.Toto == "Toto").ToList();
            Assert.Equal(1, manies.Count);
            Assert.Equal(newPoco.Id, manies[0].Poco.Id);
            Assert.Equal(newPoco.Name, manies[0].Poco.Name);
        }

        public void Select_TableType_ValuesTwoColumns_List()
        {
            var newPoco = new TestPoco { Name = "Ihihi" };
            connection.Save(newPoco);
            connection.Cache.Clear();
            var poco = connection.Select<TestPoco>().Values(x => x.Id, x => x.Name).From().ToList();
            Assert.Equal(newPoco.Id, poco[0].Id);
            Assert.Equal(newPoco.Name, poco[0].Name);
        }
        
        public void SelectAllFrom_TableTypeAndAutoJoin_WhereString_List()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);

            connection.Cache.Clear();

            var manies = connection.SelectAllFrom<TestManyPoco>(t => t.Poco).Where(t => t.Toto == "Toto").ToList();
            Assert.Equal(1, manies.Count);
            Assert.Equal(newPoco.Id, manies[0].Poco.Id);
            Assert.Equal(newPoco.Name, manies[0].Poco.Name);
        }

        public void Select_TypeThatIsNotATable_AllFieldsFromProperties_List()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);

            var manies = connection.Select<TestNotATable>().All(x => x.Poco).All(x => x.Many).From(x => x.Many)
                .LeftJoin(x => x.Poco).On(x => x.Many.Poco == x.Poco).ToList();
            Assert.Equal(newPoco.Name, manies[0].Poco.Name);
            Assert.Equal(newMany.Toto, manies[0].Many.Toto);
        }
        
        public void Select_TypeThatIsNotATable_AllFieldsFromProperties_JoinOnString_List()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);
            var newMany2 = new TestManyPoco { Toto = "Tutu", Poco = newPoco };
            connection.Save(newMany2);

            connection.Cache.Clear();

            var manies = connection.Select<TestNotATable>().All(x => x.Poco).All(x => x.Many).From(x => x.Poco)
                .LeftJoin(x => x.Many).On(x => x.Many.Poco == x.Poco).AndOn(x => x.Many.Toto == "Toto").ToList();
            Assert.Equal(1, manies.Count);
            Assert.Equal(newPoco.Name, manies[0].Poco.Name);
            Assert.Equal(newMany.Toto, manies[0].Many.Toto);
        }

        public void Select_TypeThatIsNotATable_AllFieldsFromProperties_JoinOnStringOrderByString_List()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);
            var newMany2 = new TestManyPoco { Toto = "Tutu", Poco = newPoco };
            connection.Save(newMany2);

            connection.Cache.Clear();

            var manies = connection.Select<TestNotATable>().All(x => x.Poco).All(x => x.Many).From(x => x.Poco)
                .LeftJoin(x => x.Many).On(x => x.Many.Poco == x.Poco).AndOn(x => x.Many.Toto == "Titi").OrderBy(x => x.Poco.Name).ToList();
            Assert.Equal(1, manies.Count);
            Assert.Equal(newPoco.Name, manies[0].Poco.Name);
            Assert.Equal(null, manies[0].Many);
        }

        public void SelectFrom_TableType_OrderByStringDescLimit_List()
        {
            for (var i = 0; i < 10; i++)
            {
                var newPoco = new TestPoco { Name = "Name" + i };
                connection.Save(newPoco);
            }

            var pocos = connection.SelectAllFrom<TestPoco>().OrderBy(x => x.Name).Desc().Limit(1, 2).ToList();
            Assert.Equal(2, pocos.Count);
            Assert.Equal("Name8", pocos[0].Name);
            Assert.Equal("Name7", pocos[1].Name);
        }

        public void Select_Tuple_WhereExistsSubQuery_List()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);
            var otherPoco = new TestPoco { Name = "OtherName" };
            connection.Save(otherPoco);

            var pocos = connection.Select<FolkeTuple<TestPoco, TestManyPoco>>().All(x => x.Item0).From(x => x.Item0)
                .WhereExists(sub => sub.All(x => x.Item1).From(x => x.Item1).Where(x => x.Item1.Poco == x.Item0)).ToList();
            Assert.Equal(1, pocos.Count);
            Assert.Equal(newPoco.Name, pocos[0].Item0.Name);
        }
        
        public void Select_Tuple_FromLeftJoinOnId_List()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);
            var pocos =
                connection.Select<FolkeTuple<TestPoco, TestManyPoco>>()
                    .All(x => x.Item0)
                    .All(x => x.Item1)
                    .From(x => x.Item0)
                    .LeftJoin(x => x.Item1).On(x => x.Item1.Poco == x.Item0).ToList();
            Assert.Equal(1, pocos.Count);
            Assert.NotNull(pocos[0].Item0);
            Assert.Equal(newPoco.Name, pocos[0].Item0.Name);
            Assert.Equal(newMany.Toto, pocos[0].Item1.Toto);
            Assert.Equal(newPoco, pocos[0].Item1.Poco);
        }
        
        public void Select_Tuple_FromRightJoin_List()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);
            var otherMany = new TestManyPoco { Toto = "OtherToto", Poco = newPoco };
            connection.Save(otherMany);
            var pocos =
                connection.Select<FolkeTuple<TestPoco, TestManyPoco>>()
                    .All(x => x.Item0)
                    .All(x => x.Item1)
                    .From(x => x.Item0)
                    .RightJoin(x => x.Item1)
                    .On(x => x.Item1.Poco == x.Item0)
                    .ToList();
            Assert.Equal(2, pocos.Count);
            Assert.Equal(newPoco.Name, pocos[0].Item0.Name);
            Assert.Equal(newMany.Toto, pocos[0].Item1.Toto);
            Assert.Equal(newPoco, pocos[0].Item1.Poco);
            Assert.Equal(newPoco.Name, pocos[1].Item0.Name);
            Assert.Equal(otherMany.Toto, pocos[1].Item1.Toto);
            Assert.Equal(newPoco, pocos[1].Item1.Poco);
        }

        public void Select_TableType_InnerJoinOnId_List()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);
            var otherMany = new TestManyPoco { Toto = "OtherToto" };
            connection.Save(otherMany);
            var pocos =
                connection.Select<TestManyPoco>()
                    .All()
                    .All(x => x.Poco)
                    .From()
                    .InnerJoin(x => x.Poco).OnId(x => x.Poco).ToList();
            Assert.Equal(1, pocos.Count);
            Assert.Equal(newPoco.Name, pocos[0].Poco.Name);
            Assert.Equal(newMany.Toto, pocos[0].Toto);
        }

        public void SelectAllFrom_TableType_WhereLike_List()
        {
            var onePoco = new TestPoco { Name = "One" };
            connection.Save(onePoco);
            var twoPoco = new TestPoco { Name = "Two" };
            connection.Save(twoPoco);

            var result = connection.SelectAllFrom<TestPoco>().Where(x => x.Name.Like("On%")).ToList();
            Assert.Equal(1, result.Count);
            Assert.Equal("One", result[0].Name);
        }
        
        public void Select_OneColumnInTableWithForeignKeys_List()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);

            connection.Cache.Clear();

            var response = connection.Select<TestManyPoco>().Values(x => x.Toto).From().ToList();
            Assert.Equal(newMany.Toto, response[0].Toto);
        }

        public void Select_OneColumnAndIdInTableWithForeignKeys_List()
        {
            var newPoco = new TestPoco { Name = "Name" };
            connection.Save(newPoco);
            var newMany = new TestManyPoco { Toto = "Toto", Poco = newPoco };
            connection.Save(newMany);

            connection.Cache.Clear();

            var response = connection.Select<TestManyPoco>().Values(x => x.Id).Values(x => x.Toto).From().ToList();
            Assert.Equal(newMany.Toto, response[0].Toto);
        }

        public void SelectAllFrom_TableType_OrderByAscLimitWithVariable_List()
        {
            // Arrange
            for (var i = 0; i < 10; i++)
            {
                var newPoco = new TestPoco { Name = "Name" + i };
                connection.Save(newPoco);
            }

            // Act
            var results = connection.SelectAllFrom<TestPoco>().OrderBy(x => x.Name).Asc().Limit(x => 5, 5).ToList();

            // Assert
            Assert.Equal(5, results.Count);
            Assert.Equal("Name5", results[0].Name);
        }

        public void SelectAllFrom_LimitWithParameter_List()
        {
            // Arrange
            for (var i = 0; i < 10; i++)
            {
                var newPoco = new TestPoco { Name = "Name" + i };
                connection.Save(newPoco);
            }

            // Act
            var results = FluentBaseBuilder<TestPoco, FolkeTuple<int>>.Select(connection.Driver, connection.Mapper).All()
                    .From()
                    .OrderBy(x => x.Name)
                    .Limit((x, y) => y.Item0, 5).Build(connection, 5).ToList();

            // Assert
            Assert.Equal(5, results.Count);
            Assert.Equal("Name5", results[0].Name);
        }

        public void Select_CountAll_Scalar()
        {
            // Arrange
            for (var i = 0; i < 10; i++)
            {
                var newPoco = new TestPoco { Name = "Name" + i };
                connection.Save(newPoco);
            }

            // Act
            var result = connection.Select<TestPoco>().CountAll().From().Scalar<int>();

            // Assert
            Assert.Equal(10, result);
        }

        public void SelectAllFrom_Linq_WhereStringSelectAll_List()
        {
            var newPoco = new TestPoco { Name = Guid.NewGuid().ToString() };
            connection.Save(newPoco);
            var otherPoco = new TestPoco { Name = Guid.NewGuid().ToString() };
            connection.Save(otherPoco);

            var query = from toto in connection.SelectAllFrom<TestPoco>() where toto.Name == newPoco.Name select toto;
            var result = query.ToList();
            Assert.Equal(1, result.Count);
            Assert.Equal(newPoco.Name, result[0].Name);
        }
        
        public void SelectAllFrom_TableType_WhereWithQuote_SingleOrDefault()
        {
            var newPoco = new TestPoco { Name = Guid.NewGuid().ToString() + "'azer'ty" };
            connection.Save(newPoco);

            var result = connection.SelectAllFrom<TestPoco>().Where(x => x.Name == newPoco.Name).SingleOrDefault();

            Assert.NotNull(result);
            Assert.Equal(newPoco.Name, result.Name);
        }
        
        public void SelectAllFrom_TableType_WithDecimalValue_SingleOrDefault()
        {
            var newDecimal = new TestOtherPoco() { Decimal = new decimal(1.23), Byte = 4 };
            connection.Save(newDecimal);
            connection.Cache.Clear();

            var result = connection.SelectAllFrom<TestOtherPoco>().SingleOrDefault();

            Assert.NotNull(result);
            Assert.Equal(newDecimal.Decimal, result.Decimal);
            Assert.Equal(newDecimal.Byte, result.Byte);
        }

        /*   
        public void TestLinqWithJoin()
        {
            var newPoco = new TestPoco { Name = Guid.NewGuid().ToString() };
            connection.Save(newPoco);
            var linked = new TestManyPoco {Poco = newPoco, Toto = Guid.NewGuid().ToString()};
            connection.Save(linked);

            FluentWhereBuilder<TestPoco, FolkeTuple> query = from toto in connection.SelectAll<TestPoco>() 
                                                                join link in toto.
                                                                where toto.Name == newPoco.Name select toto;
            var result = query.ToList();
            Assert.Equal(1, result.Count);
            Assert.Equal(newPoco.Name, result[0].Name);
        }*/

        public void Select_TableWithGuid_WhereId_List()
        {
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            connection.InsertInto<TableWithGuid>().Values(value).Execute();
            
            var values = connection.Select<TableWithGuid>().All().From().Where(x => x.Id == value.Id).ToList();
            Assert.NotEmpty(values);
            Assert.Equal(value.Id, values[0].Id);
            Assert.Equal(value.Text, values[0].Text);
        }

        public void Select_TableWithGuid_WhereObject_List()
        {
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            connection.InsertInto<TableWithGuid>().Values(value).Execute();
            
            var values = connection.Select<TableWithGuid>().All().From().Where(x => x == value).ToList();
            Assert.NotEmpty(values);
            Assert.Equal(value.Id, values[0].Id);
            Assert.Equal(value.Text, values[0].Text);
        }

        public void Select_TableWithGuid_WhereParameter_List()
        {
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            connection.InsertInto<TableWithGuid>().Values(value).Execute();
            
            var values = connection.Select<TableWithGuid, FolkeTuple<TableWithGuid>>().All().From().Where((x, p) => x == p.Item0).Build(connection, value).ToList();
            Assert.NotEmpty(values);
            Assert.Equal(value.Id, values[0].Id);
            Assert.Equal(value.Text, values[0].Text);
        }

        public void SelectAllFrom_UseCache()
        {
            var value = new TableWithGuid
            {
                Id = Guid.NewGuid(),
                Text = "Text"
            };
            connection.Save(value);
            var parent = new ParentTableWithGuid {Key = Guid.NewGuid(), Reference = value};
            connection.Save(parent);
            var grandParent = new GrandParentWithGuid { Key = Guid.NewGuid(), Reference = parent };
            connection.Save(grandParent);
            value = connection.Get<TableWithGuid>(value.Id);
            var all = connection.SelectAllFrom<GrandParentWithGuid>().Where(x => x.Reference == parent).ToList();
            Assert.Equal(value, all[0].Reference.Reference);
            Assert.Equal("Text", value.Text);
        }

        public void Select_WithComplexType()
        {
            var value = new Playground { Position = new Position { Latitude = 2, Longitude = 3 }};
            connection.Save(value);

            value.Position.Longitude = 4;
            connection.Update(value);

            value = connection.SelectAllFrom<Playground>().Where(x => x.Position.Longitude > 0).FirstOrDefault();
            Assert.Equal(4, value.Position.Longitude);
        }
    }
}
