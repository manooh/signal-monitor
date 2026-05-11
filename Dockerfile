FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/SignalMonitor.Api/SignalMonitor.Api.csproj src/SignalMonitor.Api/
RUN dotnet restore src/SignalMonitor.Api/SignalMonitor.Api.csproj

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
