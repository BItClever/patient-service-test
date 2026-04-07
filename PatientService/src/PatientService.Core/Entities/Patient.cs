using PatientService.Core.Enums;
using PatientService.Core.ValueObjects;

namespace PatientService.Core.Entities;

public sealed class Patient
{
    public Guid Id { get; private set; }
    public bool Active { get; private set; }
    public HumanName Name { get; private set; } = null!;
    public Gender Gender { get; private set; }
    public DateTimeOffset BirthDate { get; private set; }

    private Patient()
    {
    }

    public Patient(bool active, HumanName name, Gender gender, DateTimeOffset birthDate)
    {
        Id = Guid.NewGuid();
        SetValues(active, name, gender, birthDate);
    }

    public void Update(bool active, HumanName name, Gender gender, DateTimeOffset birthDate)
    {
        SetValues(active, name, gender, birthDate);
    }

    private void SetValues(bool active, HumanName name, Gender gender, DateTimeOffset birthDate)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (birthDate > DateTimeOffset.UtcNow)
        {
            throw new ArgumentOutOfRangeException(nameof(birthDate), "Birth date cannot be in the future.");
        }

        Active = active;
        Name = name;
        Gender = gender;
        BirthDate = birthDate;
    }
}
