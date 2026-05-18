namespace SaqueroJobs.Application.UseCases.TriggerJobExecution;

using SaqueroJobs.Application.DTOs;
using SaqueroJobs.Application.Interfaces;
using SaqueroJobs.Application.Mappers;
using SaqueroJobs.Domain.Entities;
using SaqueroJobs.Domain.Enums;
using SaqueroJobs.Domain.Exceptions;
using SaqueroJobs.Domain.Interfaces;

public class TriggerJobExecutionUseCase
{
    private readonly IJobDefinitionRepository  _definitions;
    private readonly IJobExecutionRepository   _executions;
    private readonly IJobHandlerRegistry       _handlers;

    public TriggerJobExecutionUseCase(
        IJobDefinitionRepository definitions,
        IJobExecutionRepository  executions,
        IJobHandlerRegistry      handlers)
    {
        _definitions = definitions;
        _executions  = executions;
        _handlers    = handlers;
    }

    public async Task<JobExecutionDto> ExecuteAsync(Guid jobDefinitionId, CancellationToken ct = default)
    {
        var definition = await _definitions.GetByIdAsync(jobDefinitionId, ct)
            ?? throw new JobDomainException($"Job definition '{jobDefinitionId}' not found.");

        if (!definition.CanBeTriggered())
            throw new JobDomainException($"Job '{definition.Name}' is disabled and cannot be triggered.");

        if (!_handlers.HasHandler(definition.JobType))
            throw new JobDomainException($"No handler registered for job type '{definition.JobType}'.");

        var execution = JobExecution.Create(jobDefinitionId, TriggerSource.Manual);
        await _executions.AddAsync(execution, ct);

        _ = Task.Run(async () =>
        {
            try
            {
                execution.MarkAsRunning();
                await _executions.UpdateAsync(execution, ct);

                await _handlers.ExecuteAsync(definition.JobType, execution, ct);

                execution.MarkAsCompleted();
                await _executions.UpdateAsync(execution, ct);
            }
            catch (Exception ex)
            {
                execution.MarkAsFailed(ex.Message);
                await _executions.UpdateAsync(execution, ct);
            }
        }, ct);

        return JobMapper.ToDto(execution, definition.Name);
    }
}
