using Folke.Orm.InformationSchema;
using System;
using System.Linq;
using System.Reflection;

namespace Folke.Orm
{
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
            var columns = connection.Driver.GetColumnDefinitions(connection, typeMap);
            if (columns.Count == 0)
            {
                connection.CreateTable(tableType);
                return;
            }

            var alterTable = new SchemaQueryBuilder<FolkeTuple>(connection).AlterTable(tableType);
            var changes = alterTable.AlterColumns(tableType, columns);
            if (changes)
                alterTable.Execute();
        }

        public void CreateOrUpdateAll(Assembly assembly)
        {
            var tables = assembly.DefinedTypes.Where(t => t.IsClass && (t.GetInterface("IFolkeTable") != null || t.GetCustomAttribute<TableAttribute>() != null)).ToList();
            using (var transaction = connection.BeginTransaction())
            {
                var existingTableTables = connection.Driver.GetTableDefinitions(connection, connection.Database).Select(t => t.Name.ToLower()).ToList();
                var tableToCreate = tables.Where(t => existingTableTables.All(y => y != TableHelpers.GetTableName(t).ToLower())).ToList();
                
                foreach (var table in tableToCreate)
                {
                    connection.CreateTable(table, false, existingTableTables);
                    existingTableTables.Add(table.Name.ToLower());
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
