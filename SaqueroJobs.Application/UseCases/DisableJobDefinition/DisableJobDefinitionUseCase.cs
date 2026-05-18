namespace SaqueroJobs.Application.UseCases.DisableJobDefinition;

using SaqueroJobs.Application.DTOs;
using SaqueroJobs.Application.Mappers;
using SaqueroJobs.Domain.Interfaces;

public class DisableJobDefinitionUseCase
{
    private readonly IJobDefinitionRepository _repository;

    public DisableJobDefinitionUseCase(IJobDefinitionRepository repository)
        => _repository = repository;

    public async Task<JobDefinitionDto?> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var job = await _repository.GetByIdAsync(id, ct);
        if (job is null) return null;

        job.Disable();
        await _repository.UpdateAsync(job, ct);
        return JobMapper.ToDto(job);
    }
}
