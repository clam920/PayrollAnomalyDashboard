using System.Collections.Generic;
using System.Threading.Tasks;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
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
}