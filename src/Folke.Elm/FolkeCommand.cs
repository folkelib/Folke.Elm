using System;
using System.Data.Common;
using System.Globalization;
using System.Threading.Tasks;
using Folke.Elm.Mapping;
using System.Reflection;

namespace Folke.Elm
{
    public class FolkeCommand: IDisposable, IFolkeCommand
    {
        private readonly DbCommand command;
        private readonly FolkeConnection connection;

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

        public DbParameter CreateParameter()
        {
            return command.CreateParameter();
        }

        public DbParameterCollection Parameters => command.Parameters;

        public string CommandText { get { return command.CommandText; } set { command.CommandText = value; } }

        public DbDataReader ExecuteReader()
        {
            return command.ExecuteReader();
        }

        public async Task<DbDataReader> ExecuteReaderAsync()
        {
            return await command.ExecuteReaderAsync();
        }

        public void ExecuteNonQuery()
        {
            command.ExecuteNonQuery();
        }

        public async Task ExecuteNonQueryAsync()
        {
            await command.ExecuteNonQueryAsync();
        }

        public void SetParameters(object[] commandParameters, IMapper mapper, IDatabaseDriver databaseDriver)
        {
            for (var i = 0; i < commandParameters.Length; i++)
            {
                var parameterName = "@Item" + i.ToString(CultureInfo.InvariantCulture);
                var parameter = commandParameters[i];
                var commandParameter = command.CreateParameter();
                commandParameter.ParameterName = parameterName;
                commandParameter.Value = databaseDriver.ConvertValueToParameter(mapper, parameter);
                command.Parameters.Add(commandParameter);
            }
        }
    }
}
