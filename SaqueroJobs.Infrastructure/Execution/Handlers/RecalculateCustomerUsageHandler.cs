namespace SaqueroJobs.Infrastructure.Execution.Handlers;

using SaqueroJobs.Domain.Entities;
using SaqueroJobs.Domain.Enums;

public class RecalculateCustomerUsageHandler : IJobHandler
{
    public string JobType => "RecalculateCustomerUsageJob";

    public async Task ExecuteAsync(JobExecution execution, CancellationToken ct = default)
    {
        execution.AddLog("Loading active customer list...", LogLevel.Info);
        await Task.Delay(250, ct);
        execution.AddLog("Recalculating usage for 512 customers...", LogLevel.Info);
        await Task.Delay(500, ct);
        execution.AddLog("Usage recalculation complete.", LogLevel.Info);
    }
}
