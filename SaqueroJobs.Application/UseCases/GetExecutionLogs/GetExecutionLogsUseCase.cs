namespace SaqueroJobs.Application.UseCases.GetExecutionLogs;

using SaqueroJobs.Application.DTOs;
using SaqueroJobs.Application.Mappers;
using SaqueroJobs.Domain.Exceptions;
using SaqueroJobs.Domain.Interfaces;

public class GetExecutionLogsUseCase
{
    private readonly IJobExecutionRepository _executions;

    public GetExecutionLogsUseCase(IJobExecutionRepository executions)
        => _executions = executions;

    public async Task<IEnumerable<ExecutionLogDto>> ExecuteAsync(Guid executionId, CancellationToken ct = default)
    {
        var execution = await _executions.GetByIdAsync(executionId, ct)
            ?? throw new JobDomainException($"Execution '{executionId}' not found.");

        return execution.Logs.Select(l => new ExecutionLogDto(l.Id, l.Message, l.Level.ToString(), l.Timestamp));
    }
}
