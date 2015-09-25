using System;

namespace Folke.Elm.Mapping
{
    /// <summary>
    /// Place this attribute on a IEnumerable to specify options when doing a select
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple=true)]
    public class SelectAttribute : Attribute
    {
        /// <summary>
        /// The name of a reference property in the list element type that must loaded when the element is laoded
        /// </summary>
        public string IncludeReference { get; set; }
    }
}
