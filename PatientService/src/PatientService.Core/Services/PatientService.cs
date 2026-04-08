using PatientService.Core.Entities;
using PatientService.Core.Enums;
using PatientService.Core.Repositories;
using PatientService.Core.Search;
using PatientService.Core.ValueObjects;

namespace PatientService.Core.Services;

public sealed class PatientService : IPatientService
{
    private readonly IPatientRepository _repository;

    public PatientService(IPatientRepository repository)
    {
        _repository = repository;
    }

    public async Task<Patient> CreateAsync(Patient patient, CancellationToken cancellationToken)
    {
        await _repository.AddAsync(patient, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return patient;
    }

    public Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Patient?> UpdateAsync(
        Guid id,
        bool active,
        HumanName name,
        Gender gender,
        DateTimeOffset birthDate,
        CancellationToken cancellationToken)
    {
        var patient = await _repository.GetByIdAsync(id, cancellationToken);
        if (patient is null)
        {
            return null;
        }

        patient.Update(active, name, gender, birthDate);
        await _repository.SaveChangesAsync(cancellationToken);

        return patient;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var patient = await _repository.GetByIdAsync(id, cancellationToken);
        if (patient is null)
        {
            return false;
        }

        _repository.Remove(patient);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<PagedResult<Patient>> SearchAsync(BirthDateSearchCriteria? criteria, int page, int pageSize, CancellationToken cancellationToken)
    {
        var skip = (page - 1) * pageSize;
        var (items, totalCount) = await _repository.SearchPagedAsync(criteria, skip, pageSize, cancellationToken);
        return new PagedResult<Patient>(items, totalCount, page, pageSize);
    }
}