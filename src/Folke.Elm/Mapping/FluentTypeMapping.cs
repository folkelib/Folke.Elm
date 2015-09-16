using System;
using System.Linq.Expressions;

namespace Folke.Elm.Mapping
{
    public class FluentTypeMapping<T>
    {
        private readonly TypeMapping typeMapping;

        public FluentTypeMapping(TypeMapping typeMapping)
        {
            this.typeMapping = typeMapping;
        }

        public void ToTable(string name, string schema = null)
        {
            typeMapping.TableName = name;
            typeMapping.TableSchema = schema;
        }

        public void HasKey(Expression<Func<T, object>> expression)
        {
            var propertyInfo = TableHelpers.GetExpressionPropertyInfo(expression);
            typeMapping.Key = typeMapping.Columns[propertyInfo.Name];
            typeMapping.Key.IsKey = true;
            if (propertyInfo.PropertyType == typeof (int) || propertyInfo.PropertyType == typeof(long))
                typeMapping.Key.IsAutomatic = true;
        }

        public FluentPropertyMapping Property(Expression<Func<T, object>> expression)
        {
            var property = TableHelpers.GetExpressionPropertyInfo(expression);
            var propertyMapping = typeMapping.Columns[property.Name];
            return new FluentPropertyMapping(propertyMapping);
        }

        public class FluentPropertyMapping
        {
            private readonly PropertyMapping propertyMapping;

            public FluentPropertyMapping(PropertyMapping propertyMapping)
            {
                this.propertyMapping = propertyMapping;
            }

            public FluentPropertyMapping HasColumnName(string name)
            {
                propertyMapping.ColumnName = name;
                return this;
            }
        }
    }
}