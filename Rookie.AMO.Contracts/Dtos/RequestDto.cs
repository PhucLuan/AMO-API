using Rookie.AMO.Contracts.Dtos.Assignment;
using Rookie.AMO.DataAccessor.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rookie.AMO.Contracts.Dtos
{
    public class RequestDto
    {
        public Guid Id { get; set; }
        public DateTime? ReturnDate { get; set; }
        public State State { get; set; }
        public Guid UserRequestId { get; set; }
        public Guid UserAcceptId { get; set; }
        public Guid AssignmentID { get; set; }
        public virtual AssignmentDto AssignmentDto { get; set; }
    }
}
