namespace SaqueroJobs.Infrastructure.Execution.Handlers;

using SaqueroJobs.Domain.Entities;
using SaqueroJobs.Domain.Enums;

public class GenerateDailyReportHandler : IJobHandler
{
    public string JobType => "GenerateDailyReportJob";

    public async Task ExecuteAsync(JobExecution execution, CancellationToken ct = default)
    {
        execution.AddLog("Aggregating daily metrics...", LogLevel.Info);
        await Task.Delay(400, ct);
        execution.AddLog("Report generated: daily_report_2024.pdf", LogLevel.Info);
        await Task.Delay(100, ct);
        execution.AddLog("Report stored successfully.", LogLevel.Info);
    }
}
