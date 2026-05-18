namespace SaqueroJobs.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using SaqueroJobs.Application.DTOs;
using SaqueroJobs.Application.UseCases.CreateJobDefinition;
using SaqueroJobs.Application.UseCases.DisableJobDefinition;
using SaqueroJobs.Application.UseCases.EnableJobDefinition;
using SaqueroJobs.Application.UseCases.GetJobDefinition;
using SaqueroJobs.Application.UseCases.ListJobDefinitions;
using SaqueroJobs.Application.UseCases.TriggerJobExecution;
using SaqueroJobs.Application.UseCases.ListExecutions;

[ApiController]
[Route("api/jobs")]
public class JobDefinitionsController : ControllerBase
{
    private readonly CreateJobDefinitionUseCase  _create;
    private readonly ListJobDefinitionsUseCase   _list;
    private readonly GetJobDefinitionUseCase     _get;
    private readonly EnableJobDefinitionUseCase  _enable;
    private readonly DisableJobDefinitionUseCase _disable;
    private readonly TriggerJobExecutionUseCase  _trigger;
    private readonly ListExecutionsUseCase       _listExecutions;

    public JobDefinitionsController(
        CreateJobDefinitionUseCase  create,
        ListJobDefinitionsUseCase   list,
        GetJobDefinitionUseCase     get,
        EnableJobDefinitionUseCase  enable,
        DisableJobDefinitionUseCase disable,
        TriggerJobExecutionUseCase  trigger,
        ListExecutionsUseCase       listExecutions)
    {
        _create         = create;
        _list           = list;
        _get            = get;
        _enable         = enable;
        _disable        = disable;
        _trigger        = trigger;
        _listExecutions = listExecutions;
    }

    /// <summary>Create a new job definition</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobDefinitionRequest request, CancellationToken ct)
    {
        var result = await _create.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>List all job definitions</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _list.ExecuteAsync(ct));

    /// <summary>Get a job definition by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _get.ExecuteAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Enable a job definition</summary>
    [HttpPatch("{id:guid}/enable")]
    public async Task<IActionResult> Enable(Guid id, CancellationToken ct)
    {
        var result = await _enable.ExecuteAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Disable a job definition</summary>
    [HttpPatch("{id:guid}/disable")]
    public async Task<IActionResult> Disable(Guid id, CancellationToken ct)
    {
        var result = await _disable.ExecuteAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Trigger a manual execution of a job</summary>
    [HttpPost("{id:guid}/run")]
    public async Task<IActionResult> Run(Guid id, CancellationToken ct)
    {
        var result = await _trigger.ExecuteAsync(id, ct);
        return Accepted(result);
    }

    /// <summary>Get all executions for a specific job</summary>
    [HttpGet("{id:guid}/executions")]
    public async Task<IActionResult> GetExecutions(Guid id, CancellationToken ct)
        => Ok(await _listExecutions.ExecuteAsync(jobDefinitionId: id, ct: ct));
}
