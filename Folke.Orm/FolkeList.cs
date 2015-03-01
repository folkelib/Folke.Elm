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
                    var joinTables = new List<BaseQueryBuilder.TableAlias>();
                    foreach (var join in joins)
                    {
                        var property = typeof(T).GetProperty(join);
                        query.AppendSelect();
                        joinTables.Add(query.AppendSelectedColumns(property.PropertyType, join, property.PropertyType.GetProperties()));
                    }

                    query.From();

                    foreach (var joinTable in joinTables)
                    {
                        var property = typeof(T).GetProperty(joinTable.alias);
                        var joinKeyProperty = TableHelpers.GetKey(joinTable.type);
                        query.Append(" LEFT JOIN ");
                        query.AppendTable(joinTable.type, joinTable.alias);
                        query.Append(" ON ");
                        query.AppendColumn(new BaseQueryBuilder.TableColumn { Column = joinKeyProperty, Table = joinTable });
                        query.Append(" = ");
                        query.AppendColumn(new BaseQueryBuilder.TableColumn { Column = property, Table = query.DefaultTable });
                    }

                    bool first = true;
                    foreach (var property in typeof(T).GetProperties())
                    {
                        if (property.PropertyType == parent)
                        {
                            query.Append(first ? " WHERE " : " OR ");
                            first = false;
                            query.AppendColumn(new BaseQueryBuilder.TableColumn { Column = property, Table = query.DefaultTable });
                            query.Append(" = ");
                            query.AppendParameter(parentId);
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
