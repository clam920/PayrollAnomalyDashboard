using Microsoft.EntityFrameworkCore;
using Payroll.Application.Commands;
using Payroll.Application.Interfaces;
using Payroll.Application.Queries;
using Payroll.Domain.Enums;
using Payroll.Domain.Services;
using Payroll.Infrastructure.Data;
using Payroll.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=payroll.db"));

builder.Services.AddScoped<ITimesheetRepository, TimesheetRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IAnomalyRepository, AnomalyRepository>();
builder.Services.AddSingleton<IAnomalyDetectionService, TimesheetAnomalyDetector>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SubmitTimesheetCommand).Assembly));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- API ENDPOINTS ---
app.MapPost("/api/employees", async (CreateEmployeeCommand command, MediatR.IMediator mediator) =>
{
    try
    {
        var employeeId = await mediator.Send(command);
        return Results.Created($"/api/employees/{employeeId}", new { Id = employeeId });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { Error = ex.Message });
    }
});

app.MapGet("/api/employees", async (MediatR.IMediator mediator) =>
{
    var employees = await mediator.Send(new GetEmployeesQuery());
    return Results.Ok(employees);
});

app.MapPost("/api/timesheets", async (SubmitTimesheetCommand command, MediatR.IMediator mediator) =>
{
    try
    {
        var result = await mediator.Send(command);
        return Results.Created($"/api/timesheets/{result.TimesheetId}", result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { Error = ex.Message });
    }
});

app.MapGet("/api/timesheets", async (Guid? employeeId, MediatR.IMediator mediator) =>
{
    var timesheets = await mediator.Send(new GetTimesheetsQuery(employeeId));
    return Results.Ok(timesheets);
});

app.MapGet("/api/anomalies", async (AnomalySeverity? minSeverity, MediatR.IMediator mediator) =>
{
    var anomalies = await mediator.Send(new GetAnomaliesQuery(minSeverity));
    return Results.Ok(anomalies);
});

// PUT /api/timesheets/{id}/approve - body is optional; only required when the
// timesheet has flagged anomalies, in which case the handler rejects the
// request with a 409 Conflict until an overrideReason is supplied.
app.MapPut("/api/timesheets/{id:guid}/approve", async (Guid id, ApproveTimesheetRequest? body, MediatR.IMediator mediator) =>
{
    try
    {
        await mediator.Send(new ApproveTimesheetCommand(id, body?.OverrideReason));
        return Results.NoContent();
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound(new { Error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { Error = ex.Message });
    }
});

// PUT /api/timesheets/{id}/reject - body (and the reason inside it) is optional.
app.MapPut("/api/timesheets/{id:guid}/reject", async (Guid id, RejectTimesheetRequest? body, MediatR.IMediator mediator) =>
{
    try
    {
        await mediator.Send(new RejectTimesheetCommand(id, body?.Reason));
        return Results.NoContent();
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound(new { Error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { Error = ex.Message });
    }
});

app.Run();

// Small request-body records for the two PUT endpoints above. Kept here
// rather than in the Application layer since they're pure API transport
// shapes, not part of the MediatR command contract.
record ApproveTimesheetRequest(string? OverrideReason);
record RejectTimesheetRequest(string? Reason);