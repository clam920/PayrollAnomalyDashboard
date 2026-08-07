using System.Collections.Generic;
using System.Threading.Tasks;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;

namespace Payroll.Application.Interfaces;

public interface IAnomalyRepository
{
    Task AddRangeAsync(IEnumerable<Anomaly> anomalies);
    Task SaveChangesAsync();

    // For the GET /api/anomalies endpoint. minSeverity is optional so callers
    // can e.g. ask for only Warning-and-above without a separate method per
    // severity level.
    Task<List<Anomaly>> GetAllAsync(AnomalySeverity? minSeverity = null);
}