Folke.Elm
=========

Object-Relational Mapping library written in C#.

[![Documentation Status](https://readthedocs.org/projects/folkeelm/badge/?version=latest)](http://folkeelm.readthedocs.org/en/latest/?badge=latest)
[![Build status](https://ci.appveyor.com/api/projects/status/i8rshg1fkdyhkr37?svg=true)](https://ci.appveyor.com/project/acastaner/folke-elm)
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

public class Program
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddElm<MySqlDriver>(options =>
            options.ConnectionString = "XXX");
    }
    
    public void Configure(IFolkeConnection connection)
    {        
        connection.CreateTable<TestPoco>(drop: true);
        connection.CreateTable<TestManyPoco(drop: true);
        
        var newPoco = new TestPoco { Name = "Test" };
        connection.Save(newPoco);
        
        var newMany = new TestManyPoco { Name = "Many", Poco = newPoco };
        connection.Save(newMany);
        
        var manies = connection.SelectAllFrom<TestManyPoco>(p => p.Poco).Where(t => t.Poco == newPoco).ToList();
        Debug.Assert(manies.Count == 1);
        Debug.Assert(manies[0].Poco == newPoco);
    }
}

```

##Features

* MySQL, PostgreSQL, Microsoft SQL Server and Sqlite
* .NET Core 5.0 and .NET 4.51

