namespace Folke.Elm
{
    public enum QueryContext
    {
        /// <summary>
        /// Context is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// Currently in a SELECT statement.
        /// </summary>
        Select,

        /// <summary>
        /// In a WHERE part.
        /// </summary>
        Where,

        /// <summary>
        /// In the ORDER BY part.
        /// </summary>
        OrderBy,

        /// <summary>
        /// In a SET statment.
        /// </summary>
        Set,

        /// <summary>
        /// In any JOIN part.
        /// </summary>
        Join,

        /// <summary>
        /// In the VALUES() part of an INSERT statment.
        /// </summary>
        Values,

        /// <summary>
        /// In the FROM part of a SELECT statment.
        /// </summary>
        From,

        /// <summary>
        /// In a DELETE statement.
        /// </summary>
        Delete,

        /// <summary>
        /// In a GROUP BY part.
        /// </summary>
        GroupBy,

        /// <summary>
        /// In the middle of a WhereExpression in parenthesis
        /// </summary>
        WhereExpression
    }
}