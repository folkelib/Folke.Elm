using System;
using System.Linq;

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

        public static T IsNull<T>(T? column, T defaultValue) where T: struct
        {
            return column ?? defaultValue;
        }

        public static Tuple<bool, TRet> When<TRet>(bool condition, TRet ret)
        {
            return new Tuple<bool, TRet>(condition, ret);
        }

        public static Tuple<bool, TRet> Else<TRet>(TRet ret)
        {
            return new Tuple<bool, TRet>(true, ret);
        }

        public static TRet Case<TRet>(params Tuple<bool, TRet>[] cas)
        {
            return cas.Where(x => x.Item1).Select(x => x.Item2).FirstOrDefault();
        }
    }
}
