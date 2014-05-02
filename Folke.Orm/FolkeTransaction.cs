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
        private DbTransaction transaction;
        private FolkeConnection connection;

        public FolkeTransaction(FolkeConnection connection, DbTransaction transaction)
        {
            this.connection = connection;
            this.transaction = transaction;
        }

        public void Dispose()
        {
            transaction.Dispose();
            connection.EndTransaction();
        }

        internal void Rollback()
        {
            transaction.Rollback();
            connection.EndTransaction();
        }


        public void Commit()
        {
            transaction.Commit();
            connection.EndTransaction();
        }
    }
}
