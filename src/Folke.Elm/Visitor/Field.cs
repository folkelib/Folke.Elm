using System;
using Folke.Elm.Mapping;
using System.Collections.Generic;

namespace Folke.Elm.Visitor
{
    /// <summary>
    /// A column name with the table name.
    /// </summary>
    public class Field : IVisitable
    {
        public SelectedTable Table { get; }
        public PropertyMapping Column { get; }

        public Field(SelectedTable table, PropertyMapping column)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
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

        public override string ToString()
        {
            return $"{Column} of {Table}";
        }

        public override bool Equals(object obj)
        {
            return obj is Field o && o.Table.Equals(Table) && o.Column == Column;
        }

        public override int GetHashCode()
        {
            var hashCode = -366015330;
            hashCode = hashCode * -1521134295 + EqualityComparer<SelectedTable>.Default.GetHashCode(Table);
            hashCode = hashCode * -1521134295 + EqualityComparer<PropertyMapping>.Default.GetHashCode(Column);
            return hashCode;
        }
    }
}
