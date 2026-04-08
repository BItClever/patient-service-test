using PatientService.Core.Search;
using Xunit;

namespace PatientService.UnitTests.Search;

public sealed class BirthDateSearchParserTests
{
    [Fact]
    public void TryParse_YearValue_ShouldCreateYearRange()
    {
        var result = BirthDateSearchParser.TryParse("2024", out var criteria, out var error);

        Assert.True(result);
        Assert.NotNull(criteria);
        Assert.Equal(BirthDateSearchPrefix.Eq, criteria!.Prefix);
        Assert.Equal(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), criteria.StartUtc);
        Assert.Equal(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), criteria.EndUtcExclusive);
        Assert.Equal(string.Empty, error);
    }

    [Fact]
    public void TryParse_MonthValueWithPrefix_ShouldCreateMonthRange()
    {
        var result = BirthDateSearchParser.TryParse("ge2024-01", out var criteria, out var error);

        Assert.True(result);
        Assert.NotNull(criteria);
        Assert.Equal(BirthDateSearchPrefix.Ge, criteria!.Prefix);
        Assert.Equal(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), criteria.StartUtc);
        Assert.Equal(new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero), criteria.EndUtcExclusive);
        Assert.Equal(string.Empty, error);
    }

    [Fact]
    public void TryParse_DateTimeValue_ShouldCreateSecondPrecisionRange()
    {
        var result = BirthDateSearchParser.TryParse("2024-01-13T18:25:43Z", out var criteria, out var error);

        Assert.True(result);
        Assert.NotNull(criteria);
        Assert.Equal(new DateTimeOffset(2024, 1, 13, 18, 25, 43, TimeSpan.Zero), criteria!.StartUtc);
        Assert.Equal(new DateTimeOffset(2024, 1, 13, 18, 25, 44, TimeSpan.Zero), criteria.EndUtcExclusive);
        Assert.Equal(string.Empty, error);
    }

    [Fact]
    public void TryParse_InvalidPrefix_ShouldFail()
    {
        var result = BirthDateSearchParser.TryParse("xx2024-01-01", out var criteria, out var error);

        Assert.False(result);
        Assert.Null(criteria);
        Assert.Contains("Unsupported birthDate prefix", error);
    }
}