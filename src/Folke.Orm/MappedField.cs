using System.Reflection;

namespace Folke.Orm
{
    public class MappedField
    {
        public BaseQueryBuilder.FieldAlias selectedField;
        public PropertyInfo propertyInfo;
        public MappedClass mappedClass;
    }
}