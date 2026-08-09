using Microsoft.EntityFrameworkCore;
using FluentValidation;
using MediatR;
using Payroll.API.Middleware;
using Payroll.Application.Behaviors;
using Payroll.Application.Commands;
using Payroll.Application.Interfaces;
using Payroll.Application.Queries;
using Payroll.Domain.Enums;
using Payroll.Domain.Services;
using Payroll.Infrastructure.Data;
using Payroll.Infrastructure.Repositories;
using Serilog;

// Bootstrap logger: exists before the DI container is built, so it can
// capture anything that goes wrong during startup itself (bad config,
// a service that fails to construct, etc). Replaced by the fully
// DI-integrated Serilog logger once builder.Host.UseSerilog() runs below.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Payroll API");

    var builder = WebApplication.CreateBuilder(args);

    // Route all ASP.NET Core logging (ILogger<T> everywhere, including
    // ExceptionHandlingMiddleware) through Serilog instead of the default
    // provider - no other code needs to change for this to take effect.
    builder.Host.UseSerilog();

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

    // 3b. Register FluentValidation validators (scans the same assembly for any
    // AbstractValidator<T> class) and the pipeline behavior that actually runs
    // them. Order matters here only in the sense that the behavior needs the
    // validators to be resolvable via DI, which AddValidatorsFromAssembly sets up.
    builder.Services.AddValidatorsFromAssembly(typeof(SubmitTimesheetCommand).Assembly);
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

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

    // Logs one line per request (method, path, status code, elapsed time) -
    // separate from ExceptionHandlingMiddleware, which only logs when
    // something goes wrong. This logs every request, success or failure.
    app.UseSerilogRequestLogging();

    // Registered first so it wraps every other middleware and every endpoint
    // below - any exception thrown anywhere downstream gets caught here and
    // turned into a consistent JSON error response instead of a raw 500/stack
    // trace (or, in a few of the old handlers, an inconsistent status code).
    app.UseGlobalExceptionHandling();

    // Enable Swagger UI
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // --- API ENDPOINTS ---
    // No more try/catch here - ArgumentException, NotFoundException, and
    // InvalidOperationException thrown by any handler are now caught once, in
    // ExceptionHandlingMiddleware, and mapped to the right status code there.
    app.MapPost("/api/employees", async (CreateEmployeeCommand command, MediatR.IMediator mediator) =>
    {
        var employeeId = await mediator.Send(command);
        return Results.Created($"/api/employees/{employeeId}", new { Id = employeeId });
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
        var result = await mediator.Send(command);

        // Return a 201 Created status with the new Id and anomaly count.
        // e.g. { "timesheetId": "...", "anomalyCount": 1 }
        return Results.Created($"/api/timesheets/{result.TimesheetId}", result);
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
        var pay = await mediator.Send(new GetTimesheetPayQuery(id));
        return Results.Ok(pay);
    });

    // PUT /api/timesheets/{id}/approve - body is optional; only required when the
    // timesheet has flagged anomalies, in which case the handler rejects the
    // request with a 409 Conflict until an overrideReason is supplied.
    app.MapPut("/api/timesheets/{id:guid}/approve", async (Guid id, ApproveTimesheetRequest? body, MediatR.IMediator mediator) =>
    {
        await mediator.Send(new ApproveTimesheetCommand(id, body?.OverrideReason));
        return Results.NoContent();
    });

    // PUT /api/timesheets/{id}/reject - body (and the reason inside it) is optional.
    app.MapPut("/api/timesheets/{id:guid}/reject", async (Guid id, RejectTimesheetRequest? body, MediatR.IMediator mediator) =>
    {
        await mediator.Send(new RejectTimesheetCommand(id, body?.Reason));
        return Results.NoContent();
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Payroll API terminated unexpectedly during startup");
}
finally
{
    // Flushes any buffered log events before the process exits - without
    // this, the last few log lines can be silently lost on shutdown.
    Log.CloseAndFlush();
}

// Small request-body records for the two PUT endpoints above. Kept here
// rather than in the Application layer since they're pure API transport
// shapes, not part of the MediatR command contract.
record ApproveTimesheetRequest(string? OverrideReason);
record RejectTimesheetRequest(string? Reason);