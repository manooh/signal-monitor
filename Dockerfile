FROM mcr.microsoft.com/dotnet/sdk:8.0 AS app-restore
WORKDIR /src

COPY src/SignalMonitor.Api/SignalMonitor.Api.csproj src/SignalMonitor.Api/
RUN dotnet restore src/SignalMonitor.Api/SignalMonitor.Api.csproj

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS test-restore
WORKDIR /src

COPY signal-monitor.sln .
COPY src/SignalMonitor.Api/SignalMonitor.Api.csproj src/SignalMonitor.Api/
COPY tests/SignalMonitor.Api.Tests/SignalMonitor.Api.Tests.csproj tests/SignalMonitor.Api.Tests/
RUN dotnet restore signal-monitor.sln

FROM test-restore AS test
COPY src/SignalMonitor.Api/ src/SignalMonitor.Api/
COPY tests/SignalMonitor.Api.Tests/ tests/SignalMonitor.Api.Tests/
ENTRYPOINT ["dotnet", "test", "signal-monitor.sln", "--configuration", "Release", "--no-restore"]

FROM app-restore AS build
COPY src/SignalMonitor.Api/ src/SignalMonitor.Api/
RUN dotnet publish src/SignalMonitor.Api/SignalMonitor.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SignalMonitor.Api.dll"]
