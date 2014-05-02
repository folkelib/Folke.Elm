using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Orm
{
    internal class FolkeList<T> : IReadOnlyList<T>
        where T : class, IFolkeTable, new()
    {
        private readonly Type parent;
        private readonly int parentId;
        private readonly IFolkeConnection connection;
        private IList<T> results;
        private readonly string[] joins;
        
        public FolkeList(IFolkeConnection connection, Type parent, int parentId, string[] joins)
        {
            this.connection = connection;
            this.parent = parent;
            this.parentId = parentId;
            this.joins = joins;
        }

        private IList<T> Results
        {
            get
            {
                if (results == null)
                {
                    var query = this.connection.Query<T>().SelectAll();
                    foreach (var join in joins)
                    {
                        var property = typeof(T).GetProperty(join);
                        query.AndAll(property.PropertyType, join);
                    }

                    query.From();

                    foreach (var join in joins)
                    {
                        var property = typeof(T).GetProperty(join);
                        query.LeftJoin(property.PropertyType, join).On(property, null, property.PropertyType.GetProperty("Id"), join);
                    }

                    foreach (var property in typeof(T).GetProperties())
                    {
                        if (property.PropertyType == parent)
                        {
                            query.OrWhere().Column(property).Equals().Parameter(parentId);
                        }
                    }
                    results = query.List();
                }
                return results;
            }
        }

        public T this[int index] 
        {
            get
            {
                return Results[index];
            }
        }

        public int Count
        {
            get { return Results.Count; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Results.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Results.GetEnumerator();
        }
    }
}
