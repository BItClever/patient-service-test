using Xunit;

namespace PatientService.IntegrationTests.Infrastructure;

[CollectionDefinition("PatientApi")]
public sealed class PatientApiCollection : ICollectionFixture<PatientApiFactory>
{
}
