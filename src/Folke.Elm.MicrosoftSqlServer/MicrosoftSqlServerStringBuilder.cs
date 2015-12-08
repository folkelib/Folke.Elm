using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Folke.Elm.MicrosoftSqlServer
{
    public class MicrosoftSqlServerStringBuilder : SqlStringBuilder
    {
        public override void AppendAutoIncrement()
        {
            stringBuilder.Append(" IDENTITY(1,1)");
        }

        public override void AppendLastInsertedId()
        {
            stringBuilder.Append(" @@IDENTITY");
        }

        public override void BeforeLimit()
        {
            AppendAfterSpace("OFFSET ");
        }

        public override void DuringLimit()
        {
            AppendAfterSpace("ROWS FETCH NEXT ");
        }

        public override void AfterLimit()
        {
            AppendAfterSpace("ROWS ONLY");
        }

        public override void BeforeAddColumn()
        {
            AppendAfterSpace("ADD ");
        }

        public override void BeforeAlterColumn(string previousColumnName)
        {
            AppendAfterSpace("ALTER COLUMN ");
        }
    }
}
