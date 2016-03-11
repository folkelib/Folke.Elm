namespace Folke.Elm
{
    /// <summary>
    /// An SQL command, with its parameters and its SQL
    /// </summary>
    public interface IBaseCommand
    {
        /// <summary>Gets the connection</summary>
        IFolkeConnection Connection { get; }

        /// <summary>Gets the generated SQL</summary>
        string Sql { get; }

        /// <summary>Gets the list of parameters</summary>
        object[] Parameters { get; }
    }
}