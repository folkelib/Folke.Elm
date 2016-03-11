using System;

namespace Folke.Elm.Mapping
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Reflection;

    /// <summary>
    /// This interface holds the mappings between .NET objects and 
    /// the database.
    /// </summary>
    public interface IMapper
    {
        /// <summary>
        /// Gets a mapping from a .NET type to a database table.
        /// </summary>
        /// <param name="type">The type to map</param>
        /// <returns>The mapping</returns>
        TypeMapping GetTypeMapping(Type type);

        /// <summary>
        /// Checks if this type is mapped to the database.
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns><value>true</value> if it is mapped. <value>false</value> otherwise.</returns>
        bool IsMapped(Type type);
        
        /// <summary>
        /// Gets all the mapped types.
        /// </summary>
        /// <returns>The mapped types</returns>
        IEnumerable<TypeMapping> GetTypeMappings();

        /// <summary>
        /// Gets all the mapped types from an assembly. It is all the types that implements <see cref="IFolkeTable"/> 
        /// or have a <see cref="TableAttribute"/> attribute.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns>The mapped types</returns>
        IEnumerable<TypeMapping> GetTypeMappings(Assembly assembly);

            /// <summary>
        /// Gets a mapping from a .NET type to a database table in order to modify it.
        /// </summary>
        /// <typeparam name="T">The type to map to the database</typeparam>
        /// <returns>An object that helps to change the mapping</returns>
        FluentTypeMapping<T> GetTypeMapping<T>();
    }
}
