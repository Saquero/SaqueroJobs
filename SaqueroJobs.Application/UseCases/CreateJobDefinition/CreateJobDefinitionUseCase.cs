namespace SaqueroJobs.Application.UseCases.CreateJobDefinition;

using SaqueroJobs.Application.DTOs;
using SaqueroJobs.Application.Mappers;
using SaqueroJobs.Domain.Entities;
using SaqueroJobs.Domain.Exceptions;
using SaqueroJobs.Domain.Interfaces;
using SaqueroJobs.Domain.ValueObjects;

public class CreateJobDefinitionUseCase
{
    private readonly IJobDefinitionRepository _repository;

    public CreateJobDefinitionUseCase(IJobDefinitionRepository repository)
        => _repository = repository;

    public async Task<JobDefinitionDto> ExecuteAsync(CreateJobDefinitionRequest request, CancellationToken ct = default)
    {
        if (await _repository.ExistsByJobTypeAsync(request.JobType, ct))
            throw new JobDomainException($"A job with type '{request.JobType}' already exists.");

        var policy = new RetryPolicy(request.MaxRetries, request.DelaySeconds);
        var job    = JobDefinition.Create(request.Name, request.Description, request.JobType, request.CronExpression, policy);

        await _repository.AddAsync(job, ct);
        return JobMapper.ToDto(job);
    }
}
