using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;

namespace Payroll.Application.Interfaces;

public interface IAnomalyRepository
{
    // Anomalies are always created in a batch (a single timesheet submission
    // can trigger multiple rules at once), so this takes a collection rather
    // than one-at-a-time like the other repositories' AddAsync.
    Task AddRangeAsync(IEnumerable<Anomaly> anomalies);
    Task SaveChangesAsync();

    // For the GET /api/anomalies endpoint. minSeverity is optional so callers
    // can e.g. ask for only Warning-and-above without a separate method per
    // severity level.
    Task<List<Anomaly>> GetAllAsync(AnomalySeverity? minSeverity = null);

    // Used by ApproveTimesheetCommandHandler to decide whether an override
    // reason is required before a timesheet can be approved.
    Task<List<Anomaly>> GetByTimesheetIdAsync(Guid timesheetId);
}