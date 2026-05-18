namespace SaqueroJobs.Domain.Entities;

using SaqueroJobs.Domain.Enums;
using SaqueroJobs.Domain.Exceptions;
using SaqueroJobs.Domain.ValueObjects;

public class JobExecution
{
    public Guid Id { get; private set; }
    public Guid JobDefinitionId { get; private set; }
    public ExecutionStatus Status { get; private set; }
    public TriggerSource TriggeredBy { get; private set; }
    public int AttemptNumber { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<ExecutionLog> _logs = new();
    public IReadOnlyCollection<ExecutionLog> Logs => _logs.AsReadOnly();

    private JobExecution() { }

    public static JobExecution Create(Guid jobDefinitionId, TriggerSource triggeredBy, int attemptNumber = 1)
    {
        return new JobExecution
        {
            Id              = Guid.NewGuid(),
            JobDefinitionId = jobDefinitionId,
            Status          = ExecutionStatus.Pending,
            TriggeredBy     = triggeredBy,
            AttemptNumber   = attemptNumber,
            CreatedAt       = DateTime.UtcNow
        };
    }

    public void MarkAsRunning()
    {
        if (Status != ExecutionStatus.Pending && Status != ExecutionStatus.Retrying)
            throw new JobDomainException($"Cannot start execution in '{Status}' status.");

        Status    = ExecutionStatus.Running;
        StartedAt = DateTime.UtcNow;
        AddLog("Execution started.", LogLevel.Info);
    }

    public void MarkAsCompleted()
    {
        if (Status != ExecutionStatus.Running)
            throw new JobDomainException($"Cannot complete execution in '{Status}' status.");

        Status      = ExecutionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        AddLog("Execution completed successfully.", LogLevel.Info);
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status       = ExecutionStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt  = DateTime.UtcNow;
        AddLog($"Execution failed: {errorMessage}", LogLevel.Error);
    }

    public void MarkAsTimedOut()
    {
        Status      = ExecutionStatus.TimedOut;
        CompletedAt = DateTime.UtcNow;
        AddLog("Execution timed out.", LogLevel.Warning);
    }

    public void MarkAsCancelled()
    {
        if (Status != ExecutionStatus.Pending && Status != ExecutionStatus.Running)
            throw new JobDomainException($"Cannot cancel execution in '{Status}' status.");

        Status      = ExecutionStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
        AddLog("Execution cancelled.", LogLevel.Warning);
    }

    public void MarkAsRetrying()
    {
        if (Status != ExecutionStatus.Failed && Status != ExecutionStatus.TimedOut)
            throw new JobDomainException("Only failed or timed out executions can be retried.");

        Status = ExecutionStatus.Retrying;
        AddLog($"Scheduled for retry (attempt {AttemptNumber}).", LogLevel.Warning);
    }

    public bool CanBeRetried(RetryPolicy policy) =>
        (Status == ExecutionStatus.Failed || Status == ExecutionStatus.TimedOut)
        && AttemptNumber < policy.MaxRetries;

    public bool CanBeCancelled() =>
        Status == ExecutionStatus.Pending || Status == ExecutionStatus.Running;

    public bool IsTerminal() =>
        Status is ExecutionStatus.Completed
               or ExecutionStatus.Cancelled
               or ExecutionStatus.TimedOut;

    public void AddLog(string message, LogLevel level)
    {
        _logs.Add(ExecutionLog.Create(Id, message, level));
    }
}
