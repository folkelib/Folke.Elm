using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Folke.Elm.Mapping
{
    /// <summary>The mapping from a class to a table</summary>
    public class TypeMapping
    {
        public bool IsComplexType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeMapping"/> class.
        /// </summary>
        /// <param name="type">The type to map</param>
        public TypeMapping(Type type)
        {
            Type = type;
            Columns = new Dictionary<string, PropertyMapping>();
            Collections = new Dictionary<string, MappedCollection>();
        }

        /// <summary>
        /// Maps automatically the properties using the attributes as hints
        /// </summary>
        /// <param name="mapper"></param>
        public void AutoMap(IMapper mapper)
        {
            var typeInfo = Type.GetTypeInfo();
            var tableAttribute = typeInfo.GetCustomAttribute<TableAttribute>();
            if (tableAttribute != null)
            {
                TableName = tableAttribute.Name;
                TableSchema = tableAttribute.Schema;
            }
            else
            {
                TableName = Regex.Replace(Type.Name, @"`\d+", "");
            }

            IsComplexType = typeInfo.GetCustomAttribute<ComplexTypeAttribute>() != null;

            foreach (var propertyInfo in typeInfo.GetProperties())
            {
                if (propertyInfo.GetCustomAttribute<NotMappedAttribute>() != null)
                    continue;
                
                var propertyType = propertyInfo.PropertyType;
                var nullable = false;
                if (Nullable.GetUnderlyingType(propertyType) != null)
                {
                    propertyType = Nullable.GetUnderlyingType(propertyType);
                    nullable = true;
                }

                var propertyTypeInfo = propertyType.GetTypeInfo();
                if (propertyTypeInfo.IsGenericType)
                {
                    if (propertyTypeInfo.GetInterfaces().FirstOrDefault(x => x.Name == nameof(IEnumerable)) != null)
                    {
                        var foreignType = propertyType.GenericTypeArguments[0];
                        if (mapper.IsMapped(foreignType))
                        {
                            var folkeList = typeof(FolkeList<>).MakeGenericType(foreignType);
                            if (propertyTypeInfo.IsAssignableFrom(folkeList))
                            {
                                var joins =
                                    propertyInfo.GetCustomAttributes<SelectAttribute>().Select(x => x.IncludeReference).ToArray();
                                var constructor =
                                    folkeList.GetTypeInfo().GetConstructor(new[] { typeof(IFolkeConnection), typeof(Type), typeof(object), typeof(string[]) });
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

                var propertyMapping = new PropertyMapping(propertyInfo) { Nullable = nullable };
                Columns[propertyInfo.Name] = propertyMapping;

                if (mapper.IsMapped(propertyInfo.PropertyType))
                {
                    var typeMapping = mapper.GetTypeMapping(propertyInfo.PropertyType);
                    propertyMapping.Reference = typeMapping;
                }

                var columnConstraintAttribute = propertyInfo.GetCustomAttribute<ColumnConstraintAttribute>();
                if (columnConstraintAttribute != null)
                {
                    propertyMapping.OnDelete = columnConstraintAttribute.OnDelete;
                    propertyMapping.OnUpdate = columnConstraintAttribute.OnUpdate;
                }
                else
                {
                    propertyMapping.OnDelete = ConstraintEventEnum.NoAction;
                    propertyMapping.OnUpdate = ConstraintEventEnum.NoAction;
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
                    if (propertyMapping.Reference != null && !propertyMapping.Reference.IsComplexType)
                        propertyMapping.ColumnName = propertyInfo.Name + "_id";
                    else
                        propertyMapping.ColumnName = propertyInfo.Name;
                }

                var indexAttribute = propertyInfo.GetCustomAttribute<IndexAttribute>();
                if (indexAttribute != null)
                {
                    propertyMapping.Index = indexAttribute.Name ?? TableName + "_" + propertyMapping.ColumnName;
                }

                if ((propertyInfo.Name == nameof(IFolkeTable.Id) && typeInfo.GetInterfaces().FirstOrDefault(x => x.Name == nameof(IFolkeTable)) != null) ||
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

        /// <summary>Gets or sets the type that is mapped</summary>
        public Type Type { get; set; }

        /// <summary>Gets or sets the table name</summary>
        public string TableName { get; set; }

        /// <summary>Gets or sets the table schema</summary>
        public string TableSchema { get; set; }

        /// <summary>Gets or sets the mapping to the primary key</summary>
        public PropertyMapping Key { get; set; }

        /// <summary>Gets or sets the columns, mapped by name</summary>
        public Dictionary<string, PropertyMapping> Columns { get; set; }

        /// <summary>Gets or sets the list of collections</summary>
        public Dictionary<string, MappedCollection> Collections { get; set; }

        public PropertyMapping GetColumn(MemberInfo memberInfo)
        {
            return Columns[memberInfo.Name];
        }

        public override string ToString()
        {
            return $"{Type.Name}";
        }
    }
}
