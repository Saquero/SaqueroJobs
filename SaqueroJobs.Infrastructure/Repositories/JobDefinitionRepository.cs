namespace SaqueroJobs.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using SaqueroJobs.Domain.Entities;
using SaqueroJobs.Domain.Interfaces;
using SaqueroJobs.Infrastructure.Persistence;

public class JobDefinitionRepository : IJobDefinitionRepository
{
    private readonly JobsDbContext _context;

    public JobDefinitionRepository(JobsDbContext context) => _context = context;

    public async Task<JobDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.JobDefinitions.FirstOrDefaultAsync(j => j.Id == id, ct);

    public async Task<IEnumerable<JobDefinition>> GetAllAsync(CancellationToken ct = default)
        => await _context.JobDefinitions.OrderBy(j => j.Name).ToListAsync(ct);

    public async Task<IEnumerable<JobDefinition>> GetEnabledAsync(CancellationToken ct = default)
        => await _context.JobDefinitions.Where(j => j.IsEnabled).ToListAsync(ct);

    public async Task AddAsync(JobDefinition job, CancellationToken ct = default)
    {
        await _context.JobDefinitions.AddAsync(job, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(JobDefinition job, CancellationToken ct = default)
    {
        _context.JobDefinitions.Update(job);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsByJobTypeAsync(string jobType, CancellationToken ct = default)
        => await _context.JobDefinitions.AnyAsync(j => j.JobType == jobType, ct);
}
