namespace PatientService.Core.Search;

public sealed record BirthDateSearchCriteria(
    BirthDateSearchPrefix Prefix,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtcExclusive,
    string RawValue)
{
    public (DateTimeOffset StartUtc, DateTimeOffset EndUtcExclusive) GetApproximateRange()
    {
        var baseDuration = EndUtcExclusive - StartUtc;
        var toleranceTicks = Math.Max((long)(baseDuration.Ticks * 0.1), TimeSpan.TicksPerSecond);
        var tolerance = TimeSpan.FromTicks(toleranceTicks);

        return (StartUtc - tolerance, EndUtcExclusive + tolerance);
    }
}