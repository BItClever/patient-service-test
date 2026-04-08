using PatientService.Core.Entities;
using PatientService.Core.Enums;
using PatientService.Core.ValueObjects;
using Xunit;

namespace PatientService.UnitTests.Domain;

public sealed class PatientTests
{
    [Fact]
    public void Constructor_WhenBirthDateIsInFuture_ShouldThrow()
    {
        var name = CreateName();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Patient(true, name, Gender.Male, DateTimeOffset.UtcNow.AddDays(1)));
    }

    [Fact]
    public void Constructor_WhenNameIsNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Patient(true, null!, Gender.Male, DateTimeOffset.UtcNow.AddDays(-1)));
    }

    [Fact]
    public void Constructor_ShouldGenerateNonEmptyId()
    {
        var patient = CreatePatient();
        Assert.NotEqual(Guid.Empty, patient.Id);
    }

    [Fact]
    public void Constructor_ShouldSetAllPropertiesCorrectly()
    {
        var name = CreateName();
        var birthDate = DateTimeOffset.UtcNow.AddDays(-5);

        var patient = new Patient(true, name, Gender.Female, birthDate);

        Assert.True(patient.Active);
        Assert.Equal(name, patient.Name);
        Assert.Equal(Gender.Female, patient.Gender);
        Assert.Equal(birthDate, patient.BirthDate);
    }

    [Fact]
    public void Update_WhenBirthDateIsInFuture_ShouldThrow()
    {
        var patient = CreatePatient();
        var name = CreateName();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            patient.Update(true, name, Gender.Male, DateTimeOffset.UtcNow.AddDays(1)));
    }

    [Fact]
    public void Update_WhenCalledWithValidData_ShouldUpdateAllProperties()
    {
        var patient = CreatePatient();
        var newName = new HumanName(Guid.NewGuid(), "nickname", "Петров", new[] { "Пётр" });
        var newBirthDate = DateTimeOffset.UtcNow.AddDays(-10);

        patient.Update(false, newName, Gender.Female, newBirthDate);

        Assert.False(patient.Active);
        Assert.Equal("Петров", patient.Name.Family);
        Assert.Equal(Gender.Female, patient.Gender);
        Assert.Equal(newBirthDate, patient.BirthDate);
    }

    [Fact]
    public void TwoPatients_ShouldHaveDifferentIds()
    {
        var p1 = CreatePatient();
        var p2 = CreatePatient();
        Assert.NotEqual(p1.Id, p2.Id);
    }

    private static HumanName CreateName() =>
        new HumanName(Guid.NewGuid(), "official", "Иванов", new[] { "Иван", "Иванович" });

    private static Patient CreatePatient() =>
        new Patient(true, CreateName(), Gender.Male, DateTimeOffset.UtcNow.AddDays(-1));
}