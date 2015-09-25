using System.Collections;
using System.Collections.Generic;

namespace Folke.Elm
{
    public class QueryableCommand<T> : IQueryableCommand<T>
    {
        public QueryableCommand(IFolkeConnection connection, MappedClass mappedClass, string sql, params object[] parameters)
        {
            Connection = connection;
            Parameters = parameters;
            MappedClass = mappedClass;
            Sql = sql;
        }

        public IFolkeConnection Connection { get; }
        public string Sql { get; }
        public object[] Parameters { get; }
        public IEnumerator<T> GetEnumerator()
        {
            return this.Enumerate().GetEnumerator();
        }
        
        public MappedClass MappedClass { get; }
    }
}
