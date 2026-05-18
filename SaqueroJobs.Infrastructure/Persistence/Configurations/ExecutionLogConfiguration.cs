namespace SaqueroJobs.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaqueroJobs.Domain.Entities;

public class ExecutionLogConfiguration : IEntityTypeConfiguration<ExecutionLog>
{
    public void Configure(EntityTypeBuilder<ExecutionLog> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Message).IsRequired().HasMaxLength(2000);
        builder.Property(l => l.Level).HasConversion<string>().HasMaxLength(10);
    }
}
