FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["src/PatientService.Seeder/PatientService.Seeder.csproj", "src/PatientService.Seeder/"]
RUN dotnet restore "src/PatientService.Seeder/PatientService.Seeder.csproj"

COPY . .
WORKDIR "/src/src/PatientService.Seeder"
RUN dotnet publish "PatientService.Seeder.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PatientService.Seeder.dll"]