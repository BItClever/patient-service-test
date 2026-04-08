using Microsoft.EntityFrameworkCore;
using PatientService.Core.Entities;
using PatientService.Core.Repositories;
using PatientService.Core.Search;
using PatientService.Persistence.Contexts;

namespace PatientService.Persistence.Repositories;

public sealed class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _dbContext;

    public PatientRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Patients
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Patient>> SearchAsync(
        BirthDateSearchCriteria? criteria,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Patients
            .AsNoTracking()
            .AsQueryable();

        if (criteria is not null)
        {
            query = query.ApplyBirthDateSearch(criteria);
        }

        return await query
            .OrderBy(x => x.BirthDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Patient> Items, int TotalCount)> SearchPagedAsync(
        BirthDateSearchCriteria? criteria,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Patients
            .AsNoTracking()
            .AsQueryable();

        if (criteria is not null)
        {
            query = query.ApplyBirthDateSearch(criteria);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.BirthDate)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task AddAsync(Patient patient, CancellationToken cancellationToken)
    {
        return _dbContext.Patients.AddAsync(patient, cancellationToken).AsTask();
    }

    public void Remove(Patient patient)
    {
        _dbContext.Patients.Remove(patient);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}