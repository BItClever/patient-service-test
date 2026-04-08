using PatientService.Core.Entities;
using PatientService.Core.Search;

namespace PatientService.Core.Repositories;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Patient>> SearchAsync(BirthDateSearchCriteria? criteria, CancellationToken cancellationToken);
    Task AddAsync(Patient patient, CancellationToken cancellationToken);
    void Remove(Patient patient);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}