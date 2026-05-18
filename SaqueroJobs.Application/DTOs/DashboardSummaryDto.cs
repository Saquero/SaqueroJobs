namespace SaqueroJobs.Application.DTOs;

public record DashboardSummaryDto(
    int TotalDefinitions,
    int EnabledDefinitions,
    int TotalExecutions,
    int PendingExecutions,
    int RunningExecutions,
    int CompletedExecutions,
    int FailedExecutions,
    int CancelledExecutions,
    int TimedOutExecutions,
    int RetryingExecutions
);
