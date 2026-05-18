namespace SaqueroJobs.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using SaqueroJobs.Application.UseCases.GetDashboardSummary;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly GetDashboardSummaryUseCase _summary;

    public DashboardController(GetDashboardSummaryUseCase summary)
        => _summary = summary;

    /// <summary>Get system-wide job execution summary</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
        => Ok(await _summary.ExecuteAsync(ct));
}
