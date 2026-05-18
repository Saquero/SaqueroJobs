namespace SaqueroJobs.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaqueroJobs.Domain.Entities;

public class JobDefinitionConfiguration : IEntityTypeConfiguration<JobDefinition>
{
    public void Configure(EntityTypeBuilder<JobDefinition> builder)
    {
        builder.HasKey(j => j.Id);
        builder.Property(j => j.Name).IsRequired().HasMaxLength(200);
        builder.Property(j => j.Description).HasMaxLength(500);
        builder.Property(j => j.JobType).IsRequired().HasMaxLength(100);
        builder.Property(j => j.CronExpression).HasMaxLength(100);
        builder.Property(j => j.IsEnabled).IsRequired();

        builder.OwnsOne(j => j.RetryPolicy, rp =>
        {
            rp.Property(p => p.MaxRetries).HasColumnName("RetryPolicy_MaxRetries");
            rp.Property(p => p.DelaySeconds).HasColumnName("RetryPolicy_DelaySeconds");
        });

        builder.HasMany<JobExecution>()
               .WithOne()
               .HasForeignKey(e => e.JobDefinitionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
