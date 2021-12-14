using Rookie.AMO.DataAccessor.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rookie.AMO.Contracts.Dtos.Request
{
    public class ReturnRequestDto 
    {
        public Guid Id { get; set; }
        public State State { get; set; }
        public Guid UserRequestId { get; set; }
        public Guid UserAcceptId { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public string RequestedBy { get; set; }
        public string AcceptedBy { get; set; }
    }
}
