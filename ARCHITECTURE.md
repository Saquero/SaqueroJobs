# ARCHITECTURE.md — SaqueroJobs

## Overview

SaqueroJobs is built on three architectural principles applied together:

- **Clean Architecture** — dependency rule enforced: outer layers depend on inner layers, never the reverse
- **Hexagonal Architecture (Ports & Adapters)** — the application core is isolated from infrastructure via interfaces
- **Tactical DDD** — domain concepts modeled as Entities, Value Objects, and Aggregates with enforced invariants

---

## Layer Responsibilities

### Domain

The innermost layer. Pure C#, zero framework dependencies.

Owns all business rules. Nothing outside this layer can change how state transitions work.
Domain/
├── Entities/
│   ├── JobDefinition.cs      Aggregate root — defines what a job is
│   ├── JobExecution.cs       Aggregate root — defines one run of a job
│   └── ExecutionLog.cs       Entity — one log entry within an execution
├── ValueObjects/
│   └── RetryPolicy.cs        Immutable — MaxRetries + DelaySeconds
├── Enums/
│   ├── ExecutionStatus.cs    Pending|Running|Completed|Failed|Retrying|Cancelled|TimedOut
│   ├── TriggerSource.cs      Manual|Scheduler
│   └── LogLevel.cs           Info|Warning|Error
├── Interfaces/
│   ├── IJobDefinitionRepository.cs
│   ├── IJobExecutionRepository.cs
│   └── IDateTimeProvider.cs
└── Exceptions/
└── JobDomainException.cs

**Key domain rules enforced in code:**

- `JobDefinition.CanBeTriggered()` — returns false if disabled
- `JobExecution.MarkAsRunning()` — throws if status is not Pending or Retrying
- `JobExecution.MarkAsCancelled()` — throws if status is not Pending or Running
- `JobExecution.CanBeRetried(policy)` — checks status and attempt count against policy
- `JobExecution.IsTerminal()` — Completed, Cancelled, TimedOut are terminal states
- `RetryPolicy` constructor — validates MaxRetries >= 0 and DelaySeconds >= 0

---

### Application

Orchestrates domain logic. Defines use cases and port interfaces. No infrastructure code.
Application/
├── UseCases/
│   ├── CreateJobDefinition/
│   ├── ListJobDefinitions/
│   ├── GetJobDefinition/
│   ├── EnableJobDefinition/
│   ├── DisableJobDefinition/
│   ├── TriggerJobExecution/
│   ├── ListExecutions/
│   ├── GetExecution/
│   ├── CancelExecution/
│   ├── RetryExecution/
│   ├── GetExecutionLogs/
│   └── GetDashboardSummary/
├── DTOs/
│   ├── CreateJobDefinitionRequest.cs
│   ├── JobDefinitionDto.cs
│   ├── JobExecutionDto.cs
│   ├── ExecutionLogDto.cs
│   ├── RetryPolicyDto.cs
│   └── DashboardSummaryDto.cs
├── Interfaces/
│   ├── IJobHandlerRegistry.cs    Port — resolves handler by JobType
│   └── IExecutionLogger.cs       Port — writes execution logs
└── Mappers/
└── JobMapper.cs

Each use case is a single class with one public method `ExecuteAsync`. No base classes, no MediatR. Simple, readable, testable.

---

### Infrastructure

Implements all ports. Contains EF Core, SQLite, job handlers, and the scheduler.
Infrastructure/
├── Persistence/
│   ├── JobsDbContext.cs
│   └── Configurations/
│       ├── JobDefinitionConfiguration.cs
│       ├── JobExecutionConfiguration.cs
│       └── ExecutionLogConfiguration.cs
├── Repositories/
│   ├── JobDefinitionRepository.cs
│   └── JobExecutionRepository.cs
├── Execution/
│   ├── IJobHandler.cs
│   ├── JobHandlerRegistry.cs
│   └── Handlers/
│       ├── SyncExternalOrdersHandler.cs
│       ├── GenerateDailyReportHandler.cs
│       ├── CleanExpiredSessionsHandler.cs
│       ├── RecalculateCustomerUsageHandler.cs
│       └── SendNotificationBatchHandler.cs
├── Scheduling/
│   └── JobSchedulerService.cs
└── Services/
└── DateTimeProvider.cs

**JobHandlerRegistry** resolves handlers by `JobType` string. Adding a new job type requires only a new class that implements `IJobHandler` — no changes to the engine.

**JobSchedulerService** is a `BackgroundService` (HostedService). It polls every 15 seconds for `Pending` and `Retrying` executions and processes them sequentially. Errors are caught per-execution — a single failure does not stop the scheduler.

**EF Core tracking note:** `UpdateAsync` uses `ExecuteUpdateAsync` for scalar fields to avoid EF change tracking conflicts with owned collections. New logs are inserted explicitly by checking persisted IDs before saving.

---

### API

Entry point. Controllers, middleware, DI wiring.
Api/
├── Controllers/
│   ├── JobDefinitionsController.cs
│   ├── ExecutionsController.cs
│   └── DashboardController.cs
├── Middleware/
│   └── ErrorHandlingMiddleware.cs
└── Program.cs

`ErrorHandlingMiddleware` catches `JobDomainException` and returns HTTP 400 with a JSON error body. All other exceptions return HTTP 500.

---

## Dependency Flow
Api → Application → Domain
Infrastructure → Application → Domain
Infrastructure → Domain

Infrastructure knows about Domain and Application.
Application knows about Domain only.
Domain knows nothing.

---

## Key Patterns

**Aggregate roots with private setters** — JobDefinition and JobExecution expose no public setters. All state changes go through domain methods that enforce invariants.

**Value Object** — RetryPolicy is a C# record. Immutable, equality by value, validated on construction.

**Repository pattern** — IJobDefinitionRepository and IJobExecutionRepository are domain interfaces. EF Core implementations live in Infrastructure.

**Port & Adapter** — IJobHandlerRegistry is an Application port. JobHandlerRegistry in Infrastructure is the adapter. The application layer never references a concrete handler.

**HostedService scheduler** — JobSchedulerService uses IServiceScopeFactory to create a fresh DI scope per polling cycle. This avoids DbContext lifetime issues in long-running background services.

---

## Domain Model Diagram
┌─────────────────────────────────┐      ┌──────────────────────────────────┐
│         JobDefinition           │      │          JobExecution            │
│─────────────────────────────────│      │──────────────────────────────────│
│ Id: Guid                        │      │ Id: Guid                         │
│ Name: string                    │      │ JobDefinitionId: Guid ───────────┼──►
│ Description: string             │      │ Status: ExecutionStatus          │
│ JobType: string                 │      │ TriggeredBy: TriggerSource       │
│ CronExpression: string?         │      │ AttemptNumber: int               │
│ IsEnabled: bool                 │      │ StartedAt: DateTime?             │
│ RetryPolicy: RetryPolicy ──────►│      │ CompletedAt: DateTime?           │
│ CreatedAt: DateTime             │      │ ErrorMessage: string?            │
│ UpdatedAt: DateTime             │      │ Logs: IReadOnlyCollection        │
└─────────────────────────────────┘      └──────────────────────────────────┘
│
┌────────────────▼─────────────────┐
┌───────────────────┐           │           ExecutionLog           │
│    RetryPolicy    │           │──────────────────────────────────│
│───────────────────│           │ Id: Guid                         │
│ MaxRetries: int   │           │ ExecutionId: Guid                │
│ DelaySeconds: int │           │ Message: string                  │
└───────────────────┘           │ Level: LogLevel                  │
│ Timestamp: DateTime              │
└──────────────────────────────────┘

---

## Testing Strategy

**Domain tests** — test entity behavior in isolation. No mocks, no database. Pure C# objects.

**Application tests** — test use case logic with mocked repositories via Moq. Verify that the right repository methods are called with the right arguments.

Infrastructure and API are not unit tested — they are integration concerns.

Current coverage: **18 tests, 0 failures.**


