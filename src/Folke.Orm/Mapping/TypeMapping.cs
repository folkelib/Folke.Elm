using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Folke.Orm.Mapping
{
    public class TypeMapping
    {
        public Type Type { get; set; }

        public string TableName { get; set; }

        public string TableSchema { get; set; }

        public PropertyMapping Key { get; set; }

        public Dictionary<string, PropertyMapping> Columns { get; set; }

        public Dictionary<string, MappedCollection> Collections { get; set; }

        public TypeMapping(Type type, IMapper mapper)
        {
            Type = type;
            mapper.AddMapping(this);
            Columns = new Dictionary<string, PropertyMapping>();
            Collections = new Dictionary<string, MappedCollection>();
            var tableAttribute = type.GetTypeInfo().GetCustomAttribute<TableAttribute>();
            if (tableAttribute != null)
            {
                TableName = tableAttribute.Name;
                TableSchema = tableAttribute.Schema;
            }
            else
            {
                TableName = type.Name;
            }
            
            foreach (var propertyInfo in type.GetProperties())
            {
                if (propertyInfo.GetCustomAttribute<NotMappedAttribute>() != null)
                    continue;
                
                var propertyType = propertyInfo.PropertyType;
                bool nullable = false;
                if (Nullable.GetUnderlyingType(propertyType) != null)
                {
                    propertyType = Nullable.GetUnderlyingType(propertyType);
                    nullable = true;
                }

                if (propertyType.GetTypeInfo().IsGenericType)
                {
                    if (propertyType.GetInterfaces().FirstOrDefault(x => x.Name == "IEnumerable") != null)
                    {
                        var foreignType = propertyType.GenericTypeArguments[0];
                        if (mapper.IsMapped(foreignType))
                        {
                            var folkeList = typeof(FolkeList<>).MakeGenericType(foreignType);
                            if (propertyType.IsAssignableFrom(folkeList))
                            {
                                var joins =
                                    propertyInfo.GetCustomAttributes<FolkeListAttribute>().Select(x => x.Join).ToArray();
                                var constructor =
                                    folkeList.GetConstructor(new[] { typeof(IFolkeConnection), typeof(Type), typeof(int), typeof(string[]) });
                                var mappedCollection = new MappedCollection
                                {
                                    propertyInfo = propertyInfo,
                                    listJoins = joins,
                                    listConstructor = constructor
                                };
                                Collections[propertyInfo.Name] = mappedCollection;
                            }
                        }
                        continue;
                    }
                }

                var propertyMapping = new PropertyMapping(propertyInfo) {Nullable = nullable};
                Columns[propertyInfo.Name] = propertyMapping;

                if (mapper.IsMapped(propertyInfo.PropertyType))
                {
                    propertyMapping.Reference = mapper.GetTypeMapping(propertyInfo.PropertyType);
                }

                var columnConstraintAttribute = propertyInfo.GetCustomAttribute<ColumnConstraintAttribute>();
                if (columnConstraintAttribute != null)
                {
                    propertyMapping.OnDelete = columnConstraintAttribute.OnDelete;
                    propertyMapping.OnUpdate = columnConstraintAttribute.OnUpdate;
                }

                var columnAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                if (columnAttribute != null)
                {
                    propertyMapping.ColumnName = columnAttribute.Name;
                }

                var maxLengthAttribute = propertyInfo.GetCustomAttribute<MaxLengthAttribute>();
                if (maxLengthAttribute != null)
                {
                    propertyMapping.MaxLength = maxLengthAttribute.Length;
                }

                if (propertyMapping.ColumnName == null)
                {
                    if (propertyMapping.Reference != null)
                        propertyMapping.ColumnName = propertyInfo.Name + "_id";
                    else
                        propertyMapping.ColumnName = propertyInfo.Name;
                }

                var indexAttribute = propertyInfo.GetCustomAttribute<IndexAttribute>();
                if (indexAttribute != null)
                {
                    propertyMapping.Index = indexAttribute.Name ?? TableName + "_" + propertyMapping.ColumnName;
                }

                if ((propertyInfo.Name == "Id" && type.GetInterfaces().FirstOrDefault(x => x.Name == "IFolkeTable") != null) ||
                    propertyInfo.GetCustomAttribute<KeyAttribute>() != null)
                {
                    Key = propertyMapping;
                    Key.IsKey = true;
                    if (propertyInfo.PropertyType == typeof(int) || propertyInfo.PropertyType == typeof(long))
                        Key.IsAutomatic = true;
                }

                propertyMapping.Readonly = propertyMapping.IsAutomatic;

                
            }
        }
    }
}
