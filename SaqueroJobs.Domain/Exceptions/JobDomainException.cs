namespace SaqueroJobs.Domain.Exceptions;

public class JobDomainException : Exception
{
    public JobDomainException(string message) : base(message) { }
}
