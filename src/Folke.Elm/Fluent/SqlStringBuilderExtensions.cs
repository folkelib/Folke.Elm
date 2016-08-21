namespace Folke.Elm.Fluent
{
    /// <summary>
    /// Some helpers used to create SQL directly 
    /// </summary>
    public static class SqlStringBuilderExtensions
    {
        public static void AppendTable(this SqlStringBuilder query, SelectedTable table)
        {
            query.DuringTable(table.Mapping.TableSchema, table.Mapping.TableName);
            query.DuringAliasDefinition(table.Alias);
        }

    }
}
