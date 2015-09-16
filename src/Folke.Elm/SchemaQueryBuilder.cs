using System;
using System.Collections.Generic;
using System.Linq;
using Folke.Elm.InformationSchema;
using Folke.Elm.Mapping;

namespace Folke.Elm
{
    public class SchemaQueryBuilder<T> : BaseQueryBuilder<T> where T : class, new()
    {
        public SchemaQueryBuilder(FolkeConnection connection):base(connection)
        {
        }

        private void AppendColumnName(PropertyMapping property)
        {
            query.AppendSymbol(property.ColumnName);
        }

        private void AppendColumn(PropertyMapping property)
        {
            AppendColumnName(property);
            query.Append(" ");

            if (property.Reference != null)
            {
                var foreignPrimaryKey = property.Reference.Key;
                query.Append(Connection.Driver.GetSqlType(foreignPrimaryKey.PropertyInfo, foreignPrimaryKey.MaxLength));
                query.AppendAfterSpace("NULL");
            }
            else if (property.MaxLength != 0)
            {
                if (property.PropertyInfo.PropertyType == typeof(string))
                {
                    if (property.MaxLength > 255)
                        query.Append("TEXT");
                    else
                        query.Append("VARCHAR(" + property.MaxLength + ")");
                }
                else
                    throw new Exception("MaxLength attribute not supported for " + property.PropertyInfo.PropertyType);
            }
            else
                query.Append(Connection.Driver.GetSqlType(property.PropertyInfo, property.MaxLength));

            if (property.IsKey)
            {
                query.Append(" PRIMARY KEY");
                if (property.IsAutomatic)
                {
                    query.AppendAutoIncrement();
                }
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

        private void AppendForeignKey(PropertyMapping column)
        {
            query.Append(" ");
            query.Append("FOREIGN KEY (");
            AppendColumnName(column);
            query.Append(") REFERENCES ");
            AppendTableName(column.Reference);
            query.Append("(");
            query.Append(column.Reference.Key.ColumnName);
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

        private void AppendIndex(PropertyMapping column, string name)
        {
            query.Append(" INDEX ");
            query.AppendSymbol(name);
            query.Append(" (");
            AppendColumnName(column);
            query.Append(")");            
        }

        private void AppendCreateIndex(TypeMapping table, PropertyMapping column, string name)
        {
            query.Append("CREATE INDEX ");
            query.AppendSymbol(name);
            query.Append(" ON ");
            query.AppendSymbol(table.TableName);
            query.Append("(");
            AppendColumnName(column);
            query.Append(")");
        }

        public SchemaQueryBuilder<T> CreateTable()
        {
            return CreateTable(typeof(T));
        }

        private static bool DoesForeignTableExist(PropertyMapping property, IEnumerable<string> existingTables)
        {
            return existingTables == null || property.Reference == null || existingTables.Any(t => t == property.Reference.TableName.ToLower());
        }

        private string GetAutoIndexName(TypeMapping table, PropertyMapping column)
        {
            return $"{table.TableName}_${column.ColumnName}";
        }

        public SchemaQueryBuilder<T> CreateTable(Type type, IList<string> existingTables = null)
        {
            var mapping = Mapper.GetTypeMapping(type);
            query.Append("CREATE TABLE ");
            query.AppendSymbol(mapping.TableName);
            query.Append(" (");
            bool canCreateIndex = driver.CanAddIndexInCreateTable();

            foreach (var column in mapping.Columns.Values)
            {
                if (!DoesForeignTableExist(column, existingTables))
                    continue;

                AddComma();
                AppendColumn(column);
            }

            if (canCreateIndex)
            {
                foreach (var property in mapping.Columns.Values)
                {
                    if (!DoesForeignTableExist(property, existingTables))
                        continue;

                    if (property.Index != null)
                    {
                        AddComma();
                        AppendIndex(property, property.Index);
                    }
                    else if (property.Reference != null)
                    {
                        AddComma();
                        AppendIndex(property, GetAutoIndexName(mapping, property));
                    }
                }

            }

            foreach (var property in mapping.Columns.Values)
            {
                if (property.Reference != null && DoesForeignTableExist(property, existingTables))
                {
                    AddComma();
                    AppendForeignKey(property);
                }
            }

            query.Append(")");

            if (!canCreateIndex)
            {
                foreach (var property in mapping.Columns.Values)
                {
                    if (property.Index != null)
                    {
                        query.Append(";");
                        AppendCreateIndex(mapping, property, property.Index);
                    }
                    else if (property.Reference != null)
                    {
                        query.Append(";");
                        AppendCreateIndex(mapping, property, GetAutoIndexName(mapping, property));
                    }
                }
            }
            return this;
        }

        public SchemaQueryBuilder<T> DropTable()
        {
            return DropTable(typeof(T));
        }

        public SchemaQueryBuilder<T> DropTable(Type type)
        {
            query.AppendDropTable(Mapper.GetTypeMapping(type).TableName);
            return this;
        }

        internal SchemaQueryBuilder<T> AlterTable()
        {
            return AlterTable(typeof(T));
        }

        internal SchemaQueryBuilder<T> AlterTable(Type type)
        {
            query.Append("ALTER TABLE ");
            query.AppendSymbol(Mapper.GetTypeMapping(type).TableName);
            return this;
        }

        internal void AddColumn(PropertyMapping property)
        {
            query.Append("ADD COLUMN ");
            AppendColumn(property);
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

        internal bool AlterColumns(IList<ColumnDefinition> columns)
        {
            return AlterColumns(typeof(T), columns);
        }

        internal bool AlterColumns(Type type, IList<ColumnDefinition> columns)
        {
            bool changes = false;
            var mapping = Mapper.GetTypeMapping(type);
            
 	        foreach (var property in mapping.Columns.Values)
            {
                string columnName = property.ColumnName;
                bool foreign = property.Reference != null;
                
                var existingColumn = columns.SingleOrDefault(c => c.ColumnName.Equals(columnName,StringComparison.OrdinalIgnoreCase));
                if (existingColumn == null)
                {
                    AddComma();
                    AddColumn(property);
                    changes = true;
                    if (property.Index != null)
                    {
                        AddComma();
                        query.Append("ADD ");
                        AppendIndex(property, property.Index);
                    }
                    else if (foreign)
                    {
                        AddComma();
                        query.Append("ADD ");
                        AppendIndex(property, property.ColumnName);
                    }

                    if (foreign)
                    {
                        AddComma();
                        query.Append("ADD ");
                        AppendForeignKey(property);
                    }
                }
                else
                {
                    var newType = foreign ? "INT" : Connection.Driver.GetSqlType(property.PropertyInfo, property.MaxLength);
                    if (!Connection.Driver.EquivalentTypes(newType, existingColumn.ColumnType))
                    {
                        AddComma();
                    
                        query.Append(" CHANGE COLUMN ");
                        AppendColumnName(property);
                        query.Append(" ");
                        AppendColumn(property);
                        changes = true;
                    }
                }
            }
            return changes;
        }
    }
}
