using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Folke.Elm.Mapping
{
    public class Mapper : IMapper
    {
        private readonly IDictionary<Type, TypeMapping> typeMappings = new Dictionary<Type, TypeMapping>();

        /// <inheritdoc/>
        public TypeMapping GetTypeMapping(Type type)
        {
            if (type == typeof(object))
                throw new InvalidOperationException("Unexpected type to map");
            TypeMapping typeMapping;
            if (typeMappings.TryGetValue(type, out typeMapping))
            {
                return typeMapping;
            }

            typeMapping = new TypeMapping(type, this);
            typeMappings.Add(type, typeMapping);
            return typeMapping;
        }

        public IEnumerable<TypeMapping> GetTypeMappings(Assembly assembly)
        {
            var types = assembly.DefinedTypes.Where(x => x.IsClass && IsMapped(x.AsType()));
            return types.Select(x => GetTypeMapping(x.AsType()));
        }

        public FluentTypeMapping<T> GetTypeMapping<T>()
        {
            return new FluentTypeMapping<T>(GetTypeMapping(typeof(T)));
        }
        
        /// <inheritdoc/>
        public IEnumerable<TypeMapping> GetTypeMappings()
        {
            return typeMappings.Values;
        }
        
        public bool IsMapped(Type type)
        {
            return typeMappings.ContainsKey(type) || type.GetInterfaces().FirstOrDefault(x => x.Name == "IFolkeTable") != null 
                || type.GetTypeInfo().GetCustomAttribute<TableAttribute>() != null;
        }
    }
}
