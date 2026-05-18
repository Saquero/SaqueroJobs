namespace SaqueroJobs.Application.UseCases.RetryExecution;

using SaqueroJobs.Application.DTOs;
using SaqueroJobs.Application.Interfaces;
using SaqueroJobs.Application.Mappers;
using SaqueroJobs.Domain.Entities;
using SaqueroJobs.Domain.Enums;
using SaqueroJobs.Domain.Exceptions;
using SaqueroJobs.Domain.Interfaces;

public class RetryExecutionUseCase
{
    private readonly IJobExecutionRepository  _executions;
    private readonly IJobDefinitionRepository _definitions;
    private readonly IJobHandlerRegistry      _handlers;

    public RetryExecutionUseCase(
        IJobExecutionRepository  executions,
        IJobDefinitionRepository definitions,
        IJobHandlerRegistry      handlers)
    {
        _executions  = executions;
        _definitions = definitions;
        _handlers    = handlers;
    }

    public async Task<JobExecutionDto> ExecuteAsync(Guid executionId, CancellationToken ct = default)
    {
        var original = await _executions.GetByIdAsync(executionId, ct)
            ?? throw new JobDomainException($"Execution '{executionId}' not found.");

        var definition = await _definitions.GetByIdAsync(original.JobDefinitionId, ct)
            ?? throw new JobDomainException("Job definition not found.");

        if (!original.CanBeRetried(definition.RetryPolicy))
            throw new JobDomainException($"Execution cannot be retried. Status: {original.Status}, Attempts: {original.AttemptNumber}/{definition.RetryPolicy.MaxRetries}.");

        var retry = JobExecution.Create(original.JobDefinitionId, TriggerSource.Manual, original.AttemptNumber + 1);
        await _executions.AddAsync(retry, ct);

        _ = Task.Run(async () =>
        {
            try
            {
                retry.MarkAsRunning();
                await _executions.UpdateAsync(retry, ct);

                await _handlers.ExecuteAsync(definition.JobType, retry, ct);

                retry.MarkAsCompleted();
                await _executions.UpdateAsync(retry, ct);
            }
            catch (Exception ex)
            {
                retry.MarkAsFailed(ex.Message);
                await _executions.UpdateAsync(retry, ct);
            }
        }, ct);

        return JobMapper.ToDto(retry, definition.Name);
    }
}
