namespace SaqueroJobs.Application.UseCases.ListJobDefinitions;

using SaqueroJobs.Application.DTOs;
using SaqueroJobs.Application.Mappers;
using SaqueroJobs.Domain.Interfaces;

public class ListJobDefinitionsUseCase
{
    private readonly IJobDefinitionRepository _repository;

    public ListJobDefinitionsUseCase(IJobDefinitionRepository repository)
        => _repository = repository;

    public async Task<IEnumerable<JobDefinitionDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var jobs = await _repository.GetAllAsync(ct);
        return jobs.Select(JobMapper.ToDto);
    }
}
