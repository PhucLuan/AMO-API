using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rookie.AMO.DataAccessor.Entities
{
    public enum State
    {
        Available = 0,
        [Description("Not Available")]
        NotAvailable = 1,
        [Description("Accepted")]
        Accepted = 2,
        [Description("Waiting for Accept")]
        WaitingAccept = 3,
        [Description("Waiting For Recycle")]
        WaitingForRecycle = 4,
        Recycled = 5,
        Assigned = 6,
        Completed = 7,
        WaitingForReturning = 8
    }
}
