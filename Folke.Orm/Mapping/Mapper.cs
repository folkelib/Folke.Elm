using System;
using System.Collections.Generic;
using System.Reflection;

namespace Folke.Orm.Mapping
{
    public class Mapper : IMapper
    {
        private readonly IDictionary<Type, TypeMapping> typeMappings = new Dictionary<Type, TypeMapping>();

        public Mapper()
        {
        }

        /// <summary>
        /// Get the mapping of a type to a table. Create it if it did not exist.
        /// </summary>
        /// <param name="type">The type to map</param>
        /// <returns>The mapping</returns>
        public TypeMapping GetTypeMapping(Type type)
        {
            if (type == typeof(object))
                throw new Exception("Unexpected type to map");
            return !this.typeMappings.ContainsKey(type) ? this.MapType(type) : this.typeMappings[type];
        }

        /// <summary>
        /// Add a new mapping, or replace an existing mapping
        /// </summary>
        /// <param name="mapping"></param>
        public void AddMapping(TypeMapping mapping)
        {
            typeMappings[mapping.Type] = mapping;
        }

        private TypeMapping MapType(Type type)
        {
            var newMapping = new TypeMapping(type, this);
            typeMappings[type] = newMapping;
            
            return newMapping;
        }

        public string GetColumnName(MemberInfo memberInfo)
        {
            return GetTypeMapping(memberInfo.DeclaringType).Columns[memberInfo.Name].ColumnName;
        }

        public PropertyMapping GetKey(Type type)
        {
            return GetTypeMapping(type).Key;
        }

        public bool IsMapped(Type type)
        {
            return type.GetInterface("IFolkeTable") != null || type.GetCustomAttribute<TableAttribute>() != null;
        }
    }
}
