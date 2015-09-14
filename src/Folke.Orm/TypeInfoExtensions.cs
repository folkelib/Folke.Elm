using System;
using System.Linq;
using System.Reflection;

namespace Folke.Orm
{
    public static class TypeInfoExtensions
    {
        public static ConstructorInfo GetConstructor(this TypeInfo typeInfo)
        {
            return typeInfo.DeclaredConstructors.First(x => x.GetParameters().Length == 0);
        }

        public static ConstructorInfo GetConstructor(this TypeInfo typeInfo, params Type[] parameterTypes)
        {
            return typeInfo.DeclaredConstructors.First(x => {
                var parameters = x.GetParameters();
                if (parameters.Length != parameterTypes.Length) return false;
                for (var i = 0; i< parameters.Length; i++)
                {
                    if (parameters[i].ParameterType != parameterTypes[i])
                        return false;
                }
                return true;
                });
        }

        public static Type GetInterface(this TypeInfo typeInfo, Type interfaceType)
        {
            return typeInfo.ImplementedInterfaces.FirstOrDefault(x => x == interfaceType);
        }

        public static bool IsAssignableFrom(this TypeInfo typeInfo, Type assignedType)
        {
            if (assignedType.GetTypeInfo().GetInterface(typeInfo.AsType()) != null)
                return true;
            return false;
        }
    }
}
