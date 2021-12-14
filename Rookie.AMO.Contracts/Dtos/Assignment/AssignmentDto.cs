using Rookie.AMO.DataAccessor.Entities;
using System;

namespace Rookie.AMO.Contracts.Dtos.Assignment
{
    public class AssignmentDto
    {
        public Guid Id { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public string AssignedTo { get; set; }
        public string AssignedBy { get; set; }
        public DateTime AssignedDate { get; set; }
        public string AssetSpecification { get; set; }
        public State State { get; set; }
        public string Note { get; set; }
        public Guid AssetID { get; set; }
        public Guid UserID { get; set; }
        public Guid? CreatorId { get; set; }
        public string CategoryName { get; set; }
        public string StateName { get; set; }
        public Guid? RequestAssignmentId { get; set; }
    }
}
