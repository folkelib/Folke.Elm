Folke.Orm
=========

Light Object-Relational Mapping library written in C#.

[![Build status](https://ci.appveyor.com/api/projects/status/8kij487umeteeqes?svg=true)](https://ci.appveyor.com/project/Sidoine/orm)

##Usage

```C#

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

public class Example
{
    public void Test()
    {
        var driver = new MySqlDriver(new DatabaseSettings { Database = "sample", Host = "localhost", Password = "test", User = "test" });
        var connection = new FolkeConnection(driver);
        connection.CreateTable<TestPoco>(drop: true);
        connection.CreateTable<TestManyPoco(drop: true);
        
        var newPoco = new TestPoco { Name = "Test" };
        connection.Save(newPoco);
        
        var newMany = new TestManyPoco { Name = "Many", Poco = newPoco };
        connection.Save(newMany);
        
        var manies = connection.QueryOver<TestManyPoco>(p => p.Poco).Where(t => t.Poco == newPoco).List();
        Assert.AreEqual(1, manies.Count);
        Assert.AreEqual(newPoco, manies[0].Poco);
    }
}

```

##Limitations

* Only MySQL and Sqlite
* In development, not feature complete but usable.

