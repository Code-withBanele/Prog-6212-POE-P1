# RaceDay — Part 3 (ASP.NET Core MVC)

RaceDay is an event-management application built with .NET 10. The MVC web application consumes the RaceDay REST API; the API owns SQL Server access, JWT validation and Azure Blob Storage operations.

## Architecture

- **RaceDay.Web** — MVC frontend, cookie authentication and server-side session.
- **RaceDay** — REST API, JWT authentication, EF Core and Azure Blob Storage integration.
- **SQL Server 2022** — persistent relational database.
- **Docker Compose** — starts the database, applies EF Core migrations, then starts the API and MVC frontend.

The MVC container reaches the API over the private Compose network at `http://raceday-api:8080`. The browser reaches MVC at `http://localhost:8080`. The API is also published on `http://localhost:8081` for local API testing.

## Run with Docker (recommended)

### Requirements

- Docker Desktop with the Docker Compose v2 plugin.
- At least 4 GB RAM available to Docker (SQL Server needs meaningful memory).
- A strong SQL Server password and a JWT signing secret.

### 1. Get the Part 3 branch

```powershell
git clone https://github.com/Code-withBanele/Prog-6212-POE-P1.git
cd Prog-6212-POE-P1
git switch part-3-mvc
```

If the repository is already cloned, switch to and pull the branch:

```powershell
git switch part-3-mvc
git pull origin part-3-mvc
```

### 2. Create your local environment file

```powershell
Copy-Item .env.example .env
```

Edit `.env` and replace `SA_PASSWORD` with a strong password (at least 8 characters, uppercase and lowercase letters, a number and a symbol). Do not use a semicolon in the password because it is part of the SQL connection string.

Generate a JWT secret in PowerShell:

```powershell
$bytes = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
$secret = [Convert]::ToBase64String($bytes)
```

Copy the generated value into `JWT_SECRET` in `.env`. Keep `.env` local; it is ignored by Git and must never be committed.

For image uploads, set `AZURE_BLOB_STORAGE_CONNECTION_STRING` to the connection string of your Azure Storage account. This setting is optional for the application to start, but uploads and image retrieval require it. Azure credentials are passed only to the API container, never to MVC.

### 3. Build and start the stack

```powershell
docker compose up --build -d
```

Compose performs the startup sequence:

1. Starts SQL Server and waits for its health check.
2. Runs the EF Core migration bundle against the SQL Server container.
3. Starts the API after migrations complete successfully.
4. Starts MVC after the API health check passes.

The first SQL Server image download and database initialisation can take several minutes.

### 4. Open the applications

- **RaceDay website:** http://localhost:8080
- **API Swagger:** http://localhost:8081/swagger
- **API health:** http://localhost:8081/health
- **MVC health:** http://localhost:8080/health

Swagger is enabled in the API's Development environment in this local Compose configuration. Do not expose the development Swagger interface publicly in a production deployment.

### 5. Check container status and logs

```powershell
docker compose ps
docker compose logs -f raceday-db
docker compose logs -f raceday-migrate
docker compose logs -f raceday-api
docker compose logs -f raceday-web
```

The migration container should exit with code 0. The API and MVC containers should report healthy.

### 6. Stop the application

```powershell
docker compose down
```

The named `raceday-sql-data` volume is retained, so your database records survive normal shutdown and container recreation.

**Warning:** this command permanently deletes the SQL Server data volume. Use it only when you intentionally want to reset the database:

```powershell
docker compose down --volumes
```

## Services and ports

| Service | Container port | Host port | Purpose |
|---|---:|---:|---|
| MVC | 8080 | 8080 | Browser-facing web application |
| API | 8080 | 8081 | REST API and Swagger |
| SQL Server | 1433 | 1433 | Persistent database |

The MVC container uses the Compose DNS name `raceday-api`, not `localhost`, to reach the API. Inside a container, `localhost` refers to that same container.

## Configuration and secrets

| Setting | Container environment variable | Used by |
|---|---|---|
| SQL connection | `ConnectionStrings__RaceDayConnection` | API and migration bundle |
| JWT signing secret | `Jwt__Secret` | API |
| JWT issuer | `Jwt__Issuer` | API |
| JWT audience | `Jwt__Audience` | API |
| Azure Blob connection | `AzureBlobStorage__ConnectionString` | API only |
| MVC API URL | `RaceDayApi__BaseUrl` | MVC |

The Docker Compose file supplies the SQL connection string and JWT issuer/audience. `.env` supplies the SQL password, JWT secret and optional Azure connection string. Never put real secrets in appsettings files or source control.

The containers listen on HTTP within the local Docker network. For an internet-facing deployment, put a TLS-terminating reverse proxy or managed ingress in front of MVC and API, configure HTTPS and secure cookies, and use a proper secret manager. Do not publish SQL Server's port publicly.

## Local development without Docker

The existing .NET workflow remains available. Configure SQL Server, JWT User Secrets and optional Azure Blob Storage User Secrets for the API, apply migrations, then run the API and MVC projects separately. See `RaceDay.Web/README.md` for the application feature overview and local development notes.

## Troubleshooting

- **Compose says SA_PASSWORD or JWT_SECRET is missing:** check that `.env` exists in the repository root and both values are set.
- **SQL Server is unhealthy:** check Docker Desktop memory allocation and `docker compose logs raceday-db`. The first startup can take a while.
- **Migration container exits unsuccessfully:** inspect `docker compose logs raceday-migrate`. The API is deliberately held back until migrations succeed. Do not delete the SQL volume to work around a migration error.
- **API does not start:** inspect `docker compose logs raceday-api`; check the SQL connection, JWT secret and migration result.
- **MVC cannot reach the API:** confirm the API health check passes and that `RaceDayApi__BaseUrl` is `http://raceday-api:8080/` inside Compose.
- **Image uploads fail:** configure `AZURE_BLOB_STORAGE_CONNECTION_STRING`; the API creates the private `event-banners` and `profile-pictures` containers on first upload.
- **Port already in use:** stop the process using 8080, 8081 or 1433, or change the host-side port mapping in `docker-compose.yml`.

## CI

The Part 3 GitHub Actions workflow builds the API and MVC projects and runs the unit tests. Docker image builds and live container integration should additionally be verified in an environment with Docker available.
