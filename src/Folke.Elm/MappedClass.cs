using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Folke.Elm.Mapping;

namespace Folke.Elm
{
    /// <summary>A class that has been mapped to selected fields</summary>
    public class MappedClass
    {
        private readonly IList<MappedField> fields = new List<MappedField>();
        private IList<MappedCollection> collections;
        private MappedField primaryKeyField;
        private ConstructorInfo constructor;

        /// <summary>
        /// Construct an object, fill its primary key with the given id, and create any auto collection
        /// </summary>
        /// <param name="connection">The connection (for the auto collection constructor)</param>
        /// <param name="type">The type to construct</param>
        /// <param name="id">The primary key value</param>
        /// <returns>The object, whose primary key and collections are filled</returns>
        public object Construct(IFolkeConnection connection, Type type, object id)
        {
            var ret = constructor.Invoke(null);

            primaryKeyField?.PropertyInfo.SetValue(ret, id);

            if (collections != null)
            {
                foreach (var collection in collections)
                {
                    collection.propertyInfo.SetValue(ret, collection.listConstructor.Invoke(new[] { connection, type, id, collection.listJoins }));
                }
            }
            return ret;
        }

        public object Read(IFolkeConnection folkeConnection, Type type, DbDataReader reader, object expectedId = null)
        {
            var cache = folkeConnection.Cache;
            object value;
            var idMappedField = primaryKeyField;

            // If the key field is mapped or if its value is already known, create a new item and
            // store it in cache
            if (idMappedField != null && (idMappedField.SelectedField != null || expectedId != null))
            {
                if (!cache.ContainsKey(type.Name))
                    cache[type.Name] = new Dictionary<object, object>();
                var typeCache = cache[type.Name];

                object id;

                if (idMappedField.SelectedField != null)
                {
                    var index = idMappedField.SelectedField.Index;

                    if (expectedId == null && reader.IsDBNull(index))
                        return null;

                    // Do like this because GetTypedValue does not seem to work with MySql and System.Guid
                    id = folkeConnection.Driver.ConvertReaderValueToProperty(reader.GetValue(index), idMappedField.PropertyInfo.PropertyType);
                    if (expectedId != null && !id.Equals(expectedId))
                        throw new Exception("Unexpected id");
                }
                else
                {
                    id = expectedId;
                }

                if (typeCache.ContainsKey(id))
                {
                    value = typeCache[id];
                }
                else
                {
                    value = Construct(folkeConnection, type, id);
                    typeCache[id] = value;
                }
            }
            else
            {
                value = Construct(folkeConnection, type, 0);
            }

            foreach (var mappedField in fields)
            {
                var fieldInfo = mappedField.SelectedField;
                
                if (fieldInfo != null && reader.IsDBNull(fieldInfo.Index))
                    continue;
                
                if (mappedField.MappedClass == null)
                {
                    if (fieldInfo == null)
                        throw new Exception("Unknown error");
                    object field = folkeConnection.Driver.ConvertReaderValueToValue(reader, mappedField.PropertyInfo.PropertyType, fieldInfo.Index);
                    mappedField.PropertyInfo.SetValue(value, field);
                }
                else 
                {
                    object id = fieldInfo == null ? null : reader.GetValue(fieldInfo.Index);
                    if (id != null)
                        id = folkeConnection.Driver.ConvertReaderValueToProperty(id, mappedField.MappedClass.primaryKeyField.PropertyInfo.PropertyType);
                    object other = mappedField.MappedClass.Read(folkeConnection, mappedField.PropertyInfo.PropertyType, reader, id);
                    mappedField.PropertyInfo.SetValue(value, other);
                }
            }
            return value;
        }

        /// <summary>A factory for a MappedClass instance</summary>
        /// <param name="fieldAliases">The fields that have been selected in the query and that should fill the class properties</param>
        /// <param name="selectedTable">The table of the fields to map</param>
        /// <returns>The mapping from the database query to the instancied object</returns>
        public static MappedClass MapClass(IList<SelectedField> fieldAliases, TypeMapping type, SelectedTable selectedTable)
        {
            if (fieldAliases == null)
                return null;

            var mappedClass = new MappedClass();

            var idProperty = type.Key;
            mappedClass.constructor = type.Type.GetTypeInfo().GetConstructor(Type.EmptyTypes);
            if (idProperty != null)
            {
                var selectedField = fieldAliases.SingleOrDefault(f => f.Table == selectedTable && f.PropertyMapping == idProperty);
                mappedClass.primaryKeyField = new MappedField { SelectedField = selectedField, PropertyInfo = idProperty.PropertyInfo };
            }

            foreach (var columnPair in type.Columns)
            {
                var propertyMapping = columnPair.Value;
                if (idProperty != null && propertyMapping == idProperty)
                    continue;

                var fieldInfo = fieldAliases.SingleOrDefault(f => f.Table == selectedTable && f.PropertyMapping == propertyMapping);
                bool isForeign = propertyMapping.Reference != null;
                if (fieldInfo != null /*|| isForeign && fieldAliases.Any(x => x.PropertyMapping == propertyMapping.Reference.Key)*/)
                {
                    var mappedField = new MappedField { PropertyInfo = propertyMapping.PropertyInfo, SelectedField = fieldInfo };

                    if (isForeign)
                    {
                        if (fieldInfo != null)
                        {
                            mappedField.MappedClass = MapClass(fieldAliases, propertyMapping.Reference,
                                fieldInfo.ChildTable);
                        }
                        else
                        {
                            mappedField.MappedClass = new MappedClass
                            {
                                primaryKeyField = mappedField,
                                constructor = propertyMapping.Reference.Type.GetTypeInfo().GetConstructor(Type.EmptyTypes)
                            };
                            MapCollections(propertyMapping.Reference, mappedField.MappedClass);
                        }
                    }
                    mappedClass.fields.Add(mappedField);
                }
                else if (isForeign)
                {
                    // If the foreign table id table is selected, use this
                    //fieldInfo = fieldAliases.FirstOrDefault(x => x. == )
                }
            }

            MapCollections(type, mappedClass);
            
            return mappedClass;
        }

        private static void MapCollections(TypeMapping type, MappedClass mappedClass)
        {
            if (type.Collections.Any())
            {
                mappedClass.collections = new List<MappedCollection>();
                foreach (var collection in type.Collections.Values)
                {
                    mappedClass.collections.Add(collection);
                }
            }
        }
    }
}