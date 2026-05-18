namespace SaqueroJobs.Infrastructure.Execution;

using SaqueroJobs.Domain.Entities;
using SaqueroJobs.Domain.Enums;

public interface IJobHandler
{
    string JobType { get; }
    Task ExecuteAsync(JobExecution execution, CancellationToken ct = default);
}
