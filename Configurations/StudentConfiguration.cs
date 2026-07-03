using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RegistrationNumber).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        
        // Session 3: Shadow property for audit tracking (doesn't exist in C# class)
        builder.Property<DateTime>("LastUpdated");

        // Session 3: Concurrency token (maps to PostgreSQL hidden 'xmin' column)
        builder.Property(s => s.Version).IsRowVersion();

        // Session 3: Global soft-delete filter (hides deleted students from normal queries)
        builder.HasQueryFilter(s => !s.IsDeleted);

        builder.HasMany(s => s.Enrollments)
               .WithOne(e => e.Student)
               .HasForeignKey(e => e.StudentId)
               .OnDelete(DeleteBehavior.Cascade); 
    }
}