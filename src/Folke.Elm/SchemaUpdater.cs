using System;
using System.Linq;

using Folke.Elm.Mapping;

namespace Folke.Elm
{
    using System.Collections.Generic;

    internal class SchemaUpdater
    {
        private readonly FolkeConnection connection;

        public SchemaUpdater(FolkeConnection connection)
        {
            this.connection = connection;
        }

        public void CreateOrUpdate<T>()
            where T : class, new()
        {
            CreateOrUpdate(typeof(T));
        }

        public void CreateOrUpdate(Type tableType)
        {
            var typeMap = connection.Mapper.GetTypeMapping(tableType);
            CreateOrUpdate(typeMap);
        }

        public void CreateOrUpdate(TypeMapping typeMapping)
        {
            var columns = connection.Driver.GetColumnDefinitions(connection, typeMapping);
            if (columns.Count == 0)
            {
                connection.CreateTable(typeMapping.Type);
                return;
            }

            var alterTable = new SchemaQueryBuilder<FolkeTuple>(connection).AlterTable(typeMapping.Type);
            var changes = alterTable.AlterColumns(typeMapping.Type, columns);
            if (changes)
                alterTable.Execute();
        }

        public void CreateOrUpdate(List<TypeMapping> tables)
        {
            using (var transaction = connection.BeginTransaction())
            {
                var existingTableTables = connection.Driver.GetTableDefinitions(connection).Select(t => t.Name.ToLower()).ToList();
                var tableToCreate = tables.Where(t => existingTableTables.All(y => y != t.TableName.ToLower())).ToList();
                
                foreach (var table in tableToCreate)
                {
                    connection.CreateTable(table.Type, false, existingTableTables);
                    existingTableTables.Add(table.TableName.ToLower());
                }

                foreach (var table in tables)
                {
                    CreateOrUpdate(table);
                }

                transaction.Commit();
            }
        }
    }
}
