namespace SaqueroJobs.Infrastructure.Execution;

using SaqueroJobs.Application.Interfaces;
using SaqueroJobs.Domain.Entities;

public class JobHandlerRegistry : IJobHandlerRegistry
{
    private readonly Dictionary<string, IJobHandler> _handlers;

    public JobHandlerRegistry(IEnumerable<IJobHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.JobType, h => h);
    }

    public bool HasHandler(string jobType) => _handlers.ContainsKey(jobType);

    public async Task ExecuteAsync(string jobType, JobExecution execution, CancellationToken ct = default)
    {
        if (!_handlers.TryGetValue(jobType, out var handler))
            throw new InvalidOperationException($"No handler registered for job type '{jobType}'.");

        await handler.ExecuteAsync(execution, ct);
    }
}
