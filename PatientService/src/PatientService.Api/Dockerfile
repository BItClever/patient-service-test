FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["PatientService.sln", "./"]
COPY ["src/PatientService.Api/PatientService.Api.csproj", "src/PatientService.Api/"]
COPY ["src/PatientService.Core/PatientService.Core.csproj", "src/PatientService.Core/"]
COPY ["src/PatientService.Persistence/PatientService.Persistence.csproj", "src/PatientService.Persistence/"]

RUN dotnet restore "src/PatientService.Api/PatientService.Api.csproj"

COPY . .
WORKDIR "/src/src/PatientService.Api"
RUN dotnet publish "PatientService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PatientService.Api.dll"]