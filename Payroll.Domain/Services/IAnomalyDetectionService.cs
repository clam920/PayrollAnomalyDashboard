using System.Collections.Generic;
using Payroll.Domain.Entities;

namespace Payroll.Domain.Services;

// A "domain service" - business logic that doesn't naturally belong to a
// single entity (it needs both the Employee and the Timesheet, plus some
// history, to make a decision). Living in the Domain layer means it stays
// pure and unit-testable with no database or MediatR involved: you hand it
// plain objects and get plain objects back.
public interface IAnomalyDetectionService
{
    // recentTimesheets lets rules compare the new timesheet against the
    // employee's recent history (e.g. duplicate dates). The caller
    // (Application layer, via a repository) is responsible for loading that
    // history - this service just reasons over what it's given.
    IReadOnlyList<Anomaly> Detect(Employee employee, Timesheet timesheet, IEnumerable<Timesheet> recentTimesheets);
}