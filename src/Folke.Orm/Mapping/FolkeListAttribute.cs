using System;

namespace Folke.Orm.Mapping
{
    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple=true)]
    public class FolkeListAttribute : Attribute
    {
        public string Join { get; set; }
    }
}
