# Patient Service Test

Test assignment for implementing a .NET 6 REST API for the `Patient` entity.

## Implemented Features

- CRUD API for `Patient`
- FHIR-like search by `birthDate`
- Swagger / OpenAPI documentation
- PostgreSQL persistence via Entity Framework Core
- Console seeder that creates 100 patients through API
- Docker / Docker Compose setup
- Postman requests and environment

## Tech Stack

- .NET 6
- ASP.NET Core Web API
- Entity Framework Core 6
- PostgreSQL
- xUnit
- Docker / Docker Compose
- Swagger / Swashbuckle

## Repository Structure

```
patient-service-test/
├─ README.md
├─ .gitignore
├─ postman/
│  ├─ collections/
│  └─ environments/
└─ PatientService/
   ├─ PatientService.sln
   ├─ docker-compose.yml
   ├─ .dockerignore
   ├─ src/
   │  ├─ PatientService.Api/
   │  ├─ PatientService.Core/
   │  ├─ PatientService.Persistence/
   │  └─ PatientService.Seeder/
   └─ tests/
      └─ PatientService.UnitTests/
      └─ PatientService.IntegrationTests/
```

## API Endpoints

### Patient CRUD

- `POST /api/patients` — create patient
- `GET /api/patients/{id}` — get patient by id
- `PUT /api/patients/{id}` — update patient
- `DELETE /api/patients/{id}` — delete patient

### Search

- `GET /api/patients?birthDate=...` — search by birthDate

### Example Payload

```json
{
  "name": {
    "id": "d8ff176f-bd0a-4b8e-b329-871952e32e1f",
    "use": "official",
    "family": "Иванов",
    "given": [
      "Иван",
      "Иванович"
    ]
  },
  "gender": "male",
  "birthDate": "2024-01-13T18:25:43Z",
  "active": true
}
```

## birthDate Search

FHIR-like search semantics are implemented for the `birthDate` query parameter.

Supported prefixes:

- `eq` (default)
- `ne`
- `gt`
- `ge`
- `lt`
- `le`
- `sa`
- `eb`
- `ap`

Supported formats:

- `yyyy`
- `yyyy-MM`
- `yyyy-MM-dd`
- `yyyy-MM-ddTHH`
- `yyyy-MM-ddTHH:mm`
- `yyyy-MM-ddTHH:mm:ss`
- `yyyy-MM-ddTHH:mm:ss.fffZ` and fractional seconds up to 7 digits

Examples:

- `GET /api/patients?birthDate=2024`
- `GET /api/patients?birthDate=2024-01`
- `GET /api/patients?birthDate=2024-01-13`
- `GET /api/patients?birthDate=ge2024-01-01`
- `GET /api/patients?birthDate=lt2024-02-01`
- `GET /api/patients?birthDate=ne2024-01-13`

## Design Decisions

- **Resource identifier**: `Patient.Id` is used as the primary resource identifier for CRUD operations.
- **Preserved `name.id`**: The nested `name.id` field remains present to stay compatible with the assignment's example payload.
- **birthDate type**: `birthDate` is stored as `DateTimeOffset` to preserve date-time precision and support FHIR-like search semantics.
- **given field**: `given` is stored in PostgreSQL as `jsonb` because relational search by individual name parts is not required.
- **HumanName mapping**: `HumanName` is mapped as an owned type within the `patients` table.

## Running Locally

### Prerequisites

- .NET 6 SDK
- PostgreSQL
- Docker (optional)

### Solution Location

The solution file is located at:

```
PatientService/PatientService.sln
```

### Configure Database

Connection string is configured in:

```
PatientService/src/PatientService.Api/appsettings.json
```

Default value:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=patient_service_db;Username=postgres;Password=postgres"
}
```

### Run API

From the `PatientService` directory:

```bash
dotnet run --project src/PatientService.Api
```

Swagger is available at the URL printed by ASP.NET Core on startup, for example:

```
http://localhost:5000/swagger
```

## Running with Docker

From the `PatientService` directory:

### Start database and API

```bash
docker compose up --build -d db api
```

Swagger is available at:

```
http://localhost:8080/swagger
```

### Run seeder

```bash
docker compose --profile seed up --build seeder
```

### Stop containers

```bash
docker compose down
```

### Remove containers and volumes

```bash
docker compose down -v
```

## Seeder

The console seeder creates patient records and sends them to the API over HTTP.

Default settings:

- API base URL: `http://localhost:8080`
- number of generated patients: `100`

Supported configuration:

- command-line arguments
- environment variables:
  - `PATIENT_API_BASE_URL`
  - `PATIENT_SEED_COUNT`

## Tests

From the `PatientService` directory:

```bash
dotnet test
```

## Postman

Postman artifacts are located in:

```
postman/
```

They include:

- request collection in Postman local YAML format
- local environment file

The collection demonstrates:

- patient creation
- update
- get by id
- delete
- multiple birthDate search scenarios
