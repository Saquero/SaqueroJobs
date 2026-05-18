namespace SaqueroJobs.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaqueroJobs.Domain.Entities;

public class JobExecutionConfiguration : IEntityTypeConfiguration<JobExecution>
{
    public void Configure(EntityTypeBuilder<JobExecution> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.TriggeredBy).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000);

        builder.HasMany(e => e.Logs)
               .WithOne()
               .HasForeignKey(l => l.ExecutionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
