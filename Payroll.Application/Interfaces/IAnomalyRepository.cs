using System.Collections.Generic;
using System.Threading.Tasks;
using Payroll.Domain.Entities;

namespace Payroll.Application.Interfaces;

public interface IAnomalyRepository
{
    // Anomalies are always created in a batch (a single timesheet submission
    // can trigger multiple rules at once), so this takes a collection rather
    // than one-at-a-time like the other repositories' AddAsync.
    Task AddRangeAsync(IEnumerable<Anomaly> anomalies);
    Task SaveChangesAsync();
}