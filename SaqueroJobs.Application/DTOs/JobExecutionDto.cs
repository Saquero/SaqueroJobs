namespace SaqueroJobs.Application.DTOs;

public record JobExecutionDto(
    Guid Id,
    Guid JobDefinitionId,
    string JobName,
    string Status,
    string TriggeredBy,
    int AttemptNumber,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? ErrorMessage,
    DateTime CreatedAt,
    IEnumerable<ExecutionLogDto> Logs
);
