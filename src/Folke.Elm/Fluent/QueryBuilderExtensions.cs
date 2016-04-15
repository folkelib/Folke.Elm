using System.Collections.Generic;
using Folke.Elm.Mapping;

namespace Folke.Elm.Fluent
{
    public static class QueryBuilderExtensions
    {
        public static BaseQueryBuilder.SelectedTable AppendSelectedColumns(this BaseQueryBuilder builder, BaseQueryBuilder.SelectedTable selectedTable, IEnumerable<PropertyMapping> columns)
        {
            bool first = true;

            foreach (var column in columns)
            {
                builder.SelectField(column, selectedTable);
                if (first)
                    first = false;
                else
                    builder.StringBuilder.Append(',');
                string tableName = selectedTable.Alias;
                builder.StringBuilder.DuringColumn(tableName, column.ColumnName);
            }
            return selectedTable;
        }

        public static BaseQueryBuilder.SelectedTable AppendAllSelects(this BaseQueryBuilder builder, BaseQueryBuilder.SelectedTable selectedTable)
        {
            return builder.AppendSelectedColumns(selectedTable, selectedTable.Mapping.Columns.Values);
        }
    }
}
