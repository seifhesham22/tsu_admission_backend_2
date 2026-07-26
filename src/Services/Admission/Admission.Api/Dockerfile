FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY src/ ./src/

RUN dotnet restore "src/Services/Admission/Admission.Api/Admission.Api.csproj"
RUN dotnet publish "src/Services/Admission/Admission.Api/Admission.Api.csproj"     -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_EnableDiagnostics=0

RUN adduser --disabled-password --gecos "" --uid 10001 appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "Admission.Api.dll"]
