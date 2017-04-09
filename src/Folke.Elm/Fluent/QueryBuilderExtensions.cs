using System.Collections.Generic;
using Folke.Elm.Mapping;
using Folke.Elm.Visitor;

namespace Folke.Elm.Fluent
{
    public static class QueryBuilderExtensions
    {
        public static SelectedTable AppendSelectedColumns(this BaseQueryBuilder builder, SelectedTable selectedTable, IEnumerable<PropertyMapping> columns)
        {
            foreach (var column in columns)
            {
                if (column.Reference != null && column.Reference.IsComplexType)
                {
                    var subTable = builder.RegisterTable(selectedTable, column);
                    builder.AppendAllSelects(subTable);
                }
                else
                {
                    var visitable = new Field(selectedTable, column);
                    var selectedField = builder.SelectField(visitable);
                    selectedField.Accept(builder.StringBuilder);
                }
            }
            return selectedTable;
        }

        public static SelectedTable AppendAllSelects(this BaseQueryBuilder builder, SelectedTable selectedTable)
        {
            return builder.AppendSelectedColumns(selectedTable, selectedTable.Mapping.Columns.Values);
        }
    }
}
