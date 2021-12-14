using Microsoft.EntityFrameworkCore;
using Rookie.AMO.DataAccessor.Entities;
using System;

namespace Rookie.AMO.DataAccessor.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<RequestAssignment> RequestAssignments { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            //code first
            //db first
            //model first
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>(entity =>
            {
                entity.ToTable(name: "Category");
            });

            builder.Entity<Asset>(entity =>
            {
                entity.ToTable(name: "Asset");
            });

            builder.Entity<Assignment>(entity =>
            {
                entity.ToTable(name: "Assignment");
            });

            builder.Entity<RequestAssignment>()
                .ToTable(name: "RequestAssignment")
                .HasOne(b => b.Assignment)
                .WithOne(i => i.RequestAssignment)
                .HasForeignKey<Assignment>(b => b.RequestAssignmentId)
                .IsRequired(false);
        }
    }
}
