using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace Rookie.AMO.DataAccessor.Entities
{
    public class Asset : BaseEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public State State { get; set; }
        public string Location { get; set; }
        public string Specification { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
        public DateTime InstalledDate { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; }

    }

    public class AssetMapping
    {
        public AssetMapping(EntityTypeBuilder<Asset> entityTypeBuilder)
        {
            entityTypeBuilder.HasKey(x => x.Id);
            entityTypeBuilder.Property(x => x.Code);
            entityTypeBuilder.Property(x => x.Name);
            entityTypeBuilder.Property(x => x.State);
            entityTypeBuilder.Property(x => x.Location);
            entityTypeBuilder.Property(x => x.Specification);
            entityTypeBuilder.Property(x => x.InstalledDate);
            entityTypeBuilder.HasMany(x => x.Assignments);
        }
    }

}
