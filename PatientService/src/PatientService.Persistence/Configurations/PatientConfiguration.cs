using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientService.Core.Entities;
using System.Text.Json;

namespace PatientService.Persistence.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("patients");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Active)
            .HasColumnName("active")
            .IsRequired();

        builder.Property(x => x.Gender)
            .HasColumnName("gender")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.BirthDate)
            .HasColumnName("birth_date")
            .IsRequired();

        builder.OwnsOne(x => x.Name, nameBuilder =>
        {
            nameBuilder.Property(x => x.Id)
                .HasColumnName("name_id")
                .IsRequired();

            nameBuilder.Property(x => x.Use)
                .HasColumnName("name_use")
                .HasMaxLength(100);

            nameBuilder.Property(x => x.Family)
                .HasColumnName("name_family")
                .HasMaxLength(200)
                .IsRequired();

            nameBuilder.Property(x => x.Given)
                .HasColumnName("name_given")
                .HasColumnType("jsonb")
                .HasConversion(
                    value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                    value => JsonSerializer.Deserialize<List<string>>(value, (JsonSerializerOptions?)null) ?? new List<string>())
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (left, right) => left != null && right != null && left.SequenceEqual(right),
                    value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
                    value => value.ToList()));

            nameBuilder.WithOwner();
        });
    }
}