namespace SaqueroJobs.Application.DTOs;

public record RetryPolicyDto(
    int MaxRetries,
    int DelaySeconds
);
