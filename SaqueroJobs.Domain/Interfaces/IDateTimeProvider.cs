namespace SaqueroJobs.Domain.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
