using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Payroll.Domain.Entities;
using Payroll.Domain.Services;
using Payroll.Application.Interfaces;

namespace Payroll.Application.Commands;

public class SubmitTimesheetCommandHandler : IRequestHandler<SubmitTimesheetCommand, SubmitTimesheetResult>
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAnomalyRepository _anomalyRepository;
    private readonly IAnomalyDetectionService _anomalyDetectionService;

    // Dependency Injection: The .NET engine will automatically provide the database connection here
    public SubmitTimesheetCommandHandler(
    ITimesheetRepository timesheetRepository,
    IEmployeeRepository employeeRepository,
    IAnomalyRepository anomalyRepository,
    IAnomalyDetectionService anomalyDetectionService)
    {
        _timesheetRepository = timesheetRepository;
        _employeeRepository = employeeRepository;
        _anomalyRepository = anomalyRepository;
        _anomalyDetectionService = anomalyDetectionService;
    }

    public async Task<SubmitTimesheetResult> Handle(SubmitTimesheetCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
        if (employee is null)
            throw new NotFoundException("Employee not found.");
        if (!employee.IsActive)
            throw new ArgumentException("Cannot submit timesheet for an inactive employee.");
        
        // 1. Create the Domain Entity (The constructor validates the hours and date)
        var timesheet = new Timesheet(request.EmployeeId, request.WorkDate, request.HoursWorked);

        // 2. Execute the Domain Logic
        timesheet.Submit();

        // 3. Save to the database via our Interface abstraction
        await _timesheetRepository.AddAsync(timesheet);
        await _timesheetRepository.SaveChangesAsync();

        // 4. Run anomaly detection now that the timesheet has a persisted Id.
        // recentTimesheets is loaded AFTER the save above so it will include
        // this same timesheet once - that's fine, the duplicate-date rule
        // explicitly excludes matches on the same Id (see TimesheetAnomalyDetector).
        var recentTimesheets = await _timesheetRepository.GetRecentByEmployeeIdAsync(request.EmployeeId);
        var anomalies = _anomalyDetectionService.Detect(employee, timesheet, recentTimesheets);

        if (anomalies.Count > 0)
        {
            await _anomalyRepository.AddRangeAsync(anomalies);
            await _anomalyRepository.SaveChangesAsync();
        }

        // 5. Return the Id plus how many anomalies were flagged, so the API
        // caller finds out right away without a separate lookup.
        return new SubmitTimesheetResult(timesheet.Id, anomalies.Count);
    }
}