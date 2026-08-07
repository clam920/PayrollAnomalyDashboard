using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Payroll.Application.Interfaces;

namespace Payroll.Application.Queries;

public class GetAnomaliesQueryHandler : IRequestHandler<GetAnomaliesQuery, List<AnomalyDto>>
{
    private readonly IAnomalyRepository _anomalyRepository;

    public GetAnomaliesQueryHandler(IAnomalyRepository anomalyRepository)
    {
        _anomalyRepository = anomalyRepository;
    }

    public async Task<List<AnomalyDto>> Handle(GetAnomaliesQuery request, CancellationToken cancellationToken)
    {
        var anomalies = await _anomalyRepository.GetAllAsync(request.MinSeverity);

        return anomalies
            .Select(a => new AnomalyDto(
                a.Id,
                a.TimesheetId,
                a.EmployeeId,
                a.Type.ToString(),
                a.Severity.ToString(),
                a.Description,
                a.DetectedAtUtc))
            .ToList();
    }
}