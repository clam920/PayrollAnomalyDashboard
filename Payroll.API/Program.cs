using Microsoft.EntityFrameworkCore;
using Payroll.Application.Commands;
using Payroll.Application.Interfaces;
using Payroll.Infrastructure.Data;
using Payroll.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure the SQLite Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=payroll.db"));

// 2. Register the Repository (Scoped means one instance per HTTP request)
builder.Services.AddScoped<ITimesheetRepository, TimesheetRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

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

// The Endpoint: POST /api/timesheets
app.MapPost("/api/timesheets", async (SubmitTimesheetCommand command, MediatR.IMediator mediator) =>
{
    try
    {
        // Hand the command off to MediatR
        var timesheetId = await mediator.Send(command);
        
        // Return a 201 Created status with the new ID
        return Results.Created($"/api/timesheets/{timesheetId}", new { Id = timesheetId });
    }
    catch (ArgumentException ex)
    {
        // If the Domain rules reject the data (e.g., negative hours), return a 400 Bad Request
        return Results.BadRequest(new { Error = ex.Message });
    }
});

app.Run();