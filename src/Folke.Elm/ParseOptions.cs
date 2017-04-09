using System;

namespace Folke.Elm
{
    [Flags]
    public enum ParseOptions
    {
        /// <summary>If there is path that is not known, assume it is a table. Otherwise, returns null.</summary>
        RegisterTables = 1,

        /// <summary>If a path returns a table, returns the key of this table. Otherwise, returns the table itself</summary>
        Value = 2
    }
}
