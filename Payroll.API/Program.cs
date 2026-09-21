using Microsoft.EntityFrameworkCore;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Payroll.API.Middleware;
using Payroll.Application.Auth;
using Payroll.Application.Behaviors;
using Payroll.Application.Commands;
using Payroll.Application.Interfaces;
using Payroll.Application.Queries;
using Payroll.Domain.Enums;
using Payroll.Domain.Services;
using Payroll.Infrastructure.Auth;
using Payroll.Infrastructure.Data;
using Payroll.Infrastructure.Repositories;
using Serilog;
using System.Text;

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

    // 1. Configure the PostgreSQL database. Connection string comes from
    // configuration (appsettings.json / environment variables), not a
    // hardcoded literal - see ConnectionStrings:DefaultConnection.
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Paste just the token - Swagger adds the 'Bearer ' prefix automatically. Get one from POST /api/auth/login."
        });

        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });
    
    // 5. Configure JWT authentication + role-based authorization.
    // JwtSettings is bound manually (not via IOptions<T>) and registered as
    // a plain singleton, so JwtTokenGenerator can take it as an ordinary
    // constructor dependency without an extra package reference.
    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
        ?? throw new InvalidOperationException("Missing 'Jwt' configuration section.");
    builder.Services.AddSingleton(jwtSettings);

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey))
            };
        });

    // Named policy rather than repeating RequireRole(Roles.Manager) at every
    // endpoint - if the "who can approve/reject" rule ever needs a second
    // role, it changes in exactly one place.
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("ManagerOnly", policy => policy.RequireRole(Roles.Manager));
    });

    // 5b. Register the auth-related services. IPasswordHasher<AppUser> comes
    // from the ASP.NET Core shared framework (Sdk.Web projects get it for
    // free) - PasswordHasher<T> itself needs no database or external state,
    // so Singleton is safe here same as the domain services above.
    builder.Services.AddSingleton<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
    builder.Services.AddSingleton<IUserStore, InMemoryUserStore>();
    builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

    var app = builder.Build();

    // 5. Apply any pending EF Core migrations on startup. This replaces the
    // old EnsureCreated() call: EnsureCreated() builds a schema straight from
    // the current model with no history and no ability to evolve it later -
    // Migrate() applies the ordered set of migration files in Migrations/,
    // which is what makes schema changes trackable and repeatable across
    // environments (dev machine, CI, a real deployment) instead of a
    // "just delete the .db file" workaround.
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
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

    // Must come after UseHttpsRedirection and before any endpoint mapping.
    // Authentication figures out WHO is calling (validates the JWT,
    // populates HttpContext.User); Authorization then checks WHETHER that
    // identity is allowed to hit the specific endpoint - order between the
    // two matters, you can't authorize an identity that hasn't been
    // established yet.
    app.UseAuthentication();
    app.UseAuthorization();

    // --- API ENDPOINTS ---
    // POST /api/auth/login - the only endpoint that's intentionally
    // anonymous; every endpoint below requires a valid Bearer token.
    app.MapPost("/api/auth/login", async (LoginCommand command, MediatR.IMediator mediator) =>
    {
        var result = await mediator.Send(command);
        return Results.Ok(result);
    });

    // No more try/catch here - ArgumentException, NotFoundException, and
    // InvalidOperationException thrown by any handler are now caught once, in
    // ExceptionHandlingMiddleware, and mapped to the right status code there.
    //
    // Authorization split: creating employees, approving/rejecting, and the
    // dashboard summary are Manager-only (HR/admin-style actions); everything
    // else just requires SOME authenticated user (.RequireAuthorization()
    // with no policy name), whether Employee or Manager.
    app.MapPost("/api/employees", async (CreateEmployeeCommand command, MediatR.IMediator mediator) =>
    {
        var employeeId = await mediator.Send(command);
        return Results.Created($"/api/employees/{employeeId}", new { Id = employeeId });
    }).RequireAuthorization("ManagerOnly");

    // GET /api/employees - list every employee (active and inactive)
    app.MapGet("/api/employees", async (MediatR.IMediator mediator) =>
    {
        var employees = await mediator.Send(new GetEmployeesQuery());
        return Results.Ok(employees);
    }).RequireAuthorization();

    // The Endpoint: POST /api/timesheets
    app.MapPost("/api/timesheets", async (SubmitTimesheetCommand command, MediatR.IMediator mediator) =>
    {
        var result = await mediator.Send(command);

        // Return a 201 Created status with the new Id and anomaly count.
        // e.g. { "timesheetId": "...", "anomalyCount": 1 }
        return Results.Created($"/api/timesheets/{result.TimesheetId}", result);
    }).RequireAuthorization();

    // GET /api/timesheets?employeeId=... - employeeId is optional; omit it to list everyone's timesheets
    app.MapGet("/api/timesheets", async (Guid? employeeId, MediatR.IMediator mediator) =>
    {
        var timesheets = await mediator.Send(new GetTimesheetsQuery(employeeId));
        return Results.Ok(timesheets);
    }).RequireAuthorization();

    // GET /api/anomalies?minSeverity=... - minSeverity is optional (Info/Warning/Critical);
    // omit it to see every flagged anomaly regardless of severity.
    app.MapGet("/api/anomalies", async (AnomalySeverity? minSeverity, MediatR.IMediator mediator) =>
    {
        var anomalies = await mediator.Send(new GetAnomaliesQuery(minSeverity));
        return Results.Ok(anomalies);
    }).RequireAuthorization();

    // GET /api/timesheets/{id}/pay - regular/overtime hours and pay for one timesheet,
    // calculated on the fly rather than stored (so it always reflects the
    // employee's current hourly rate, not whatever it was at submission time).
    app.MapGet("/api/timesheets/{id:guid}/pay", async (Guid id, MediatR.IMediator mediator) =>
    {
        var pay = await mediator.Send(new GetTimesheetPayQuery(id));
        return Results.Ok(pay);
    }).RequireAuthorization();

    // PUT /api/timesheets/{id}/approve - Manager only. Body is optional;
    // only required when the timesheet has flagged anomalies, in which case
    // the handler rejects the request with a 409 Conflict until an
    // overrideReason is supplied.
    app.MapPut("/api/timesheets/{id:guid}/approve", async (Guid id, ApproveTimesheetRequest? body, MediatR.IMediator mediator) =>
    {
        await mediator.Send(new ApproveTimesheetCommand(id, body?.OverrideReason));
        return Results.NoContent();
    }).RequireAuthorization("ManagerOnly");

    // PUT /api/timesheets/{id}/reject - Manager only. Body (and the reason inside it) is optional.
    app.MapPut("/api/timesheets/{id:guid}/reject", async (Guid id, RejectTimesheetRequest? body, MediatR.IMediator mediator) =>
    {
        await mediator.Send(new RejectTimesheetCommand(id, body?.Reason));
        return Results.NoContent();
    }).RequireAuthorization("ManagerOnly");

    // GET /api/dashboard/summary?topEmployeeCount=... - Manager only. Total
    // anomaly count, a breakdown by severity and by type, and a leaderboard
    // of the most-flagged employees (topEmployeeCount defaults to 5 if
    // omitted). This is the aggregate view the "Dashboard" half of the
    // app's name refers to - treated as a reporting/management view.
    app.MapGet("/api/dashboard/summary", async (int? topEmployeeCount, MediatR.IMediator mediator) =>
    {
        var summary = await mediator.Send(new GetDashboardSummaryQuery(topEmployeeCount ?? 5));
        return Results.Ok(summary);
    }).RequireAuthorization("ManagerOnly");

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

// Top-level statements generate an internal Program class by default - a
// different test assembly (Payroll.IntegrationTests) can't reference an
// internal type, so this explicit partial declaration is what makes
// WebApplicationFactory<Program> possible there.
public partial class Program { }