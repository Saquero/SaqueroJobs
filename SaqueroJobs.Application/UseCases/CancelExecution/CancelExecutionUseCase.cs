namespace SaqueroJobs.Application.UseCases.CancelExecution;

using SaqueroJobs.Application.DTOs;
using SaqueroJobs.Application.Mappers;
using SaqueroJobs.Domain.Exceptions;
using SaqueroJobs.Domain.Interfaces;

public class CancelExecutionUseCase
{
    private readonly IJobExecutionRepository  _executions;
    private readonly IJobDefinitionRepository _definitions;

    public CancelExecutionUseCase(IJobExecutionRepository executions, IJobDefinitionRepository definitions)
    {
        _executions  = executions;
        _definitions = definitions;
    }

    public async Task<JobExecutionDto?> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var execution = await _executions.GetByIdAsync(id, ct)
            ?? throw new JobDomainException($"Execution '{id}' not found.");

        if (!execution.CanBeCancelled())
            throw new JobDomainException($"Execution in '{execution.Status}' status cannot be cancelled.");

        execution.MarkAsCancelled();
        await _executions.UpdateAsync(execution, ct);

        var definition = await _definitions.GetByIdAsync(execution.JobDefinitionId, ct);
        return JobMapper.ToDto(execution, definition?.Name ?? "Unknown");
    }
}
