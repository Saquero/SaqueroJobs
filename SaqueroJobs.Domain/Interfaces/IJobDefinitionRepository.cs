namespace SaqueroJobs.Domain.Interfaces;

using SaqueroJobs.Domain.Entities;

public interface IJobDefinitionRepository
{
    Task<JobDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<JobDefinition>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<JobDefinition>> GetEnabledAsync(CancellationToken ct = default);
    Task AddAsync(JobDefinition job, CancellationToken ct = default);
    Task UpdateAsync(JobDefinition job, CancellationToken ct = default);
    Task<bool> ExistsByJobTypeAsync(string jobType, CancellationToken ct = default);
}
