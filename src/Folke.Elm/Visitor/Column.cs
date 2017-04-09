using Folke.Elm.Mapping;

namespace Folke.Elm.Visitor
{
    /// <summary>
    /// A column name with the table name.
    /// </summary>
    public class Column : IVisitable
    {
        public SelectedTable Table { get; set; }
        public PropertyMapping Property { get; set; }

        public Column(SelectedTable table, PropertyMapping property)
        {
            Table = table;
            Property = property;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.DuringColumn(this.Table.Alias, this.Property.ColumnName);
        }
    }
}
