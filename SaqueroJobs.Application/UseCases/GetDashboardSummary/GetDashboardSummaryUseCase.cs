namespace SaqueroJobs.Application.UseCases.GetDashboardSummary;

using SaqueroJobs.Application.DTOs;
using SaqueroJobs.Domain.Enums;
using SaqueroJobs.Domain.Interfaces;

public class GetDashboardSummaryUseCase
{
    private readonly IJobDefinitionRepository _definitions;
    private readonly IJobExecutionRepository  _executions;

    public GetDashboardSummaryUseCase(IJobDefinitionRepository definitions, IJobExecutionRepository executions)
    {
        _definitions = definitions;
        _executions  = executions;
    }

    public async Task<DashboardSummaryDto> ExecuteAsync(CancellationToken ct = default)
    {
        var allDefs  = (await _definitions.GetAllAsync(ct)).ToList();
        var allExecs = (await _executions.GetAllAsync(ct)).ToList();

        return new DashboardSummaryDto(
            TotalDefinitions:    allDefs.Count,
            EnabledDefinitions:  allDefs.Count(d => d.IsEnabled),
            TotalExecutions:     allExecs.Count,
            PendingExecutions:   allExecs.Count(e => e.Status == ExecutionStatus.Pending),
            RunningExecutions:   allExecs.Count(e => e.Status == ExecutionStatus.Running),
            CompletedExecutions: allExecs.Count(e => e.Status == ExecutionStatus.Completed),
            FailedExecutions:    allExecs.Count(e => e.Status == ExecutionStatus.Failed),
            CancelledExecutions: allExecs.Count(e => e.Status == ExecutionStatus.Cancelled),
            TimedOutExecutions:  allExecs.Count(e => e.Status == ExecutionStatus.TimedOut),
            RetryingExecutions:  allExecs.Count(e => e.Status == ExecutionStatus.Retrying)
        );
    }
}
