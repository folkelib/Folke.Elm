﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Folke.Elm.Fluent;
using Folke.Elm.Visitor;

namespace Folke.Elm
{
    internal class FolkeList<T> : IReadOnlyList<T>
        where T : class, new()
    {
        private readonly Type parent;
        private readonly object parentId;
        private readonly IFolkeConnection connection;
        private IList<T> results;
        private readonly string[] joins;
        
        public FolkeList(IFolkeConnection connection, Type parent, object parentId, string[] joins)
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
                    var joinTables = new Dictionary<PropertyInfo, SelectedTable>();
                    var queryBuilder = query.QueryBuilder;
                    var type = typeof (T);
                    var typeInfo = type.GetTypeInfo();
                    var typeMapping = queryBuilder.Mapper.GetTypeMapping(type);
                    var rootTable = queryBuilder.RegisterRootTable();
                    // Select from the tables in the joins list
                    foreach (var join in joins)
                    {
                        var property = typeInfo.GetProperty(join);
                        var propertyMapping = typeMapping.GetColumn(property);
                        query.AppendSelect();
                        var table = queryBuilder.RegisterTable(rootTable, propertyMapping);
                        joinTables.Add(property, table);
                        queryBuilder.AppendAllSelects(table);
                    }

                    query.AppendFrom();
                    queryBuilder.StringBuilder.AppendTable(rootTable);

                    // Join on all the tables in the joins list
                    foreach (var joinTable in joinTables)
                    {
                        var property = joinTable.Key;
                        var joinKeyProperty = joinTable.Value.Mapping.Key;
                        queryBuilder.StringBuilder.AppendAfterSpace("LEFT JOIN ");
                        queryBuilder.StringBuilder.AppendTable(joinTable.Value);
                        queryBuilder.StringBuilder.Append(" ON ");
                        Field tableColumn = new Field(joinTable.Value, joinKeyProperty);
                        queryBuilder.StringBuilder.DuringColumn(tableColumn.Table.Alias, tableColumn.Column.ColumnName);
                        queryBuilder.StringBuilder.Append(" = ");
                        Field tableColumn1 = new Field(queryBuilder.DefaultTable, queryBuilder.DefaultTable.Mapping.Columns[property.Name]);
                        queryBuilder.StringBuilder.DuringColumn(tableColumn1.Table.Alias, tableColumn1.Column.ColumnName);
                    }

                    bool first = true;
                    // Add a WHERE in order to get only the values that reference the parent row
                    foreach (var property in typeMapping.Columns.Values)
                    {
                        if (property.PropertyInfo.PropertyType == parent)
                        {
                            queryBuilder.StringBuilder.Append(first ? " WHERE " : " OR ");
                            first = false;
                            Field tableColumn = new Field(queryBuilder.DefaultTable, property);
                            queryBuilder.StringBuilder.DuringColumn(tableColumn.Table.Alias, tableColumn.Column.ColumnName);
                            queryBuilder.StringBuilder.Append(" = ");
                            var index = queryBuilder.AddParameter(parentId);
                            queryBuilder.StringBuilder.DuringParameter(index);
                        }
                    }
                    results = query.ToList();
                }
                return results;
            }
        }

        public T this[int index] => Results[index];

        public int Count => Results.Count;

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
