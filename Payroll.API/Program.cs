using Microsoft.EntityFrameworkCore;
using Payroll.Application.Commands;
using Payroll.Application.Interfaces;
using Payroll.Application.Queries;
using Payroll.Domain.Enums;
using Payroll.Domain.Services;
using Payroll.Infrastructure.Data;
using Payroll.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure the SQLite Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=payroll.db"));

// 2. Register the Repository (Scoped means one instance per HTTP request)
builder.Services.AddScoped<ITimesheetRepository, TimesheetRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IAnomalyRepository, AnomalyRepository>();

// 2b. Register the payroll calculation and anomaly detection domain
// services. Both are stateless (no fields, no DB access) so they're safe -
// and cheaper - as Singletons rather than Scoped like the repositories
// above. TimesheetAnomalyDetector depends on IPayrollCalculationService, so
// that one must be registered first (DI resolves it when building the
// detector).
builder.Services.AddSingleton<IPayrollCalculationService, PayrollCalculationService>();
builder.Services.AddSingleton<IAnomalyDetectionService, TimesheetAnomalyDetector>();

// 3. Register MediatR
// This tells MediatR to scan the assembly (project) where SubmitTimesheetCommand lives and register all handlers
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SubmitTimesheetCommand).Assembly));

// 4. Add Swagger for easy API testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 5. Ensure the database is created (Do not use this in production, but great for portfolios!)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

// Enable Swagger UI
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

// GET /api/employees - list every employee (active and inactive)
app.MapGet("/api/employees", async (MediatR.IMediator mediator) =>
{
    var employees = await mediator.Send(new GetEmployeesQuery());
    return Results.Ok(employees);
});

// The Endpoint: POST /api/timesheets
app.MapPost("/api/timesheets", async (SubmitTimesheetCommand command, MediatR.IMediator mediator) =>
{
    try
    {
        // Hand the command off to MediatR
        var result = await mediator.Send(command);
        
        // Return a 201 Created status with the new Id and anomaly count.
        // e.g. { "timesheetId": "...", "anomalyCount": 1 }
        return Results.Created($"/api/timesheets/{result.TimesheetId}", result);
    }
    catch (ArgumentException ex)
    {
        // If the Domain rules reject the data (e.g., negative hours), return a 400 Bad Request
        return Results.BadRequest(new { Error = ex.Message });
    }
});

// GET /api/timesheets?employeeId=... - employeeId is optional; omit it to list everyone's timesheets
app.MapGet("/api/timesheets", async (Guid? employeeId, MediatR.IMediator mediator) =>
{
    var timesheets = await mediator.Send(new GetTimesheetsQuery(employeeId));
    return Results.Ok(timesheets);
});

// GET /api/anomalies?minSeverity=... - minSeverity is optional (Info/Warning/Critical);
// omit it to see every flagged anomaly regardless of severity.
app.MapGet("/api/anomalies", async (AnomalySeverity? minSeverity, MediatR.IMediator mediator) =>
{
    var anomalies = await mediator.Send(new GetAnomaliesQuery(minSeverity));
    return Results.Ok(anomalies);
});

// GET /api/timesheets/{id}/pay - regular/overtime hours and pay for one timesheet,
// calculated on the fly rather than stored (so it always reflects the
// employee's current hourly rate, not whatever it was at submission time).
app.MapGet("/api/timesheets/{id:guid}/pay", async (Guid id, MediatR.IMediator mediator) =>
{
    try
    {
        var pay = await mediator.Send(new GetTimesheetPayQuery(id));
        return Results.Ok(pay);
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound(new { Error = ex.Message });
    }
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
        // e.g. "requires an override reason" or "must be submitted before approval"
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