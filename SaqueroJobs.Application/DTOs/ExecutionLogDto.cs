namespace SaqueroJobs.Application.DTOs;

public record ExecutionLogDto(
    Guid Id,
    string Message,
    string Level,
    DateTime Timestamp
);
