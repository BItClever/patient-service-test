using PatientService.Seeder.Contracts;

namespace PatientService.Seeder.Generation;

public static class PatientGenerator
{
    private static readonly string[] MaleFirstNames =
    {
        "Иван", "Алексей", "Дмитрий", "Максим", "Павел",
        "Никита", "Артём", "Егор", "Михаил", "Кирилл"
    };

    private static readonly string[] FemaleFirstNames =
    {
        "Анна", "Мария", "Екатерина", "София", "Дарья",
        "Алиса", "Ева", "Полина", "Виктория", "Ксения"
    };

    private static readonly string[] MalePatronymics =
    {
        "Иванович", "Алексеевич", "Дмитриевич", "Максимович", "Павлович",
        "Никитич", "Артёмович", "Егорович", "Михайлович", "Кириллович"
    };

    private static readonly string[] FemalePatronymics =
    {
        "Ивановна", "Алексеевна", "Дмитриевна", "Максимовна", "Павловна",
        "Никитична", "Артёмовна", "Егоровна", "Михайловна", "Кирилловна"
    };

    private static readonly SurnamePair[] Surnames =
    {
        new("Иванов", "Иванова"),
        new("Петров", "Петрова"),
        new("Сидоров", "Сидорова"),
        new("Козлов", "Козлова"),
        new("Смирнов", "Смирнова"),
        new("Васильев", "Васильева"),
        new("Морозов", "Морозова"),
        new("Новиков", "Новикова"),
        new("Фёдоров", "Фёдорова"),
        new("Романов", "Романова")
    };

    private static readonly string[] UnknownOrOtherNames =
    {
        "Саша", "Женя", "Валера", "Лера", "Миша",
        "Ника", "Даня", "Арина", "Тимур", "Марк"
    };

    public static IReadOnlyList<CreatePatientRequest> CreateMany(int count)
    {
        if (count <= 0)
        {
            return Array.Empty<CreatePatientRequest>();
        }

        var genderPlan = BuildGenderPlan(count);
        Shuffle(genderPlan);

        return genderPlan
            .Select(CreateForGender)
            .ToList();
    }

    private static List<string> BuildGenderPlan(int count)
    {
        var maleCount = count * 45 / 100;
        var femaleCount = count * 45 / 100;
        var otherCount = count * 5 / 100;
        var unknownCount = count * 5 / 100;

        var assigned = maleCount + femaleCount + otherCount + unknownCount;
        var remaining = count - assigned;

        for (var i = 0; i < remaining; i++)
        {
            if (i % 2 == 0)
            {
                maleCount++;
            }
            else
            {
                femaleCount++;
            }
        }

        var result = new List<string>(count);

        result.AddRange(Enumerable.Repeat("male", maleCount));
        result.AddRange(Enumerable.Repeat("female", femaleCount));
        result.AddRange(Enumerable.Repeat("other", otherCount));
        result.AddRange(Enumerable.Repeat("unknown", unknownCount));

        return result;
    }

    private static CreatePatientRequest CreateForGender(string gender)
    {
        return gender switch
        {
            "male" => CreateMalePatient(),
            "female" => CreateFemalePatient(),
            "other" => CreateOtherPatient(),
            _ => CreateUnknownPatient()
        };
    }

    private static CreatePatientRequest CreateMalePatient()
    {
        var surname = Pick(Surnames);
        var firstName = Pick(MaleFirstNames);
        var patronymic = Pick(MalePatronymics);

        return new CreatePatientRequest
        {
            Name = new HumanNameDto
            {
                Id = Guid.NewGuid(),
                Use = "official",
                Family = surname.Male,
                Given = new List<string> { firstName, patronymic }
            },
            Gender = "male",
            BirthDate = GenerateBirthDate(),
            Active = GenerateActive()
        };
    }

    private static CreatePatientRequest CreateFemalePatient()
    {
        var surname = Pick(Surnames);
        var firstName = Pick(FemaleFirstNames);
        var patronymic = Pick(FemalePatronymics);

        return new CreatePatientRequest
        {
            Name = new HumanNameDto
            {
                Id = Guid.NewGuid(),
                Use = "official",
                Family = surname.Female,
                Given = new List<string> { firstName, patronymic }
            },
            Gender = "female",
            BirthDate = GenerateBirthDate(),
            Active = GenerateActive()
        };
    }

    private static CreatePatientRequest CreateOtherPatient()
    {
        var surname = Pick(Surnames);
        var useFemaleSurname = Random.Shared.Next(0, 2) == 0;

        return new CreatePatientRequest
        {
            Name = new HumanNameDto
            {
                Id = Guid.NewGuid(),
                Use = "official",
                Family = useFemaleSurname ? surname.Female : surname.Male,
                Given = new List<string> { Pick(UnknownOrOtherNames) }
            },
            Gender = "other",
            BirthDate = GenerateBirthDate(),
            Active = GenerateActive()
        };
    }

    private static CreatePatientRequest CreateUnknownPatient()
    {
        var surname = Pick(Surnames);
        var useFemaleSurname = Random.Shared.Next(0, 2) == 0;

        return new CreatePatientRequest
        {
            Name = new HumanNameDto
            {
                Id = Guid.NewGuid(),
                Use = "official",
                Family = useFemaleSurname ? surname.Female : surname.Male,
                Given = new List<string> { Pick(UnknownOrOtherNames) }
            },
            Gender = "unknown",
            BirthDate = GenerateBirthDate(),
            Active = GenerateActive()
        };
    }

    private static DateTimeOffset GenerateBirthDate()
    {
        var daysAgo = Random.Shared.Next(0, 60);
        var hours = Random.Shared.Next(0, 24);
        var minutes = Random.Shared.Next(0, 60);
        var seconds = Random.Shared.Next(0, 60);

        return DateTimeOffset.UtcNow
            .AddDays(-daysAgo)
            .AddHours(-hours)
            .AddMinutes(-minutes)
            .AddSeconds(-seconds);
    }

    private static bool GenerateActive()
    {
        return Random.Shared.Next(0, 10) != 0;
    }

    private static T Pick<T>(IReadOnlyList<T> values)
    {
        return values[Random.Shared.Next(values.Count)];
    }

    private static void Shuffle<T>(IList<T> values)
    {
        for (var i = values.Count - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }
    }

    private sealed record SurnamePair(string Male, string Female);
}