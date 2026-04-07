using PatientService.Core.Entities;

namespace PatientService.Core.Search;

public static class PatientSearchExtensions
{
    public static IQueryable<Patient> ApplyBirthDateSearch(
        this IQueryable<Patient> query,
        BirthDateSearchCriteria criteria)
    {
        var start = criteria.StartUtc;
        var end = criteria.EndUtcExclusive;

        return criteria.Prefix switch
        {
            BirthDateSearchPrefix.Eq => query.Where(x => x.BirthDate >= start && x.BirthDate < end),
            BirthDateSearchPrefix.Ne => query.Where(x => x.BirthDate < start || x.BirthDate >= end),
            BirthDateSearchPrefix.Gt => query.Where(x => x.BirthDate >= end),
            BirthDateSearchPrefix.Ge => query.Where(x => x.BirthDate >= start),
            BirthDateSearchPrefix.Lt => query.Where(x => x.BirthDate < start),
            BirthDateSearchPrefix.Le => query.Where(x => x.BirthDate < end),
            BirthDateSearchPrefix.Sa => query.Where(x => x.BirthDate >= end),
            BirthDateSearchPrefix.Eb => query.Where(x => x.BirthDate < start),
            BirthDateSearchPrefix.Ap => ApplyApproximate(query, criteria),
            _ => query
        };
    }

    private static IQueryable<Patient> ApplyApproximate(
        IQueryable<Patient> query,
        BirthDateSearchCriteria criteria)
    {
        var (approximateStart, approximateEnd) = criteria.GetApproximateRange();

        return query.Where(x => x.BirthDate >= approximateStart && x.BirthDate < approximateEnd);
    }
}