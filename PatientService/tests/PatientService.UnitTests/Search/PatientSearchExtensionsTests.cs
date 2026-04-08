using PatientService.Core.Entities;
using PatientService.Core.Enums;
using PatientService.Core.Search;
using PatientService.Core.ValueObjects;
using Xunit;

namespace PatientService.UnitTests.Search;

public sealed class PatientSearchExtensionsTests
{
    [Fact]
    public void ApplyBirthDateSearch_EqDay_ShouldReturnOnlyMatchingDay()
    {
        var patients = new List<Patient>
        {
            CreatePatient(new DateTimeOffset(2024, 1, 13, 10, 0, 0, TimeSpan.Zero)),
            CreatePatient(new DateTimeOffset(2024, 1, 14, 10, 0, 0, TimeSpan.Zero))
        }.AsQueryable();

        BirthDateSearchParser.TryParse("2024-01-13", out var criteria, out _);

        var result = patients
            .ApplyBirthDateSearch(criteria!)
            .ToList();

        Assert.Single(result);
        Assert.Equal(new DateTimeOffset(2024, 1, 13, 10, 0, 0, TimeSpan.Zero), result[0].BirthDate);
    }

    [Fact]
    public void ApplyBirthDateSearch_LtMonth_ShouldReturnOnlyEarlierPatients()
    {
        var patients = new List<Patient>
        {
            CreatePatient(new DateTimeOffset(2023, 12, 31, 23, 59, 59, TimeSpan.Zero)),
            CreatePatient(new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero))
        }.AsQueryable();

        BirthDateSearchParser.TryParse("lt2024-01", out var criteria, out _);

        var result = patients
            .ApplyBirthDateSearch(criteria!)
            .ToList();

        Assert.Single(result);
        Assert.Equal(new DateTimeOffset(2023, 12, 31, 23, 59, 59, TimeSpan.Zero), result[0].BirthDate);
    }

    private static Patient CreatePatient(DateTimeOffset birthDate)
    {
        var name = new HumanName(
            Guid.NewGuid(),
            "official",
            "Ivanov",
            new[] { "Ivan" });

        return new Patient(true, name, Gender.Male, birthDate);
    }
}