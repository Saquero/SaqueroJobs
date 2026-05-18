namespace SaqueroJobs.Infrastructure.Services;

using SaqueroJobs.Domain.Interfaces;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
