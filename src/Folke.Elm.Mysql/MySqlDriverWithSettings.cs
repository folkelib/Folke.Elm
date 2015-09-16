using System.Data.Common;
using MySql.Data.MySqlClient;

namespace Folke.Elm.Mysql
{
    public class MySqlDriverWithSettings : MySqlDriver
    {
        public IDatabaseSettings Settings { get; private set; }

        public MySqlDriverWithSettings(IDatabaseSettings settings)
        {
            Settings = settings;
        }
        
        public override DbConnection CreateConnection(string connectionString)
        {
            return new MySqlConnection(connectionString ?? "Server=" + Settings.Host + "; Database=" + Settings.Database + "; Uid=" + Settings.User +
                               "; Pwd=" + Settings.Password);
        }
    }
}
