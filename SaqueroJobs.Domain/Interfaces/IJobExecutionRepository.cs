namespace SaqueroJobs.Domain.Interfaces;

using SaqueroJobs.Domain.Entities;
using SaqueroJobs.Domain.Enums;

public interface IJobExecutionRepository
{
    Task<JobExecution?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<JobExecution>> GetByJobDefinitionIdAsync(Guid jobDefinitionId, CancellationToken ct = default);
    Task<IEnumerable<JobExecution>> GetByStatusAsync(ExecutionStatus status, CancellationToken ct = default);
    Task<IEnumerable<JobExecution>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<JobExecution>> GetPendingAndRetryingAsync(CancellationToken ct = default);
    Task AddAsync(JobExecution execution, CancellationToken ct = default);
    Task UpdateAsync(JobExecution execution, CancellationToken ct = default);
    Task<int> CountByStatusAsync(ExecutionStatus status, CancellationToken ct = default);
}
