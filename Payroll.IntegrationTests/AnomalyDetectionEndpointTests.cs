using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Payroll.IntegrationTests;

public class AnomalyDetectionEndpointTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public AnomalyDetectionEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _client = await _factory.CreateAuthenticatedClientAsync("manager", "Manager123!");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SubmittingAnExcessiveHoursTimesheet_IsFlaggedAndVisibleViaTheAnomaliesEndpoint()
    {
        var createEmployeeResponse = await _client.PostAsJsonAsync("/api/employees", new
        {
            FirstName = "Integration",
            LastName = "Test",
            HourlyRate = 25m
        });
        createEmployeeResponse.EnsureSuccessStatusCode();
        var employee = await createEmployeeResponse.Content.ReadFromJsonAsync<CreatedEmployeeResponse>();

        var submitResponse = await _client.PostAsJsonAsync("/api/timesheets", new
        {
            EmployeeId = employee!.Id,
            WorkDate = TestDates.GetLastWeekday(),
            HoursWorked = 13m
        });

        submitResponse.EnsureSuccessStatusCode();
        var submitResult = await submitResponse.Content.ReadFromJsonAsync<SubmitTimesheetResponse>();
        Assert.Equal(1, submitResult!.AnomalyCount);

        var anomaliesResponse = await _client.GetAsync("/api/anomalies?minSeverity=Warning");
        anomaliesResponse.EnsureSuccessStatusCode();
        var anomalies = await anomaliesResponse.Content.ReadFromJsonAsync<List<AnomalyResponse>>();
        Assert.Contains(anomalies!, a => a.EmployeeId == employee.Id);
    }

    private record CreatedEmployeeResponse(Guid Id);
    private record SubmitTimesheetResponse(Guid TimesheetId, int AnomalyCount);
    private record AnomalyResponse(Guid Id, Guid EmployeeId, string Type, string Severity, string Description, DateTime DetectedAtUtc);
}