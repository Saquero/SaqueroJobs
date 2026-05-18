namespace SaqueroJobs.Domain.ValueObjects;

using SaqueroJobs.Domain.Exceptions;

public record RetryPolicy
{
    public int MaxRetries { get; init; }
    public int DelaySeconds { get; init; }

    public static RetryPolicy Default => new(3, 30);
    public static RetryPolicy None => new(0, 0);

    public RetryPolicy(int maxRetries, int delaySeconds)
    {
        if (maxRetries < 0)
            throw new JobDomainException("MaxRetries cannot be negative.");
        if (delaySeconds < 0)
            throw new JobDomainException("DelaySeconds cannot be negative.");

        MaxRetries   = maxRetries;
        DelaySeconds = delaySeconds;
    }
}
