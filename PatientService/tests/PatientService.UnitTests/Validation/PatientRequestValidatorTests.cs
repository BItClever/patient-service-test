using PatientService.Api.Contracts.Patients;
using PatientService.Api.Validation;
using Xunit;

namespace PatientService.UnitTests.Validation;

public sealed class PatientRequestValidatorTests
{
    [Fact]
    public void Validate_WhenRequestIsValid_ShouldReturnNoErrors()
    {
        var request = CreateValidRequest();
        var errors = PatientRequestValidator.Validate(request);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_WhenFamilyIsEmpty_ShouldReturnFamilyError()
    {
        var request = new CreatePatientRequest
        {
            Active = true,
            Name = new HumanNameDto { Family = "" },
            Gender = "male",
            BirthDate = DateTimeOffset.UtcNow.AddDays(-1)
        };

        var errors = PatientRequestValidator.Validate(request);

        Assert.True(errors.ContainsKey("name.family"));
    }

    [Fact]
    public void Validate_WhenBirthDateIsDefault_ShouldReturnBirthDateError()
    {
        var request = new CreatePatientRequest
        {
            Active = true,
            Name = new HumanNameDto { Family = "Иванов" },
            Gender = "male",
            BirthDate = default
        };

        var errors = PatientRequestValidator.Validate(request);

        Assert.True(errors.ContainsKey("birthDate"));
    }

    [Fact]
    public void Validate_WhenBirthDateIsInFuture_ShouldReturnBirthDateError()
    {
        var request = new CreatePatientRequest
        {
            Active = true,
            Name = new HumanNameDto { Family = "Иванов" },
            Gender = "male",
            BirthDate = DateTimeOffset.UtcNow.AddDays(1)
        };

        var errors = PatientRequestValidator.Validate(request);

        Assert.True(errors.ContainsKey("birthDate"));
    }

    [Fact]
    public void Validate_WhenGenderIsInvalid_ShouldReturnGenderError()
    {
        var request = new CreatePatientRequest
        {
            Active = true,
            Name = new HumanNameDto { Family = "Иванов" },
            Gender = "invalid_gender",
            BirthDate = DateTimeOffset.UtcNow.AddDays(-1)
        };

        var errors = PatientRequestValidator.Validate(request);

        Assert.True(errors.ContainsKey("gender"));
    }

    [Fact]
    public void Validate_WhenGenderIsEmpty_ShouldReturnGenderError()
    {
        var request = new CreatePatientRequest
        {
            Active = true,
            Name = new HumanNameDto { Family = "Иванов" },
            Gender = "",
            BirthDate = DateTimeOffset.UtcNow.AddDays(-1)
        };

        var errors = PatientRequestValidator.Validate(request);

        Assert.True(errors.ContainsKey("gender"));
    }

    [Theory]
    [InlineData("male")]
    [InlineData("female")]
    [InlineData("other")]
    [InlineData("unknown")]
    public void Validate_WhenGenderIsValid_ShouldNotReturnGenderError(string gender)
    {
        var request = new CreatePatientRequest
        {
            Active = true,
            Name = new HumanNameDto { Family = "Иванов" },
            Gender = gender,
            BirthDate = DateTimeOffset.UtcNow.AddDays(-1)
        };

        var errors = PatientRequestValidator.Validate(request);

        Assert.False(errors.ContainsKey("gender"));
    }

    [Fact]
    public void Validate_WhenMultipleFieldsInvalid_ShouldReturnMultipleErrors()
    {
        var request = new CreatePatientRequest
        {
            Active = true,
            Name = new HumanNameDto { Family = "" },
            Gender = "",
            BirthDate = default
        };

        var errors = PatientRequestValidator.Validate(request);

        Assert.True(errors.ContainsKey("name.family"));
        Assert.True(errors.ContainsKey("gender"));
        Assert.True(errors.ContainsKey("birthDate"));
    }

    private static CreatePatientRequest CreateValidRequest() =>
        new CreatePatientRequest
        {
            Active = true,
            Name = new HumanNameDto
            {
                Id = Guid.NewGuid(),
                Use = "official",
                Family = "Иванов",
                Given = new List<string> { "Иван", "Иванович" }
            },
            Gender = "male",
            BirthDate = DateTimeOffset.UtcNow.AddDays(-1)
        };
}