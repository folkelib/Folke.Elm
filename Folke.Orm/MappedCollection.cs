using System.Reflection;

namespace Folke.Orm
{
    public class MappedCollection
    {
        public string[] listJoins;
        public ConstructorInfo listConstructor;
        public PropertyInfo propertyInfo;
    }
}