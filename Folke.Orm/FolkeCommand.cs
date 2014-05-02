using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Orm
{
    public class FolkeCommand: IDisposable
    {
        private DbCommand command;
        private FolkeConnection connection;

        public FolkeCommand(FolkeConnection connection, DbCommand command)
        {
            this.connection = connection;
            this.command = command;
        }

        public void Dispose()
        {
            command.Dispose();
            connection.CloseCommand();
        }

        internal DbParameter CreateParameter()
        {
            return command.CreateParameter();
        }

        public DbParameterCollection Parameters { get { return command.Parameters; } }

        public string CommandText { get { return command.CommandText; } set { command.CommandText = value; } }

        internal DbDataReader ExecuteReader()
        {
            return command.ExecuteReader();
        }

        internal void ExecuteNonQuery()
        {
            command.ExecuteNonQuery();
        }
    }
}
