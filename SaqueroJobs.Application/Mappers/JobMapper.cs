namespace SaqueroJobs.Application.Mappers;

using SaqueroJobs.Application.DTOs;
using SaqueroJobs.Domain.Entities;

public static class JobMapper
{
    public static JobDefinitionDto ToDto(JobDefinition def) => new(
        def.Id,
        def.Name,
        def.Description,
        def.JobType,
        def.CronExpression,
        def.IsEnabled,
        new RetryPolicyDto(def.RetryPolicy.MaxRetries, def.RetryPolicy.DelaySeconds),
        def.CreatedAt,
        def.UpdatedAt
    );

    public static JobExecutionDto ToDto(JobExecution ex, string jobName) => new(
        ex.Id,
        ex.JobDefinitionId,
        jobName,
        ex.Status.ToString(),
        ex.TriggeredBy.ToString(),
        ex.AttemptNumber,
        ex.StartedAt,
        ex.CompletedAt,
        ex.ErrorMessage,
        ex.CreatedAt,
        ex.Logs.Select(l => new ExecutionLogDto(l.Id, l.Message, l.Level.ToString(), l.Timestamp))
    );
}
