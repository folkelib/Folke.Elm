using System;
using Folke.Elm.Mapping;
using Folke.Elm.Sqlite;
using Microsoft.Extensions.Options;

namespace Folke.Elm.Sample
{
    public class Program
    {
        static void Main(string[] args)
        {
            var mapper = new Mapper();
            var table = mapper.GetTypeMapping<Table>();
            table.ToTable("Blabla");
            table.HasKey(x => x.Id);
            var folke = new FolkeConnection(new SqliteDriver(), mapper, new OptionsManager<ElmOptions>(new []{ new ConfigureOptions<ElmOptions>(
                options =>
                {
                    options.ConnectionString = "Data Source=:memory:";
                    options.SqlCommandTracesEnabled = true;
                })}));
            folke.CreateTable<Table>();
            folke.Save(new Table());
        }

        public class Table
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }
    }
}