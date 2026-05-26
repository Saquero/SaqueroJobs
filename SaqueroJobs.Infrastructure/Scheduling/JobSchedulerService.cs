namespace SaqueroJobs.Infrastructure.Scheduling;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SaqueroJobs.Application.Interfaces;
using SaqueroJobs.Domain.Enums;
using SaqueroJobs.Domain.Interfaces;
using SaqueroJobs.Infrastructure.Persistence;

public class JobSchedulerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobSchedulerService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(15);

    public JobSchedulerService(
        IServiceScopeFactory scopeFactory,
        ILogger<JobSchedulerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "JobSchedulerService started. Polling every {Seconds}s.",
            _interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessPendingExecutionsAsync(stoppingToken);
            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("JobSchedulerService stopped.");
    }

    private async Task ProcessPendingExecutionsAsync(CancellationToken ct)
    {
        try
        {
            // Scope dedicado solo para leer las ejecuciones pendientes
            List<Guid> pendingIds;
            using (var readScope = _scopeFactory.CreateScope())
            {
                var factory = readScope.ServiceProvider
                    .GetRequiredService<IDbContextFactory<JobsDbContext>>();

                await using var readDb = await factory.CreateDbContextAsync(ct);

                pendingIds = await readDb.JobExecutions
                    .Where(e => e.Status == ExecutionStatus.Pending
                             || e.Status == ExecutionStatus.Retrying)
                    .Select(e => e.Id)
                    .ToListAsync(ct);
            }

            if (!pendingIds.Any()) return;

            _logger.LogInformation(
                "Scheduler: processing {Count} pending execution(s).",
                pendingIds.Count);

            // Scope AISLADO por cada ejecucion — DbContext limpio, sin tracking residual
            foreach (var executionId in pendingIds)
            {
                await ProcessSingleExecutionAsync(executionId, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduler encountered an unexpected error.");
        }
    }

    private async Task ProcessSingleExecutionAsync(Guid executionId, CancellationToken ct)
    {
        // Scope nuevo = DbContext nuevo = tracking limpio para esta ejecucion
        using var scope = _scopeFactory.CreateScope();

        var factory = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<JobsDbContext>>();

        await using var db = await factory.CreateDbContextAsync(ct);

        var executions      = scope.ServiceProvider.GetRequiredService<IJobExecutionRepository>();
        var definitions     = scope.ServiceProvider.GetRequiredService<IJobDefinitionRepository>();
        var handlerRegistry = scope.ServiceProvider.GetRequiredService<IJobHandlerRegistry>();

        var execution = await executions.GetByIdAsync(executionId, ct);
        if (execution is null) return;

        var definition = await definitions.GetByIdAsync(execution.JobDefinitionId, ct);
        if (definition is null || !definition.IsEnabled) return;

        try
        {
            execution.MarkAsRunning();
            await executions.UpdateAsync(execution, ct);

            _logger.LogInformation(
                "Execution {ExecutionId} started. JobType: {JobType}, Attempt: {Attempt}",
                execution.Id,
                definition.JobType,
                execution.AttemptNumber);

            await handlerRegistry.ExecuteAsync(definition.JobType, execution, ct);

            execution.MarkAsCompleted();
            await executions.UpdateAsync(execution, ct);

            _logger.LogInformation(
                "Execution {ExecutionId} completed successfully.",
                execution.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Execution {ExecutionId} failed. JobType: {JobType}",
                execution.Id,
                definition.JobType);

            execution.MarkAsFailed(ex.Message);
            await executions.UpdateAsync(execution, ct);
        }
    }
}
