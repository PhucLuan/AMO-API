using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.ComponentModel.DataAnnotations;

namespace Rookie.AMO.DataAccessor.Entities
{
    public class RequestAssignment
    {
        public Guid Id { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? ReturnDate { get; set; }
        public State State { get; set; }
        public Guid UserRequestId { get; set; }
        public Guid UserAcceptId { get; set; }
        public virtual Assignment Assignment { get; set; }
    }

    public class RequestAssignmentMapping
    {
        public RequestAssignmentMapping(EntityTypeBuilder<RequestAssignment> entityTypeBuilder)
        {
            entityTypeBuilder.HasKey(x => x.Id);
            entityTypeBuilder.Property(x => x.ReturnDate);
            entityTypeBuilder.Property(x => x.State).IsRequired();
            entityTypeBuilder.Property(x => x.UserRequestId).IsRequired();
            entityTypeBuilder.Property(x => x.UserAcceptId).IsRequired();
        }
    }
}
