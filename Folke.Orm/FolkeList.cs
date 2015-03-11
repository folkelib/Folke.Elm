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
                    var query = this.connection.Query<T>().All();
                    var joinTables = new List<BaseQueryBuilder.TableAlias>();
                    var queryBuilder = query.QueryBuilder;
                    foreach (var join in joins)
                    {
                        var property = typeof(T).GetProperty(join);
                        queryBuilder.AppendSelect();
                        joinTables.Add(queryBuilder.AppendSelectedColumns(property.PropertyType, join, property.PropertyType.GetProperties()));
                    }

                    queryBuilder.AppendFrom();
                    queryBuilder.AppendTable(typeof(T), (string)null);

                    foreach (var joinTable in joinTables)
                    {
                        var property = typeof(T).GetProperty(joinTable.alias);
                        var joinKeyProperty = TableHelpers.GetKey(joinTable.type);
                        queryBuilder.Append(" LEFT JOIN ");
                        queryBuilder.AppendTable(joinTable.type, joinTable.alias);
                        queryBuilder.Append(" ON ");
                        queryBuilder.AppendColumn(new BaseQueryBuilder.TableColumn { Column = joinKeyProperty, Table = joinTable });
                        queryBuilder.Append(" = ");
                        queryBuilder.AppendColumn(new BaseQueryBuilder.TableColumn { Column = property, Table = queryBuilder.DefaultTable });
                    }

                    bool first = true;
                    foreach (var property in typeof(T).GetProperties())
                    {
                        if (property.PropertyType == parent)
                        {
                            queryBuilder.Append(first ? " WHERE " : " OR ");
                            first = false;
                            queryBuilder.AppendColumn(new BaseQueryBuilder.TableColumn { Column = property, Table = queryBuilder.DefaultTable });
                            queryBuilder.Append(" = ");
                            queryBuilder.AppendParameter(parentId);
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
