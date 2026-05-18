namespace SaqueroJobs.Application.UseCases.GetExecution;

using SaqueroJobs.Application.DTOs;
using SaqueroJobs.Application.Mappers;
using SaqueroJobs.Domain.Interfaces;

public class GetExecutionUseCase
{
    private readonly IJobExecutionRepository  _executions;
    private readonly IJobDefinitionRepository _definitions;

    public GetExecutionUseCase(IJobExecutionRepository executions, IJobDefinitionRepository definitions)
    {
        _executions  = executions;
        _definitions = definitions;
    }

    public async Task<JobExecutionDto?> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var execution = await _executions.GetByIdAsync(id, ct);
        if (execution is null) return null;

        var definition = await _definitions.GetByIdAsync(execution.JobDefinitionId, ct);
        return JobMapper.ToDto(execution, definition?.Name ?? "Unknown");
    }
}
