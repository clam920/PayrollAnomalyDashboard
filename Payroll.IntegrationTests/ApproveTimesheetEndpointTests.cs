using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Payroll.IntegrationTests;

public class ApproveTimesheetEndpointTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public ApproveTimesheetEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        // Manager, not Employee - this test class exercises Approve, which
        // is a ManagerOnly-policy endpoint. Creating employees and
        // submitting timesheets both also happen through this same client.
        _client = await _factory.CreateAuthenticatedClientAsync("manager", "Manager123!");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ApprovingAFlaggedTimesheetWithoutAReason_Returns409Conflict()
    {
        var employeeId = await CreateEmployeeAsync();
        var timesheetId = await SubmitTimesheetAsync(employeeId, hoursWorked: 13m);

        var response = await _client.PutAsJsonAsync($"/api/timesheets/{timesheetId}/approve", new { });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ApprovingAFlaggedTimesheetWithAReason_Returns204NoContent()
    {
        var employeeId = await CreateEmployeeAsync();
        var timesheetId = await SubmitTimesheetAsync(employeeId, hoursWorked: 13m);

        var response = await _client.PutAsJsonAsync($"/api/timesheets/{timesheetId}/approve",
            new { OverrideReason = "Confirmed with employee - legitimate double shift." });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ApprovingANonExistentTimesheet_Returns404NotFound()
    {
        var response = await _client.PutAsJsonAsync($"/api/timesheets/{Guid.NewGuid()}/approve", new { });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ApprovingAsAnEmployeeRole_Returns403Forbidden()
    {
        // Proves the ManagerOnly policy is actually enforced, not just that
        // authentication in general works - an Employee-role token is
        // valid and authenticated, but shouldn't be authorized here.
        var employeeId = await CreateEmployeeAsync();
        var timesheetId = await SubmitTimesheetAsync(employeeId, hoursWorked: 13m);

        var employeeClient = await _factory.CreateAuthenticatedClientAsync("employee", "Employee123!");
        var response = await employeeClient.PutAsJsonAsync($"/api/timesheets/{timesheetId}/approve",
            new { OverrideReason = "Employee role attempting a Manager-only action." });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<Guid> CreateEmployeeAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/employees", new
        {
            FirstName = "Approve",
            LastName = "Flow",
            HourlyRate = 20m
        });
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CreatedEmployeeResponse>();
        return created!.Id;
    }

    private async Task<Guid> SubmitTimesheetAsync(Guid employeeId, decimal hoursWorked)
    {
        var response = await _client.PostAsJsonAsync("/api/timesheets", new
        {
            EmployeeId = employeeId,
            WorkDate = TestDates.GetLastWeekday(),
            HoursWorked = hoursWorked
        });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SubmitTimesheetResponse>();
        return result!.TimesheetId;
    }

    private record CreatedEmployeeResponse(Guid Id);
    private record SubmitTimesheetResponse(Guid TimesheetId, int AnomalyCount);
}