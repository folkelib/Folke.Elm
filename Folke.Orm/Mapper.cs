namespace Folke.Orm
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class Mapper
    {
        private readonly IDictionary<Type, TypeMapping> typeMappings = new Dictionary<Type, TypeMapping>();

        private readonly string schema;

        public Mapper(string schema)
        {
            this.schema = schema;
        }

        public TypeMapping GetTypeMapping(Type type)
        {
            return !this.typeMappings.ContainsKey(type) ? this.MapType(type) : this.typeMappings[type];
        }

        private TypeMapping MapType(Type type)
        {
            var newMapping = new TypeMapping();
            typeMappings[type] = newMapping;
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            if (tableAttribute != null)
            {
                newMapping.TableName = tableAttribute.Name;
                newMapping.TableSchema = tableAttribute.Schema ?? schema;
            }
            else
            {
                newMapping.TableName = type.Name;
                newMapping.TableSchema = schema;
            }

            return newMapping;
        }
    }
}
