Folke.Elm
=========

 Object-Relational Mapping library written in C#

##Usage

```C#

public class TestPoco
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public bool Boolean { get; set; }
}

public class TestManyPoco
{
    [Key]
    public int Id { get; set; }
    public string Toto { get; set; }
    public TestPoco Poco { get; set; }
}

public class Example
{
    public void Test()
    {
        var driver = new MySqlDriver();
        var mapper = new Mapper();
        var connectionString = "Server=localhost;Database=test;Uid=root;Pwd=test";
        var connection = new FolkeConnection(driver, mapper, connectionString);
        connection.CreateTable<TestPoco>(drop: true);
        connection.CreateTable<TestManyPoco(drop: true);
        
        var newPoco = new TestPoco { Name = "Test" };
        connection.Save(newPoco);
        
        var newMany = new TestManyPoco { Name = "Many", Poco = newPoco };
        connection.Save(newMany);
        
        var manies = connection.SelectAllFrom<TestManyPoco>(p => p.Poco).Where(t => t.Poco == newPoco).List();
        Assert.AreEqual(1, manies.Count);
        Assert.AreEqual(newPoco, manies[0].Poco);
    }
}

```

##Features

* MySQL and Sqlite
* DNX 4.51, Core 5.0, and .NET 4.5

