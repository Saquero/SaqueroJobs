namespace SaqueroJobs.Domain.Entities;

using SaqueroJobs.Domain.Enums;

public class ExecutionLog
{
    public Guid Id { get; private set; }
    public Guid ExecutionId { get; private set; }
    public string Message { get; private set; } = null!;
    public LogLevel Level { get; private set; }
    public DateTime Timestamp { get; private set; }

    private ExecutionLog() { }

    public static ExecutionLog Create(Guid executionId, string message, LogLevel level)
    {
        return new ExecutionLog
        {
            Id          = Guid.NewGuid(),
            ExecutionId = executionId,
            Message     = message,
            Level       = level,
            Timestamp   = DateTime.UtcNow
        };
    }
}
