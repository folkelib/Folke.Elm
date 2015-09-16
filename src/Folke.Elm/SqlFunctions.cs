namespace Folke.Elm
{
    public static class SqlFunctions
    {
        /// <summary>
        /// Returns the last inserted id
        /// </summary>
        /// <returns>The last inserted id</returns>
        public static int LastInsertedId()
        {
            return 0;
        }

        public static T Max<T>(T column)
        {
            return column;
        }

        public static T Sum<T>(T column)
        {
            return column;
        }
    }
}
