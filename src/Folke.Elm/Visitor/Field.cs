using System;
using Folke.Elm.Mapping;

namespace Folke.Elm.Visitor
{
    /// <summary>
    /// A column name with the table name.
    /// </summary>
    public class Field : IVisitable
    {
        public SelectedTable Table { get; set; }
        public PropertyMapping Column { get; set; }

        public Field(SelectedTable table, PropertyMapping column)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            Table = table;
            Column = column;
        }

        public void Accept(IVisitor visitor)
        {
            var alias = Table;
            var name = Column.ColumnName;
            while (alias.Mapping.IsComplexType)
            {
                name = alias.ParentMember.ComposeNameReverse(name);
                alias = alias.Parent;
            }
                
            visitor.DuringColumn(alias.Alias, name);
        }
    }
}
