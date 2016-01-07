using System;
using System.Data.Common;
using System.Threading.Tasks;
using Folke.Elm.Mapping;

namespace Folke.Elm
{
    public interface IFolkeCommand : IDisposable
    {
        DbParameter CreateParameter();
        DbParameterCollection Parameters { get; }
        string CommandText { get; set; }
        DbDataReader ExecuteReader();
        Task<DbDataReader> ExecuteReaderAsync();
        void ExecuteNonQuery();
        Task ExecuteNonQueryAsync();
        void SetParameters(object[] commandParameters, IMapper mapper, IDatabaseDriver databaseDriver);
    }
}