namespace SaqueroJobs.Application.Interfaces;

using SaqueroJobs.Domain.Entities;
using SaqueroJobs.Domain.Enums;

public interface IExecutionLogger
{
    Task LogAsync(Guid executionId, string message, LogLevel level, CancellationToken ct = default);
}
