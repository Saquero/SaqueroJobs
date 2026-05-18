namespace SaqueroJobs.Infrastructure.Scheduling;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SaqueroJobs.Application.Interfaces;
using SaqueroJobs.Domain.Enums;
using SaqueroJobs.Domain.Interfaces;

public class JobSchedulerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobSchedulerService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(15);

    public JobSchedulerService(IServiceScopeFactory scopeFactory, ILogger<JobSchedulerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("JobSchedulerService started. Polling every {Seconds}s.", _interval.TotalSeconds);

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
            using var scope      = _scopeFactory.CreateScope();
            var executions       = scope.ServiceProvider.GetRequiredService<IJobExecutionRepository>();
            var definitions      = scope.ServiceProvider.GetRequiredService<IJobDefinitionRepository>();
            var handlerRegistry  = scope.ServiceProvider.GetRequiredService<IJobHandlerRegistry>();

            var pending = (await executions.GetPendingAndRetryingAsync(ct)).ToList();

            if (!pending.Any()) return;

            _logger.LogInformation("Scheduler: processing {Count} pending execution(s).", pending.Count);

            foreach (var execution in pending)
            {
                var definition = await definitions.GetByIdAsync(execution.JobDefinitionId, ct);
                if (definition is null || !definition.IsEnabled) continue;

                try
                {
                    execution.MarkAsRunning();
                    await executions.UpdateAsync(execution, ct);

                    await handlerRegistry.ExecuteAsync(definition.JobType, execution, ct);

                    execution.MarkAsCompleted();
                    await executions.UpdateAsync(execution, ct);

                    _logger.LogInformation("Execution {Id} completed.", execution.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Execution {Id} failed.", execution.Id);
                    execution.MarkAsFailed(ex.Message);
                    await executions.UpdateAsync(execution, ct);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduler encountered an unexpected error.");
        }
    }
}
