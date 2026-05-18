namespace SaqueroJobs.Domain.Entities;

using SaqueroJobs.Domain.Enums;
using SaqueroJobs.Domain.Exceptions;
using SaqueroJobs.Domain.ValueObjects;

public class JobDefinition
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string JobType { get; private set; } = null!;
    public string? CronExpression { get; private set; }
    public bool IsEnabled { get; private set; }
    public RetryPolicy RetryPolicy { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private JobDefinition() { }

    public static JobDefinition Create(
        string name,
        string description,
        string jobType,
        string? cronExpression = null,
        RetryPolicy? retryPolicy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new JobDomainException("Job name cannot be empty.");
        if (string.IsNullOrWhiteSpace(jobType))
            throw new JobDomainException("Job type cannot be empty.");

        return new JobDefinition
        {
            Id             = Guid.NewGuid(),
            Name           = name,
            Description    = description ?? string.Empty,
            JobType        = jobType,
            CronExpression = cronExpression,
            IsEnabled      = true,
            RetryPolicy    = retryPolicy ?? RetryPolicy.Default,
            CreatedAt      = DateTime.UtcNow,
            UpdatedAt      = DateTime.UtcNow
        };
    }

    public void Enable()
    {
        IsEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        IsEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRetryPolicy(RetryPolicy policy)
    {
        RetryPolicy = policy;
        UpdatedAt   = DateTime.UtcNow;
    }

    public bool CanBeTriggered() => IsEnabled;
}
