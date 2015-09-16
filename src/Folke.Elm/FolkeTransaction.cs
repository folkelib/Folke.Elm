using System;

namespace Folke.Elm
{
    public class FolkeTransaction : IDisposable
    {
        private FolkeConnection connection;
        private bool commited = false;

        public FolkeTransaction(FolkeConnection connection)
        {
            this.connection = connection;
       }

        public void Dispose()
        {
            if (!commited)
                connection.RollbackTransaction();
        }

        internal void Rollback()
        {
            if (!commited)
            {
                commited = true;
                connection.RollbackTransaction();
            }
        }


        public void Commit()
        {
            if (!commited)
            {
                commited = true;
                connection.CommitTransaction();
            }
        }
    }
}
