using System.ComponentModel.DataAnnotations.Schema;

namespace Folke.Elm.InformationSchema
{
    public interface IColumnDefinition
    {
        string ColumnName { get; }
        string ColumnType { get; }
    }
}
