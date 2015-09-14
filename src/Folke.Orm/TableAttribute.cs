namespace Folke.Orm
{
    using System;

    [System.AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public TableAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public string Schema { get; set; }
    }
}
