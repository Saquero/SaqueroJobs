namespace SaqueroJobs.Infrastructure.Execution.Handlers;

using SaqueroJobs.Domain.Entities;
using SaqueroJobs.Domain.Enums;

public class CleanExpiredSessionsHandler : IJobHandler
{
    public string JobType => "CleanExpiredSessionsJob";

    public async Task ExecuteAsync(JobExecution execution, CancellationToken ct = default)
    {
        execution.AddLog("Scanning for expired sessions...", LogLevel.Info);
        await Task.Delay(200, ct);
        execution.AddLog("Found 38 expired sessions.", LogLevel.Warning);
        await Task.Delay(150, ct);
        execution.AddLog("Deleted 38 sessions. Database cleaned.", LogLevel.Info);
    }
}
