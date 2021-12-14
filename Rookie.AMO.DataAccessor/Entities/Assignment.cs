using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.ComponentModel.DataAnnotations;

namespace Rookie.AMO.DataAccessor.Entities
{
    public class Assignment : BaseEntity
    {
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime AssignedDate { get; set; }
        public State State { get; set; }
        public string Note { get; set; }
        public Guid AssetID { get; set; }
        public Guid UserID { get; set; }
        public virtual Asset Asset { get; set; }
        public Guid? RequestAssignmentId { get; set; }
        public virtual RequestAssignment RequestAssignment { get; set; }
    }

    public class AssignmentMapping
    {
        public AssignmentMapping(EntityTypeBuilder<Assignment> entityTypeBuilder)
        {
            entityTypeBuilder.HasKey(x => x.Id);
            entityTypeBuilder.Property(x => x.AssignedDate).IsRequired();
            entityTypeBuilder.Property(x => x.Note).HasMaxLength(1000);
            entityTypeBuilder.Property(x => x.State).IsRequired();
            entityTypeBuilder.Property(x => x.AssetID).IsRequired();
            entityTypeBuilder.HasOne(x => x.Asset).WithMany(x => x.Assignments).HasForeignKey(x => x.AssetID);
            entityTypeBuilder.Property(x => x.UserID).IsRequired();
            entityTypeBuilder.Property(x => x.CreatorId).IsRequired();
        }
    }
}
