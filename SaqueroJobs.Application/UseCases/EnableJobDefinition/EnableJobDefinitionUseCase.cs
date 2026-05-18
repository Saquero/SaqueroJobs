namespace SaqueroJobs.Application.UseCases.EnableJobDefinition;

using SaqueroJobs.Application.DTOs;
using SaqueroJobs.Application.Mappers;
using SaqueroJobs.Domain.Interfaces;

public class EnableJobDefinitionUseCase
{
    private readonly IJobDefinitionRepository _repository;

    public EnableJobDefinitionUseCase(IJobDefinitionRepository repository)
        => _repository = repository;

    public async Task<JobDefinitionDto?> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var job = await _repository.GetByIdAsync(id, ct);
        if (job is null) return null;

        job.Enable();
        await _repository.UpdateAsync(job, ct);
        return JobMapper.ToDto(job);
    }
}
