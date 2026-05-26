using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SaqueroJobs.Api.Middleware;
using SaqueroJobs.Application.Interfaces;
using SaqueroJobs.Application.UseCases.CancelExecution;
using SaqueroJobs.Application.UseCases.CreateJobDefinition;
using SaqueroJobs.Application.UseCases.DisableJobDefinition;
using SaqueroJobs.Application.UseCases.EnableJobDefinition;
using SaqueroJobs.Application.UseCases.GetDashboardSummary;
using SaqueroJobs.Application.UseCases.GetExecution;
using SaqueroJobs.Application.UseCases.GetExecutionLogs;
using SaqueroJobs.Application.UseCases.GetJobDefinition;
using SaqueroJobs.Application.UseCases.ListExecutions;
using SaqueroJobs.Application.UseCases.ListJobDefinitions;
using SaqueroJobs.Application.UseCases.RetryExecution;
using SaqueroJobs.Application.UseCases.TriggerJobExecution;
using SaqueroJobs.Domain.Interfaces;
using SaqueroJobs.Infrastructure.Execution;
using SaqueroJobs.Infrastructure.Execution.Handlers;
using SaqueroJobs.Infrastructure.Persistence;
using SaqueroJobs.Infrastructure.Repositories;
using SaqueroJobs.Infrastructure.Scheduling;
using SaqueroJobs.Infrastructure.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// DbContext para requests HTTP normales (Scoped)
builder.Services.AddDbContext<JobsDbContext>(options =>
    options.UseSqlite("Data Source=saquero_jobs.db"));

// DbContextFactory para el scheduler (BackgroundService = Singleton)
// Cada ciclo del scheduler crea su propio DbContext limpio via factory
builder.Services.AddDbContextFactory<JobsDbContext>(options =>
    options.UseSqlite("Data Source=saquero_jobs.db"),
    ServiceLifetime.Scoped);

builder.Services.AddScoped<IJobDefinitionRepository, JobDefinitionRepository>();
builder.Services.AddScoped<IJobExecutionRepository,  JobExecutionRepository>();
builder.Services.AddScoped<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddScoped<IJobHandler, SyncExternalOrdersHandler>();
builder.Services.AddScoped<IJobHandler, GenerateDailyReportHandler>();
builder.Services.AddScoped<IJobHandler, CleanExpiredSessionsHandler>();
builder.Services.AddScoped<IJobHandler, RecalculateCustomerUsageHandler>();
builder.Services.AddScoped<IJobHandler, SendNotificationBatchHandler>();
builder.Services.AddScoped<IJobHandlerRegistry, JobHandlerRegistry>();
builder.Services.AddScoped<CreateJobDefinitionUseCase>();
builder.Services.AddScoped<ListJobDefinitionsUseCase>();
builder.Services.AddScoped<GetJobDefinitionUseCase>();
builder.Services.AddScoped<EnableJobDefinitionUseCase>();
builder.Services.AddScoped<DisableJobDefinitionUseCase>();
builder.Services.AddScoped<TriggerJobExecutionUseCase>();
builder.Services.AddScoped<ListExecutionsUseCase>();
builder.Services.AddScoped<GetExecutionUseCase>();
builder.Services.AddScoped<CancelExecutionUseCase>();
builder.Services.AddScoped<RetryExecutionUseCase>();
builder.Services.AddScoped<GetExecutionLogsUseCase>();
builder.Services.AddScoped<GetDashboardSummaryUseCase>();
builder.Services.AddHostedService<JobSchedulerService>();
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "SaqueroJobs API",
        Version     = "v1",
        Description = "Background Job Processing Engine — .NET 8 · Clean Architecture · DDD · Hexagonal"
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<JobsDbContext>();
    db.Database.EnsureCreated();
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SaqueroJobs v1");
    c.RoutePrefix = "swagger";
});
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
