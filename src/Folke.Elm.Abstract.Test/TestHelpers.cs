using System;
using System.Configuration;

namespace Folke.Elm.Mysql.Test
{
    public static class TestHelpers
    {
        public static string ConnectionString
        {
            get
            {
                if (Environment.GetEnvironmentVariable("CI") != null)
                {
                    return ConfigurationManager.ConnectionStrings["CI"].ConnectionString;
                }
                return ConfigurationManager.ConnectionStrings["Test"].ConnectionString;
            }
        }
    }
}
