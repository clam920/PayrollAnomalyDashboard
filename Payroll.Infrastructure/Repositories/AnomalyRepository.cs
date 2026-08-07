using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;
using Payroll.Infrastructure.Data;

namespace Payroll.Infrastructure.Repositories;

public class AnomalyRepository : IAnomalyRepository
{
    private readonly AppDbContext _context;

    public AnomalyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddRangeAsync(IEnumerable<Anomaly> anomalies)
    {
        await _context.Anomalies.AddRangeAsync(anomalies);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<Anomaly>> GetAllAsync(AnomalySeverity? minSeverity = null)
    {
        var query = _context.Anomalies.AsQueryable();

        // AnomalySeverity is declared Info < Warning < Critical, so ">="
        // here really does mean "at least this urgent".
        if (minSeverity is not null)
            query = query.Where(a => a.Severity >= minSeverity);

        return await query
            .OrderByDescending(a => a.DetectedAtUtc)
            .ToListAsync();
    }
}