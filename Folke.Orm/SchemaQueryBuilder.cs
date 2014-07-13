using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Folke.Orm
{
    public class SchemaQueryBuilder<T> : QueryBuilder<T> where T : class, new()
    {
        public SchemaQueryBuilder(FolkeConnection connection):base(connection)
        {

        }

        private void AppendColumnName(PropertyInfo property)
        {
            var attributes = property.GetCustomAttribute<ColumnAttribute>();
            query.Append(beginSymbol);
            query.Append(attributes == null ? property.Name : (attributes.Name ?? property.Name));
            if (IsForeign(property.PropertyType))
                query.Append("_id");
            query.Append(endSymbol);
        }

        private void AppendColumn(PropertyInfo property)
        {
            var attributes = property.GetCustomAttribute<ColumnAttribute>();
            AppendColumnName(property);
            query.Append(" ");
            if (IsForeign(property.PropertyType))
                query.Append("INT");
            else if (attributes != null && attributes.MaxLength != 0)
            {
                if (property.PropertyType == typeof(string))
                {
                    if (attributes.MaxLength > 255)
                        query.Append("TEXT");
                    else
                        query.Append("VARCHAR(" + attributes.MaxLength + ")");
                }
                else
                    throw new Exception("MaxLength attribute not supported for " + property.PropertyType);
            }
            else
                query.Append(connection.Driver.GetSqlType(property));
            if (property.Name == "Id")
                query.Append(" PRIMARY KEY AUTO_INCREMENT");
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

        private void AppendForeignKey(PropertyInfo column)
        {
            var attribute = column.GetCustomAttribute<ColumnAttribute>();

            query.Append(" ");
            query.Append("FOREIGN KEY (");
            AppendColumnName(column);
            query.Append(") REFERENCES ");
            AppendTableName(column.PropertyType);
            query.Append("(Id)");
            if (attribute!=null && attribute.OnDelete!= ConstraintEventEnum.Restrict)
            {
                query.Append(" ON DELETE ");
                query.Append(GetConstraintEventString(attribute.OnDelete));
            }
            if (attribute != null && attribute.OnUpdate != ConstraintEventEnum.Restrict)
            {
                query.Append(" ON UPDATE ");
                query.Append(GetConstraintEventString(attribute.OnUpdate));
            }
        }

        private void AppendIndex(PropertyInfo column, string name)
        {
            query.Append(" INDEX ");
            query.Append(beginSymbol);
            query.Append(name);
            query.Append(endSymbol);
            query.Append(" (");
            AppendColumnName(column);
            query.Append(")");            
        }

        public SchemaQueryBuilder<T> CreateTable()
        {
            return CreateTable(typeof(T));
        }

        private static bool DoesForeignTableExist(PropertyInfo property, IList<string> existingTables)
        {
            return existingTables == null || !IsForeign(property.PropertyType) || existingTables.Any(t => t == property.PropertyType.Name.ToLower());
        }

        public SchemaQueryBuilder<T> CreateTable(Type type, IList<string> existingTables = null)
        {
            query.Append("CREATE TABLE ");
            query.Append(beginSymbol);
            query.Append(type.Name);
            query.Append(endSymbol);
            query.Append(" (");
            foreach (var property in type.GetProperties())
            {
                if (IsIgnored(property.PropertyType))
                    continue;

                if (!DoesForeignTableExist(property, existingTables))
                    continue;

                AddComma();
                AppendColumn(property);
            }

            foreach (var property in type.GetProperties())
            {
                if (!DoesForeignTableExist(property, existingTables))
                    continue;

                var attribute = property.GetCustomAttribute<ColumnAttribute>();
                if (attribute != null && attribute.Index != null)
                {
                    AddComma();
                    AppendIndex(property, attribute.Index);
                }
                else if (IsForeign(property.PropertyType))
                {
                    AddComma();
                    AppendIndex(property, property.Name);
                }
            }

            foreach (var property in type.GetProperties())
            {
                if (IsForeign(property.PropertyType) && DoesForeignTableExist(property, existingTables))
                {
                    AddComma();
                    AppendForeignKey(property);
                }
            }

            query.Append(")");
            return this;
        }

        public SchemaQueryBuilder<T> DropTable()
        {
            return DropTable(typeof(T));
        }

        public SchemaQueryBuilder<T> DropTable(Type type)
        {
            query.Append("SET FOREIGN_KEY_CHECKS = 0 ;");// TODO pas propre
            query.Append("DROP TABLE ");
            query.Append(beginSymbol);
            query.Append(type.Name);
            query.Append(endSymbol);
            return this;
        }

        internal SchemaQueryBuilder<T> AlterTable()
        {
            return AlterTable(typeof(T));
        }

        internal SchemaQueryBuilder<T> AlterTable(Type type)
        {
            query.Append("ALTER TABLE ");
            query.Append(beginSymbol);
            query.Append(type.Name);
            query.Append(endSymbol);
            return this;
        }

        internal void AddColumn(PropertyInfo property)
        {
            query.Append("ADD COLUMN ");
            AppendColumn(property);
        }
    
        private bool commaAdded = false;

        internal void AddComma()
        {
 	        if (commaAdded)
                query.Append(',');
            else
                commaAdded = true;
        }

        internal bool AlterColumns(IList<InformationSchema.Columns> columns)
        {
            return AlterColumns(typeof(T), columns);
        }

        internal bool AlterColumns(Type type, IList<InformationSchema.Columns> columns)
        {
            bool changes = false;
            
 	        foreach (var property in type.GetProperties())
            {
                if (IsIgnored(property.PropertyType))
                    continue;

                var attribute = property.GetCustomAttribute<ColumnAttribute>();
                var columnName = attribute == null ? property.Name : (attribute.Name ?? property.Name);
                bool foreign = IsForeign(property.PropertyType);
                if (foreign)
                    columnName += "_id";
                var existingColumn = columns.SingleOrDefault(c => c.COLUMN_NAME == columnName);
                if (existingColumn == null)
                {
                    AddComma();
                    AddColumn(property);
                    changes = true;
                    if (attribute != null && attribute.Index != null)
                    {
                        AddComma();
                        query.Append("ADD ");
                        AppendIndex(property, attribute.Index);
                    }
                    else if (foreign)
                    {
                        AddComma();
                        query.Append("ADD ");
                        AppendIndex(property, property.Name);
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
                    var newType = foreign ? "INT" : connection.Driver.GetSqlType(property);
                    if (!connection.Driver.EquivalentTypes(newType, existingColumn.COLUMN_TYPE))
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
