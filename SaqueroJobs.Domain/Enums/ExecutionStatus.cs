namespace SaqueroJobs.Domain.Enums;

public enum ExecutionStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Retrying,
    Cancelled,
    TimedOut
}
