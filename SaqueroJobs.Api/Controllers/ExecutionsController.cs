namespace SaqueroJobs.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using SaqueroJobs.Application.UseCases.CancelExecution;
using SaqueroJobs.Application.UseCases.GetExecution;
using SaqueroJobs.Application.UseCases.GetExecutionLogs;
using SaqueroJobs.Application.UseCases.ListExecutions;
using SaqueroJobs.Application.UseCases.RetryExecution;

[ApiController]
[Route("api/executions")]
public class ExecutionsController : ControllerBase
{
    private readonly ListExecutionsUseCase   _list;
    private readonly GetExecutionUseCase     _get;
    private readonly CancelExecutionUseCase  _cancel;
    private readonly RetryExecutionUseCase   _retry;
    private readonly GetExecutionLogsUseCase _logs;

    public ExecutionsController(
        ListExecutionsUseCase   list,
        GetExecutionUseCase     get,
        CancelExecutionUseCase  cancel,
        RetryExecutionUseCase   retry,
        GetExecutionLogsUseCase logs)
    {
        _list   = list;
        _get    = get;
        _cancel = cancel;
        _retry  = retry;
        _logs   = logs;
    }

    /// <summary>List all executions — filter by ?status=Pending|Running|Failed etc.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken ct)
        => Ok(await _list.ExecuteAsync(status: status, ct: ct));

    /// <summary>Get a specific execution by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _get.ExecuteAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Cancel a pending or running execution</summary>
    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var result = await _cancel.ExecuteAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Retry a failed or timed-out execution</summary>
    [HttpPost("{id:guid}/retry")]
    public async Task<IActionResult> Retry(Guid id, CancellationToken ct)
    {
        var result = await _retry.ExecuteAsync(id, ct);
        return Accepted(result);
    }

    /// <summary>Get execution logs</summary>
    [HttpGet("{id:guid}/logs")]
    public async Task<IActionResult> GetLogs(Guid id, CancellationToken ct)
        => Ok(await _logs.ExecuteAsync(id, ct));
}
