namespace SaqueroJobs.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using SaqueroJobs.Domain.Entities;
using SaqueroJobs.Domain.Enums;
using SaqueroJobs.Domain.Interfaces;
using SaqueroJobs.Infrastructure.Persistence;

public class JobExecutionRepository : IJobExecutionRepository
{
    private readonly JobsDbContext _context;
    public JobExecutionRepository(JobsDbContext context) => _context = context;

    public async Task<JobExecution?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.JobExecutions
            .AsNoTracking()
            .Include(e => e.Logs)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IEnumerable<JobExecution>> GetByJobDefinitionIdAsync(Guid jobDefinitionId, CancellationToken ct = default)
        => await _context.JobExecutions
            .AsNoTracking()
            .Include(e => e.Logs)
            .Where(e => e.JobDefinitionId == jobDefinitionId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);

    public async Task<IEnumerable<JobExecution>> GetByStatusAsync(ExecutionStatus status, CancellationToken ct = default)
        => await _context.JobExecutions
            .AsNoTracking()
            .Include(e => e.Logs)
            .Where(e => e.Status == status)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);

    public async Task<IEnumerable<JobExecution>> GetAllAsync(CancellationToken ct = default)
        => await _context.JobExecutions
            .AsNoTracking()
            .Include(e => e.Logs)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);

    public async Task<IEnumerable<JobExecution>> GetPendingAndRetryingAsync(CancellationToken ct = default)
        => await _context.JobExecutions
            .AsNoTracking()
            .Include(e => e.Logs)
            .Where(e => e.Status == ExecutionStatus.Pending
                     || e.Status == ExecutionStatus.Retrying)
            .ToListAsync(ct);

    public async Task AddAsync(JobExecution execution, CancellationToken ct = default)
    {
        await _context.JobExecutions.AddAsync(execution, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(JobExecution execution, CancellationToken ct = default)
    {
        // Obtener IDs de logs ya persistidos en DB
        var persistedLogIds = await _context.ExecutionLogs
            .AsNoTracking()
            .Where(l => l.ExecutionId == execution.Id)
            .Select(l => l.Id)
            .ToListAsync(ct);

        // Solo insertar logs nuevos que no existen en DB
        var newLogs = execution.Logs
            .Where(l => !persistedLogIds.Contains(l.Id))
            .ToList();

        if (newLogs.Any())
            await _context.ExecutionLogs.AddRangeAsync(newLogs, ct);

        // Actualizar solo campos escalares — sin tocar la coleccion de logs
        await _context.JobExecutions
            .Where(e => e.Id == execution.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.Status,        execution.Status)
                .SetProperty(e => e.StartedAt,     execution.StartedAt)
                .SetProperty(e => e.CompletedAt,   execution.CompletedAt)
                .SetProperty(e => e.ErrorMessage,  execution.ErrorMessage),
            ct);

        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> CountByStatusAsync(ExecutionStatus status, CancellationToken ct = default)
        => await _context.JobExecutions
            .AsNoTracking()
            .CountAsync(e => e.Status == status, ct);
}
