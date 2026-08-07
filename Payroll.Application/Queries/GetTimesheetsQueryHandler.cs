using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Payroll.Application.Interfaces;

namespace Payroll.Application.Queries;

public class GetTimesheetsQueryHandler : IRequestHandler<GetTimesheetsQuery, List<TimesheetDto>>
{
    private readonly ITimesheetRepository _timesheetRepository;

    public GetTimesheetsQueryHandler(ITimesheetRepository timesheetRepository)
    {
        _timesheetRepository = timesheetRepository;
    }

    public async Task<List<TimesheetDto>> Handle(GetTimesheetsQuery request, CancellationToken cancellationToken)
    {
        var timesheets = await _timesheetRepository.GetAllAsync(request.EmployeeId);

        return timesheets
            .Select(t => new TimesheetDto(t.Id, t.EmployeeId, t.WorkDate, t.HoursWorked, t.Status.ToString()))
            .ToList();
    }
}