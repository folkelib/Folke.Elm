using System.Reflection;

namespace Folke.Elm
{
    public class MappedField
    {
        public BaseQueryBuilder.FieldAlias selectedField;
        public PropertyInfo propertyInfo;
        public MappedClass mappedClass;
    }
}