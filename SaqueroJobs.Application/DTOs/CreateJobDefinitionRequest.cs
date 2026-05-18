namespace SaqueroJobs.Application.DTOs;

public record CreateJobDefinitionRequest(
    string Name,
    string Description,
    string JobType,
    string? CronExpression = null,
    int MaxRetries = 3,
    int DelaySeconds = 30
);
