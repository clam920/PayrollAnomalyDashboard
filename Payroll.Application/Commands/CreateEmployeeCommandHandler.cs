using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
namespace Payroll.Application.Commands;
public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Guid>
{
    private readonly IEmployeeRepository _repository;
    public CreateEmployeeCommandHandler(IEmployeeRepository repository)
    {
        _repository = repository;
    }
    public async Task<Guid> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = new Employee(request.FirstName, request.LastName, request.HourlyRate);
        await _repository.AddAsync(employee);
        await _repository.SaveChangesAsync();
        return employee.Id;
    }
}