using System;

namespace Folke.Elm
{
    public class ElmException : Exception
    {
        public ElmException(string message) : base(message)
        {
        }
    }
}
