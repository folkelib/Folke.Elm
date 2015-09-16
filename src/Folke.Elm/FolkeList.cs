using System;
using System.Collections.Generic;
using System.Reflection;

namespace Folke.Elm
{
    internal class FolkeList<T> : IReadOnlyList<T>
        where T : class, new()
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
                    var query = connection.Select<T>().All();
                    var joinTables = new List<BaseQueryBuilder.TableAlias>();
                    var queryBuilder = query.QueryBuilder;
                    var type = typeof (T);
                    var typeMapping = queryBuilder.Mapper.GetTypeMapping(type);

                    foreach (var join in joins)
                    {
                        var property = type.GetProperty(join);
                        queryBuilder.AppendSelect();
                        //joinTables.Add(queryBuilder.AppendSelectedColumns(property.PropertyType, join, property.PropertyType.GetProperties()));
                        joinTables.Add(queryBuilder.AppendAllSelects(property.PropertyType, join));
                    }

                    queryBuilder.AppendFrom();
                    queryBuilder.AppendTable(typeof(T), (string)null);

                    foreach (var joinTable in joinTables)
                    {
                        var property = typeof(T).GetProperty(joinTable.alias);
                        var joinKeyProperty = joinTable.Mapping.Key;
                        queryBuilder.Append(" LEFT JOIN ");
                        queryBuilder.AppendTable(joinTable.Mapping.Type, joinTable.alias);
                        queryBuilder.Append(" ON ");
                        queryBuilder.AppendColumn(new BaseQueryBuilder.TableColumn { Column = joinKeyProperty, Table = joinTable });
                        queryBuilder.Append(" = ");
                        queryBuilder.AppendColumn(new BaseQueryBuilder.TableColumn { Column = queryBuilder.DefaultTable.Mapping.Columns[property.Name], Table = queryBuilder.DefaultTable });
                    }

                    bool first = true;
                    foreach (var property in typeMapping.Columns.Values)
                    {
                        if (property.PropertyInfo.PropertyType == parent)
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
