using PatientService.Core.Entities;
using PatientService.Core.Enums;
using PatientService.Core.Search;
using PatientService.Core.ValueObjects;

namespace PatientService.Core.Services;

public interface IPatientService
{
    Task<Patient> CreateAsync(Patient patient, CancellationToken cancellationToken);
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Patient?> UpdateAsync(Guid id, bool active, HumanName name, Gender gender, DateTimeOffset birthDate, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Patient>> SearchAsync(BirthDateSearchCriteria? criteria, CancellationToken cancellationToken);
}