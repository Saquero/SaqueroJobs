namespace SaqueroJobs.Application.UseCases.GetJobDefinition;

using SaqueroJobs.Application.DTOs;
using SaqueroJobs.Application.Mappers;
using SaqueroJobs.Domain.Interfaces;

public class GetJobDefinitionUseCase
{
    private readonly IJobDefinitionRepository _repository;

    public GetJobDefinitionUseCase(IJobDefinitionRepository repository)
        => _repository = repository;

    public async Task<JobDefinitionDto?> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var job = await _repository.GetByIdAsync(id, ct);
        return job is null ? null : JobMapper.ToDto(job);
    }
}
