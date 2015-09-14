using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Orm
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
