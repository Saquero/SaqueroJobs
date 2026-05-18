namespace SaqueroJobs.Domain.Tests;

using Xunit;
using FluentAssertions;
using SaqueroJobs.Domain.Entities;
using SaqueroJobs.Domain.Enums;
using SaqueroJobs.Domain.Exceptions;
using SaqueroJobs.Domain.ValueObjects;

public class JobDefinitionTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateEnabledDefinition()
    {
        var job = JobDefinition.Create("Sync Orders", "Demo job", "SyncExternalOrdersJob");

        job.Id.Should().NotBeEmpty();
        job.Name.Should().Be("Sync Orders");
        job.Description.Should().Be("Demo job");
        job.JobType.Should().Be("SyncExternalOrdersJob");
        job.IsEnabled.Should().BeTrue();
        job.RetryPolicy.Should().Be(RetryPolicy.Default);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowDomainException()
    {
        var act = () => JobDefinition.Create("", "Demo", "SyncExternalOrdersJob");

        act.Should().Throw<JobDomainException>()
            .WithMessage("*name*empty*");
    }

    [Fact]
    public void Create_WithEmptyJobType_ShouldThrowDomainException()
    {
        var act = () => JobDefinition.Create("Sync Orders", "Demo", "");

        act.Should().Throw<JobDomainException>()
            .WithMessage("*type*empty*");
    }

    [Fact]
    public void Disable_ShouldPreventTriggering()
    {
        var job = JobDefinition.Create("Sync Orders", "Demo", "SyncExternalOrdersJob");

        job.Disable();

        job.IsEnabled.Should().BeFalse();
        job.CanBeTriggered().Should().BeFalse();
    }

    [Fact]
    public void Enable_AfterDisable_ShouldAllowTriggering()
    {
        var job = JobDefinition.Create("Sync Orders", "Demo", "SyncExternalOrdersJob");

        job.Disable();
        job.Enable();

        job.IsEnabled.Should().BeTrue();
        job.CanBeTriggered().Should().BeTrue();
    }

    [Fact]
    public void UpdateRetryPolicy_ShouldReplacePolicy()
    {
        var job = JobDefinition.Create("Sync Orders", "Demo", "SyncExternalOrdersJob");
        var policy = new RetryPolicy(5, 60);

        job.UpdateRetryPolicy(policy);

        job.RetryPolicy.Should().Be(policy);
    }
}

public class JobExecutionTests
{
    [Fact]
    public void Create_ShouldCreatePendingExecution()
    {
        var jobDefinitionId = Guid.NewGuid();

        var execution = JobExecution.Create(jobDefinitionId, TriggerSource.Manual);

        execution.Id.Should().NotBeEmpty();
        execution.JobDefinitionId.Should().Be(jobDefinitionId);
        execution.Status.Should().Be(ExecutionStatus.Pending);
        execution.TriggeredBy.Should().Be(TriggerSource.Manual);
        execution.AttemptNumber.Should().Be(1);
        execution.Logs.Should().BeEmpty();
    }

    [Fact]
    public void MarkAsRunning_FromPending_ShouldSetRunningAndCreateLog()
    {
        var execution = JobExecution.Create(Guid.NewGuid(), TriggerSource.Manual);

        execution.MarkAsRunning();

        execution.Status.Should().Be(ExecutionStatus.Running);
        execution.StartedAt.Should().NotBeNull();
        execution.Logs.Should().ContainSingle();
    }

    [Fact]
    public void MarkAsRunning_FromRunning_ShouldThrowDomainException()
    {
        var execution = JobExecution.Create(Guid.NewGuid(), TriggerSource.Manual);
        execution.MarkAsRunning();

        var act = () => execution.MarkAsRunning();

        act.Should().Throw<JobDomainException>();
    }

    [Fact]
    public void MarkAsCompleted_FromRunning_ShouldSetCompletedStatus()
    {
        var execution = JobExecution.Create(Guid.NewGuid(), TriggerSource.Manual);
        execution.MarkAsRunning();

        execution.MarkAsCompleted();

        execution.Status.Should().Be(ExecutionStatus.Completed);
        execution.CompletedAt.Should().NotBeNull();
        execution.IsTerminal().Should().BeTrue();
    }

    [Fact]
    public void MarkAsCompleted_FromPending_ShouldThrowDomainException()
    {
        var execution = JobExecution.Create(Guid.NewGuid(), TriggerSource.Manual);

        var act = () => execution.MarkAsCompleted();

        act.Should().Throw<JobDomainException>();
    }

    [Fact]
    public void MarkAsFailed_ShouldSetFailedStatusAndErrorMessage()
    {
        var execution = JobExecution.Create(Guid.NewGuid(), TriggerSource.Manual);
        execution.MarkAsRunning();

        execution.MarkAsFailed("Boom");

        execution.Status.Should().Be(ExecutionStatus.Failed);
        execution.ErrorMessage.Should().Be("Boom");
        execution.CompletedAt.Should().NotBeNull();
        execution.Logs.Should().NotBeEmpty();
    }

    [Fact]
    public void MarkAsCancelled_FromPending_ShouldSetCancelledStatus()
    {
        var execution = JobExecution.Create(Guid.NewGuid(), TriggerSource.Manual);

        execution.MarkAsCancelled();

        execution.Status.Should().Be(ExecutionStatus.Cancelled);
        execution.CompletedAt.Should().NotBeNull();
        execution.IsTerminal().Should().BeTrue();
    }

    [Fact]
    public void CanBeRetried_WhenFailedAndAttemptsAvailable_ShouldReturnTrue()
    {
        var execution = JobExecution.Create(Guid.NewGuid(), TriggerSource.Manual);
        execution.MarkAsRunning();
        execution.MarkAsFailed("Boom");

        var result = execution.CanBeRetried(new RetryPolicy(3, 30));

        result.Should().BeTrue();
    }

    [Fact]
    public void CanBeRetried_WhenAttemptLimitReached_ShouldReturnFalse()
    {
        var execution = JobExecution.Create(Guid.NewGuid(), TriggerSource.Manual, attemptNumber: 3);
        execution.MarkAsRunning();
        execution.MarkAsFailed("Boom");

        var result = execution.CanBeRetried(new RetryPolicy(3, 30));

        result.Should().BeFalse();
    }
}

public class RetryPolicyTests
{
    [Fact]
    public void Create_WithNegativeMaxRetries_ShouldThrowDomainException()
    {
        var act = () => new RetryPolicy(-1, 30);

        act.Should().Throw<JobDomainException>();
    }

    [Fact]
    public void Create_WithNegativeDelaySeconds_ShouldThrowDomainException()
    {
        var act = () => new RetryPolicy(3, -1);

        act.Should().Throw<JobDomainException>();
    }
}
