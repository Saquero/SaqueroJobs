namespace SaqueroJobs.Infrastructure.Execution.Handlers;

using SaqueroJobs.Domain.Entities;
using SaqueroJobs.Domain.Enums;

public class SendNotificationBatchHandler : IJobHandler
{
    public string JobType => "SendNotificationBatchJob";

    public async Task ExecuteAsync(JobExecution execution, CancellationToken ct = default)
    {
        execution.AddLog("Loading pending notifications...", LogLevel.Info);
        await Task.Delay(200, ct);
        execution.AddLog("Sending batch of 89 notifications...", LogLevel.Info);
        await Task.Delay(350, ct);
        execution.AddLog("89 notifications sent successfully.", LogLevel.Info);
    }
}
