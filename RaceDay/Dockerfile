# Build and publish the API, plus a self-contained EF Core migration bundle.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY RaceDay/RaceDay.csproj RaceDay/
RUN dotnet restore RaceDay/RaceDay.csproj

COPY . .
RUN dotnet publish RaceDay/RaceDay.csproj -c Release -o /app/publish --no-restore

# Build migrations into a small executable so Compose can migrate SQL Server
# before starting the API. The bundle reads ConnectionStrings__RaceDayConnection
# from its environment at runtime.
RUN dotnet tool install --tool-path /tools dotnet-ef --version 10.0.13
RUN /tools/dotnet-ef migrations bundle --self-contained --target-runtime linux-x64 --output /app/efbundle --project RaceDay/RaceDay.csproj

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
COPY --from=build /app/efbundle ./efbundle
COPY --from=build /src/RaceDay/appsettings.json ./appsettings.json
RUN chmod +x ./efbundle

HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=5 \
    CMD curl --fail --silent http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "RaceDay.dll"]
