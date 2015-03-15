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
                if (Environment.GetEnvironmentVariable("CI") == "True")
                {
                    return ConfigurationManager.ConnectionStrings["CI"].ConnectionString;
                }
                return ConfigurationManager.ConnectionStrings["Test"].ConnectionString;
            }
        }
    }
}
