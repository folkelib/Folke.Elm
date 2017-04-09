using System;
using System.Collections.Generic;
using System.Linq;
using Folke.Elm.InformationSchema;
using Folke.Elm.Mapping;

namespace Folke.Elm
{
    public class SchemaQueryBuilder<T> : SchemaQueryBuilder
    {
        private readonly TypeMapping mapping;

        public SchemaQueryBuilder(FolkeConnection connection) : base(connection)
        {
            mapping = connection.Mapper.GetTypeMapping(typeof(T));
        }

        public SchemaQueryBuilder(IMapper mapper, IDatabaseDriver driver) : base(driver)
        {
            mapping = mapper.GetTypeMapping(typeof(T));
        }

        public SchemaQueryBuilder<T> CreateTable()
        {
            CreateTable(mapping);
            return this;
        }
        
        public SchemaQueryBuilder<T> DropTable()
        {
            DropTable(mapping);
            return this;
        }

        internal SchemaQueryBuilder<T> AlterTable()
        {
            AlterTable(mapping);
            return this;
        }

        internal bool AlterColumns(IList<IColumnDefinition> columns)
        {
            return AlterColumns(mapping, columns);
        }
    }

    public class SchemaQueryBuilder : IBaseCommand
    {
        private readonly IDatabaseDriver databaseDriver;
        private readonly FolkeConnection connection;

        private readonly SqlStringBuilder query;

        public SchemaQueryBuilder(FolkeConnection connection) : this(connection.Driver)
        {
            this.connection = connection;
        }

        public SchemaQueryBuilder(IDatabaseDriver databaseDriver)
        {
            this.databaseDriver = databaseDriver;
            query = databaseDriver.CreateSqlStringBuilder();
        }

        private void AppendColumnName(PropertyMapping property, string baseName)
        {
            query.DuringSymbol(property.ComposeName(baseName));
        }
        
        private void AppendColumnType(PropertyMapping property)
        {
            if (property.IsKey)
            {
                query.AppendAfterSpace(databaseDriver.GetSqlType(property, false));
                query.DuringPrimaryKey(property.IsAutomatic);
            }
            else
            {
                query.BeforeColumnTypeDefinition();

                if (property.Reference != null)
                {
                    var foreignPrimaryKey = property.Reference.Key;
                    query.Append(connection.Driver.GetSqlType(foreignPrimaryKey, true));
                    query.AppendAfterSpace("NULL");
                }
                else
                    query.Append(databaseDriver.GetSqlType(property, false));
            }
        }

        private static string GetConstraintEventString(ConstraintEventEnum onWhat)
        {
            switch (onWhat)
            {
                case ConstraintEventEnum.Cascade:
                    return "CASCADE";
                case ConstraintEventEnum.NoAction:
                    return "NO ACTION";
                case ConstraintEventEnum.Restrict:
                    return "RESTRICT";
                case ConstraintEventEnum.SetNull:
                    return "SET NULL";
                default:
                    return null;
            }
        }

        private void AppendForeignKey(PropertyMapping column, string baseName)
        {
            query.Append(" ");
            query.Append("FOREIGN KEY (");
            AppendColumnName(column, baseName);
            query.Append(")");
            AppendReferences(column);
        }

        private void AppendReferences(PropertyMapping column)
        { 
            query.Append(" REFERENCES ");
            query.DuringTable(column.Reference.TableSchema, column.Reference.TableName);
            query.Append("(");
            query.DuringSymbol(column.Reference.Key.ColumnName);
            query.Append(")");
            if (column.OnDelete!= ConstraintEventEnum.Restrict)
            {
                query.Append(" ON DELETE ");
                query.Append(GetConstraintEventString(column.OnDelete));
            }
            if (column.OnUpdate != ConstraintEventEnum.Restrict)
            {
                query.Append(" ON UPDATE ");
                query.Append(GetConstraintEventString(column.OnUpdate));
            }
        }

        private void AppendIndex(PropertyMapping column, string name, string baseName)
        {
            query.Append(" INDEX ");
            query.DuringSymbol(name);
            query.Append(" (");
            AppendColumnName(column, baseName);
            query.Append(")");            
        }

        private void AppendCreateIndex(TypeMapping table, PropertyMapping column, string name, string baseName)
        {
            query.Append("CREATE INDEX ");
            query.DuringSymbol(name);
            query.Append(" ON ");
            query.DuringSymbol(table.TableName);
            query.Append("(");
            AppendColumnName(column, baseName);
            query.Append(")");
        }

        private static bool DoesForeignTableExist(PropertyMapping property, IEnumerable<string> existingTables)
        {
            return existingTables == null || property.Reference == null || existingTables.Any(t => t == property.Reference.TableName.ToLower());
        }

        private string GetAutoIndexName(TypeMapping table, PropertyMapping column)
        {
            return $"{table.TableName}_{column.ColumnName}";
        }

        public SchemaQueryBuilder CreateTable(TypeMapping mapping, IList<string> existingTables = null)
        {
            query.Append("CREATE TABLE ");
            query.DuringSymbol(mapping.TableName);
            query.Append(" (");
            bool canCreateIndex = databaseDriver.CanAddIndexInCreateTable();

            AppendColumns(existingTables, mapping, null);

            if (canCreateIndex)
            {
                AppendColumnIndexes(existingTables, mapping, null);
            }

            AppendColumnForeignKeys(existingTables, mapping, null);

            query.Append(")");

            if (!canCreateIndex)
            {
                AppendCreateIndexes(existingTables, mapping, null);
            }
            return this;
        }

        private void AppendCreateIndexes(IList<string> existingTables, TypeMapping mapping, string baseName)
        {
            foreach (var property in mapping.Columns.Values)
            {
                if (!DoesForeignTableExist(property, existingTables))
                    continue;

                if (property.Index != null)
                {
                    query.Append(";");
                    AppendCreateIndex(mapping, property, property.Index, baseName);
                }
                else if (property.Reference != null)
                {
                    if (property.Reference.IsComplexType)
                    {
                        AppendCreateIndexes(existingTables, property.Reference, property.ComposeName(baseName));
                    }
                    else
                    {
                        query.Append(";");
                        AppendCreateIndex(mapping, property, GetAutoIndexName(mapping, property), baseName);
                    }
                }
            }
        }

        private void AppendColumnForeignKeys(IList<string> existingTables, TypeMapping mapping, string baseName)
        {
            foreach (var property in mapping.Columns.Values)
            {
                if (property.Reference != null)
                {
                    if (property.Reference.IsComplexType)
                    {
                        AppendColumnForeignKeys(existingTables, property.Reference, property.ComposeName(baseName));
                    }
                    else if(DoesForeignTableExist(property, existingTables))
                    {
                        AddComma();
                        AppendForeignKey(property, baseName);
                    }
                }
            }
        }

        private void AppendColumnIndexes(IList<string> existingTables, TypeMapping mapping, string baseName)
        {
            foreach (var property in mapping.Columns.Values)
            {
                if (!DoesForeignTableExist(property, existingTables))
                    continue;

                if (property.Index != null)
                {
                    AddComma();
                    AppendIndex(property, property.Index, baseName);
                }
                else if (property.Reference != null)
                {
                    if (property.Reference.IsComplexType)
                    {
                        AppendColumnIndexes(existingTables, property.Reference, property.ComposeName(baseName));
                    }
                    else
                    {
                        AddComma();
                        AppendIndex(property, GetAutoIndexName(mapping, property), baseName);
                    }
                }
                
            }
        }

        private void AppendColumns(IList<string> existingTables, TypeMapping mapping, string baseName)
        {
            foreach (var column in mapping.Columns.Values)
            {
                if (!DoesForeignTableExist(column, existingTables))
                    continue;

                if (column.Reference != null && column.Reference.IsComplexType)
                {
                    AppendColumns(existingTables, column.Reference, column.ComposeName(baseName));
                }
                else
                {
                    AddComma();
                    AppendColumnName(column, baseName);
                    AppendColumnType(column);
                }
            }
        }

        public SchemaQueryBuilder DropTable(TypeMapping type)
        {
            query.BeforeDropTable();
            query.DuringSymbol(type.TableName);
            return this;
        }

        internal SchemaQueryBuilder AlterTable(TypeMapping type)
        {
            query.AppendAfterSpace("ALTER TABLE ");
            query.DuringSymbol(type.TableName);
            return this;
        }

        internal void AddColumn(PropertyMapping property, string baseName)
        {
            query.BeforeAddColumn();
            AppendColumnName(property, baseName);
            AppendColumnType(property);
        }
    
        private bool commaAdded;

        internal void AddComma()
        {
            if (commaAdded)
            {
                query.Append(',');
            }
            else
            {
                query.AppendSpace();
                commaAdded = true;
            }
        }
        
        internal bool AlterColumns(TypeMapping typeMapping, IList<IColumnDefinition> columns)
        {
            return AlterColumns(typeMapping, columns, null);
        }

        internal bool AlterColumns(TypeMapping mapping, IList<IColumnDefinition> columns, string baseName)
        {
            bool changes = false;
            foreach (var property in mapping.Columns.Values)
            {
                if (property.Reference != null && property.Reference.IsComplexType)
                {
                    changes |= AlterColumns(property.Reference, columns, property.ComposeName(baseName));
                    continue;
                }

                string columnName = property.ComposeName(baseName);
                bool foreign = property.Reference != null;
                
                var existingColumn = columns.SingleOrDefault(c => c.ColumnName.Equals(columnName,StringComparison.OrdinalIgnoreCase));
                if (existingColumn == null)
                {
                    // TODO ugly
                    DuringAlterTable(mapping);

                    AddColumn(property, baseName);
                    changes = true;
                    if (foreign)
                    {
                    //    AddComma();
                      //  query.Append("ADD ");
                        AppendReferences(property);
                    }

                    if (databaseDriver.CanAddIndexInCreateTable())
                    {
                        // TODO add indexes to existing columns if not present
                        if (property.Index != null || foreign)
                        {
                            if (connection.Driver.CanDoMultipleActionsInAlterTable())
                            {
                                AddComma();
                                query.Append("ADD ");
                                if (property.Index != null)
                                {
                                    AppendIndex(property, property.Index, baseName);
                                }
                                else if (foreign)
                                {
                                    AppendIndex(property, property.ColumnName, baseName);
                                }
                            }
                            else
                            {
                                query.Append(";");
                                if (property.Index != null)
                                {
                                    AppendCreateIndex(mapping, property, property.Index, baseName);
                                }
                                else if (foreign)
                                {
                                    AppendCreateIndex(mapping, property, property.ColumnName, baseName);
                                }
                            }
                        }

                    }
                }
                else
                {
                    var columnProperty = foreign ? property.Reference.Key : property;
                    var newType = connection.Driver.GetSqlType(columnProperty, foreign);
                    if (!connection.Driver.EquivalentTypes(newType, existingColumn.ColumnType))
                    {
                        DuringAlterTable(mapping);
                        query.BeforeAlterColumnType(property.ComposeName(baseName));
                        AppendColumnType(property);
                        changes = true;
                    }
                }
            }
            return changes;
        }

        private void DuringAlterTable(TypeMapping type)
        {
            if (databaseDriver.CanDoMultipleActionsInAlterTable())
            {
                AddComma();
            }
            else
            {
                if (commaAdded)
                {
                    query.Append(";");
                    AlterTable(type);
                }
                else
                {
                    commaAdded = true;
                }
            }
        }

        public IFolkeConnection Connection => connection;

        public string Sql => query.ToString();

        public object[] Parameters => null;
    }
}
