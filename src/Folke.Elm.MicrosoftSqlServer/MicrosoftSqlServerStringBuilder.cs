using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Folke.Elm.MicrosoftSqlServer
{
    public class MicrosoftSqlServerStringBuilder : SqlStringBuilder
    {
        public override void DuringPrimaryKey(bool autoIncrement)
        {
            stringBuilder.Append(" PRIMARY KEY");
            if (autoIncrement)
                stringBuilder.Append(" IDENTITY(1,1)");
        }

        public override void DuringLastInsertedId()
        {
            stringBuilder.Append(" @@IDENTITY");
        }

        public override void DuringSkip()
        {
            AppendAfterSpace("OFFSET ");
        }

        public override void DuringTake()
        {
            AppendAfterSpace("ROWS FETCH NEXT ");
        }

        public override void AfterTake()
        {
            AppendAfterSpace("ROWS ONLY");
        }

        public override void BeforeAddColumn()
        {
            AppendAfterSpace("ADD ");
        }

        public override void BeforeAlterColumnType(string previousColumnName)
        {
            AppendAfterSpace("ALTER COLUMN ");
            DuringSymbol(previousColumnName);
        }
    }
}
