namespace SaqueroJobs.Application.DTOs;

public record JobDefinitionDto(
    Guid Id,
    string Name,
    string Description,
    string JobType,
    string? CronExpression,
    bool IsEnabled,
    RetryPolicyDto RetryPolicy,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
