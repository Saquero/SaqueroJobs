namespace SaqueroJobs.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using SaqueroJobs.Domain.Entities;

public class JobsDbContext : DbContext
{
    public JobsDbContext(DbContextOptions<JobsDbContext> options) : base(options) { }

    public DbSet<JobDefinition> JobDefinitions => Set<JobDefinition>();
    public DbSet<JobExecution>  JobExecutions  => Set<JobExecution>();
    public DbSet<ExecutionLog>  ExecutionLogs  => Set<ExecutionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobsDbContext).Assembly);
    }
}
