using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Folke.Orm
{
    public class MappedClass<T, TMe> where T : class, new()
    {
        public readonly IList<MappedField<T, TMe>> fields = new List<MappedField<T, TMe>>();
        public IList<MappedCollection> collections;
        public MappedField<T, TMe> idField;
        public ConstructorInfo constructor;

        public object Construct(IFolkeConnection connection, Type type, object id)
        {
            var ret = constructor.Invoke(null);
                
            if (idField != null)
                idField.propertyInfo.SetValue(ret, id);

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
            var idMappedField = idField;

            // If the key field is mapped or if its value is already known, create a new item and
            // store it in cache
            if (idMappedField != null && (idMappedField.selectedField != null || expectedId != null))
            {
                if (!cache.ContainsKey(type.Name))
                    cache[type.Name] = new Dictionary<object, object>();
                var typeCache = cache[type.Name];

                object id;

                if (idMappedField.selectedField != null)
                {
                    var index = idMappedField.selectedField.index;

                    if (expectedId == null && reader.IsDBNull(index))
                        return null;

                    id = reader.GetValue(index);
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
                var fieldInfo = mappedField.selectedField;
                
                if (fieldInfo != null && reader.IsDBNull(fieldInfo.index))
                    continue;
                
                if (mappedField.mappedClass == null)
                {
                    if (fieldInfo == null)
                        throw new Exception("Unknown error");
                    object field = reader.GetTypedValue(mappedField.propertyInfo.PropertyType, fieldInfo.index);
                    mappedField.propertyInfo.SetValue(value, field);
                }
                else 
                {
                    object id = fieldInfo == null ? null : reader.GetValue(fieldInfo.index);
                    object other = mappedField.mappedClass.Read(folkeConnection, mappedField.propertyInfo.PropertyType, reader, id);
                    mappedField.propertyInfo.SetValue(value, other);
                }
            }
            return value;
        }

        public static MappedClass<T, TMe> MapClass(IList<BaseQueryBuilder<T, TMe>.FieldAlias> fieldAliases, Type type, string alias = null)
        {
            if (fieldAliases == null)
                return null;

            var mappedClass = new MappedClass<T, TMe>();

            var idProperty = TableHelpers.GetKey(type);
            mappedClass.constructor = type.GetConstructor(Type.EmptyTypes);
            if (idProperty != null)
            {
                var selectedField = fieldAliases.SingleOrDefault(f => f.alias == alias && f.propertyInfo == idProperty);
                mappedClass.idField = new MappedField<T, TMe> { selectedField = selectedField, propertyInfo = idProperty };
            }
            
            foreach (var property in type.GetProperties())
            {
                if (property == idProperty)
                    continue;

                var propertyType = property.PropertyType;
                if (Nullable.GetUnderlyingType(propertyType) != null)
                {
                    propertyType = Nullable.GetUnderlyingType(propertyType);
                }

                
                if (propertyType.IsGenericType)
                {
                    var foreignType = propertyType.GenericTypeArguments[0];
                    var folkeList = typeof(FolkeList<>).MakeGenericType(foreignType);
                    if (property.PropertyType.IsAssignableFrom(folkeList))
                    {
                        var joins = property.GetCustomAttributes<FolkeListAttribute>().Select(x => x.Join).ToArray();
                        var constructor = folkeList.GetConstructor(new[] { typeof(IFolkeConnection), typeof(Type), typeof(int), typeof(string[]) });
                        var mappedCollection = new MappedCollection { propertyInfo = property, listJoins = joins, listConstructor = constructor };
                        if (mappedClass.collections == null)
                            mappedClass.collections = new List<MappedCollection>();
                        mappedClass.collections.Add(mappedCollection);
                    }
                }
                else if (!TableHelpers.IsIgnored(propertyType))
                {
                    var fieldInfo = fieldAliases.SingleOrDefault(f => f.alias == alias && f.propertyInfo == property);
                    bool isForeign = TableHelpers.IsForeign(property.PropertyType);
                    if (fieldInfo != null || (isForeign && (mappedClass.idField == null || mappedClass.idField.selectedField != null)))
                    {
                        var mappedField = new MappedField<T, TMe> { propertyInfo = property, selectedField = fieldInfo };

                        if (TableHelpers.IsForeign(property.PropertyType))
                        {
                            mappedField.mappedClass = MapClass(fieldAliases, property.PropertyType, alias == null ? property.Name : alias + "." + property.Name);
                        }
                        mappedClass.fields.Add(mappedField);
                    }
                }
            }
            return mappedClass;
        }
    }
}