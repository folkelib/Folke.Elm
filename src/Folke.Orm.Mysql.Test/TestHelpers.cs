using Microsoft.Framework.ConfigurationModel;
using System;
using System.Configuration;

namespace Folke.Orm.Mysql.Test
{
    public static class TestHelpers
    {
        public static string ConnectionString
        {
            get
            {
                var config = new Configuration().AddJsonFile("Config.json");
                if (Environment.GetEnvironmentVariable("CI") != null)
                {
                    return config.Get("Data:CI:ConnectionString");
                }
                return config.Get("Data:DefaultConnection:ConnectionString");
            }
        }
    }
}
