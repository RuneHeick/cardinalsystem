using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.SystemMessages
{
    public enum SystemMessageType
    {
        MERGING_REQUEST,
        MERGING_ACCEPTED,
        MERGING_DENIDE,
        MERGING_COMPLETE_OK,
        MERGING_ERROR,
        MERGING_DATA
    }
}
