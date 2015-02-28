using System.Reflection;

namespace Folke.Orm
{
    public class MappedField<T, TMe> where T : class, new()
    {
        public BaseQueryBuilder<T, TMe>.FieldAlias selectedField;
        public PropertyInfo propertyInfo;
        public MappedClass<T, TMe> mappedClass;
    }
}