namespace SaqueroJobs.Infrastructure.Execution.Handlers;

using SaqueroJobs.Domain.Entities;
using SaqueroJobs.Domain.Enums;

public class SyncExternalOrdersHandler : IJobHandler
{
    public string JobType => "SyncExternalOrdersJob";

    public async Task ExecuteAsync(JobExecution execution, CancellationToken ct = default)
    {
        execution.AddLog("Connecting to external orders API...", LogLevel.Info);
        await Task.Delay(300, ct);
        execution.AddLog("Fetched 142 orders from remote source.", LogLevel.Info);
        await Task.Delay(200, ct);
        execution.AddLog("Sync completed. 142 orders processed.", LogLevel.Info);
    }
}
