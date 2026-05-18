namespace SaqueroJobs.Application.Interfaces;

using SaqueroJobs.Domain.Entities;

public interface IJobHandlerRegistry
{
    bool HasHandler(string jobType);
    Task ExecuteAsync(string jobType, JobExecution execution, CancellationToken ct = default);
}
