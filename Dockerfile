FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Trackr.Api/Trackr.Api.csproj src/Trackr.Api/
RUN dotnet restore src/Trackr.Api/Trackr.Api.csproj

COPY src/Trackr.Api/ src/Trackr.Api/
RUN dotnet publish src/Trackr.Api/Trackr.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Trackr.Api.dll"]
