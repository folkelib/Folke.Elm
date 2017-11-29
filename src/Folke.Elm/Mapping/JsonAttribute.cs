using System;

namespace Folke.Elm.Mapping
{
    /// <summary>
    /// This property must be serialized in database as a JSON string
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property)]
    public class JsonAttribute : Attribute
    {
    }
}
