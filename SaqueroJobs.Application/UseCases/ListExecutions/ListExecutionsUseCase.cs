namespace SaqueroJobs.Application.UseCases.ListExecutions;

using SaqueroJobs.Application.DTOs;
using SaqueroJobs.Application.Mappers;
using SaqueroJobs.Domain.Enums;
using SaqueroJobs.Domain.Interfaces;

public class ListExecutionsUseCase
{
    private readonly IJobExecutionRepository   _executions;
    private readonly IJobDefinitionRepository  _definitions;

    public ListExecutionsUseCase(IJobExecutionRepository executions, IJobDefinitionRepository definitions)
    {
        _executions  = executions;
        _definitions = definitions;
    }

    public async Task<IEnumerable<JobExecutionDto>> ExecuteAsync(string? status = null, Guid? jobDefinitionId = null, CancellationToken ct = default)
    {
        IEnumerable<Domain.Entities.JobExecution> executions;

        if (jobDefinitionId.HasValue)
            executions = await _executions.GetByJobDefinitionIdAsync(jobDefinitionId.Value, ct);
        else if (status is not null && Enum.TryParse<ExecutionStatus>(status, ignoreCase: true, out var parsed))
            executions = await _executions.GetByStatusAsync(parsed, ct);
        else
            executions = await _executions.GetAllAsync(ct);

        var definitions = (await _definitions.GetAllAsync(ct)).ToDictionary(d => d.Id, d => d.Name);

        return executions.Select(e => JobMapper.ToDto(e, definitions.GetValueOrDefault(e.JobDefinitionId, "Unknown")));
    }
}
