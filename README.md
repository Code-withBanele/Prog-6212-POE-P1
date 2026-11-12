# RaceDay — Part 3: ASP.NET Core MVC Event Management System

**Module:** Programming 2B  
**Assessment:** Portfolio of Evidence — Part 3  
**Student:** Banele Innocent Mjayezi  
**Student number:** ST10489580  
**Technology:** .NET 10, ASP.NET Core MVC, ASP.NET Core Web API, Entity Framework Core, SQL Server and Docker

> **Submission branch:** `part-3-mvc`  
> **Demonstration video:** **[INSERT UNLISTED YOUTUBE VIDEO URL HERE]**

---

## 1. Project overview

RaceDay is an event-management application designed to support the administration and participation of sporting events. Part 3 introduces an ASP.NET Core MVC web application that consumes the RaceDay REST API developed in the earlier project stages.

The application separates the presentation layer from the API and data layer. The MVC application handles browser-facing pages and user interaction. The API handles application data, authentication tokens, database operations and Azure Blob Storage integration.

### Main objectives

- Provide a browser-based interface for participants and organisers.
- Separate access according to user roles.
- Allow participants to discover events and manage their participation.
- Allow organisers to manage routes, events, enrolments and results.
- Store application data in SQL Server through Entity Framework Core.
- Support event-banner and participant-profile image storage through Azure Blob Storage.
- Provide a repeatable local deployment using Docker Compose.

## 2. Technology stack

| Component | Technology | Responsibility |
|---|---|---|
| Web frontend | ASP.NET Core MVC (.NET 10) | Views, forms, navigation and browser interaction |
| Backend | ASP.NET Core Web API (.NET 10) | REST endpoints and business operations |
| Authentication | JWT and MVC cookie authentication | API authorisation and MVC sign-in state |
| Data access | Entity Framework Core | Object-relational mapping and migrations |
| Database | Microsoft SQL Server 2022 | Persistent application data |
| Image storage | Azure Blob Storage | Event banners and participant profile images |
| Containerisation | Docker and Docker Compose | Local multi-service deployment |
| CI | GitHub Actions | Restore, build, unit-test and Docker image build workflow |

## 3. Application architecture

The solution uses a layered client/API/database arrangement:

```text
                    Browser
                       |
                       v
              RaceDay.Web (MVC)
              - Views and forms
              - Session and cookies
              - Role-specific navigation
                       |
                 HTTP API calls
                       |
                       v
                RaceDay REST API
              - JWT authentication
              - Application endpoints
              - Validation and logic
              - Entity Framework Core
                 /           \
                v             v
          SQL Server     Azure Blob Storage
          Application    Event/profile images
          data
```

The MVC application communicates with the API through the configured API base URL. It does not connect directly to SQL Server or Azure Blob Storage. The API is responsible for database access and storage operations.

### Docker network

When using Docker Compose, the MVC container calls the API at `http://raceday-api:8080/`. The browser accesses the MVC application at `http://localhost:8080`. The API is published on host port `8081` for Swagger and API access.

## 4. Repository structure

The main Part 3 solution components are:

| Path | Purpose |
|---|---|
| `RaceDay/` | ASP.NET Core REST API |
| `RaceDay.Web/` | ASP.NET Core MVC web application |
| `RaceDay.Tests/` | Automated test project |
| `docker-compose.yml` | Local multi-container orchestration |
| `RaceDay/Dockerfile` | API container build |
| `RaceDay.Web/Dockerfile` | MVC container build |
| `.env.example` | Template for local Docker environment variables |
| `.github/workflows/raceday-part3.yml` | Part 3 continuous-integration workflow |
| `docs/VIDEO-DEMONSTRATION-SCRIPT.md` | Full walkthrough script for the submission video |

## 5. Implemented application functionality

The following is a guide to the implemented areas of the Part 3 application. The exact options visible to a signed-in user depend on their role and the data configured in the database.

### 5.1 Account access and role separation

- Registration and login are handled through the API.
- The MVC application maintains sign-in state using cookie authentication and server-side session.
- The API JWT is retained in the MVC session for authenticated API requests.
- Organiser and Participant areas use role-specific navigation and access policies.

### 5.2 Participant functionality

Participants can:

- Browse available events.
- View event information and available categories.
- Enrol in an event.
- View their race participation/history.
- Update their profile.
- Upload a profile picture, where Azure Blob Storage is configured.

### 5.3 Organiser functionality

Organisers can:

- Access the organiser area and dashboard.
- Create routes.
- Create events and associate event categories.
- Edit and delete events.
- View event enrolments.
- Record race results, including finishing information.
- Upload event-banner images, where Azure Blob Storage is configured.

**Initial configuration note:** An event requires available category and route records. A new or empty database may therefore require the organiser to configure the required category data through the Categories API and create at least one route before creating an event. This is initial application data setup, not a Docker installation step.

### 5.4 Image storage

The API integrates with Azure Blob Storage. The MVC application sends image requests to the API rather than accessing Azure directly. The implementation uses private containers for:

- `event-banners`
- `profile-pictures`

Supported image formats are JPG, PNG and WebP, with a 5 MB upload limit. Azure Storage credentials must remain on the API side and must not be committed to the repository.

## 6. Requirements

### Recommended Docker setup

- Windows, macOS or Linux.
- Docker Desktop or Docker Engine with Docker Compose v2.
- Git.
- At least 4 GB of memory available to Docker; more may be needed depending on the host machine.
- A strong SQL Server SA password.
- A JWT signing secret.
- An Azure Storage connection string if image upload and retrieval are to be used.

### Local .NET development

- .NET 10 SDK.
- SQL Server instance.
- Entity Framework Core tooling compatible with the project.
- API JWT configuration.
- Azure Storage connection string if image features are required.

## 7. Run the application with Docker Compose

Docker Compose is the recommended way to run the complete local application stack.

### Step 1 — Clone the repository and select Part 3

```powershell
git clone https://github.com/Code-withBanele/Prog-6212-POE-P1.git
cd Prog-6212-POE-P1
git switch part-3-mvc
```

For an existing clone:

```powershell
git switch part-3-mvc
git pull origin part-3-mvc
```

### Step 2 — Create the environment file

```powershell
Copy-Item .env.example .env
```

Open `.env` and replace the example values. Set a strong SQL Server password and a long, random JWT secret. The SQL Server password must meet SQL Server password complexity requirements; avoid semicolons because the password is included in a connection string.

A JWT secret can be generated in PowerShell:

```powershell
$bytes = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
$secret = [Convert]::ToBase64String($bytes)
$secret
```

Copy the generated value into `JWT_SECRET` in the local `.env` file.

Set `AZURE_BLOB_STORAGE_CONNECTION_STRING` if image upload and retrieval are required. It can remain unset when demonstrating functionality that does not require Azure image storage.

**Security:** Do not commit `.env`, database passwords, JWT secrets or Azure credentials.

### Step 3 — Build and start the services

Run from the repository root:

```powershell
docker compose up --build -d
```

Compose starts the services in dependency order:

1. SQL Server starts and passes its health check.
2. The migration service applies the EF Core migrations.
3. The API starts after migration completion.
4. The MVC application starts after the API is healthy.

The first startup may take several minutes while Docker downloads images and SQL Server initialises.

### Step 4 — Open the application

| Service | Address |
|---|---|
| RaceDay MVC website | http://localhost:8080 |
| API Swagger | http://localhost:8081/swagger |
| API health endpoint | http://localhost:8081/health |
| MVC health endpoint | http://localhost:8080/health |

Swagger is provided for local development and demonstration. It should not be exposed publicly as an unsecured production interface.

### Step 5 — Inspect service status and logs

```powershell
docker compose ps
docker compose logs -f raceday-db
docker compose logs -f raceday-migrate
docker compose logs -f raceday-api
docker compose logs -f raceday-web
```

The migration service is expected to complete successfully and exit. The API and MVC services should become healthy.

### Step 6 — Stop the services

```powershell
docker compose down
```

This stops and removes the containers but retains the named SQL Server data volume.

To intentionally delete the database volume and reset local data:

```powershell
docker compose down --volumes
```

**Warning:** `--volumes` permanently removes the persisted database data. Do not use it unless a database reset is intended.

## 8. Configuration reference

The Docker Compose configuration uses the following environment variables:

| Setting | Environment variable | Purpose |
|---|---|---|
| SQL connection | `ConnectionStrings__RaceDayConnection` | API and migration database connection |
| SQL password | `SA_PASSWORD` | SQL Server administrator password |
| JWT secret | `JWT_SECRET` / `Jwt__Secret` | Token signing |
| JWT issuer | `Jwt__Issuer` | Token issuer validation |
| JWT audience | `Jwt__Audience` | Token audience validation |
| Azure Storage | `AZURE_BLOB_STORAGE_CONNECTION_STRING` / `AzureBlobStorage__ConnectionString` | API image storage |
| MVC API URL | `RaceDayApi__BaseUrl` | API address used by MVC |

The root `.env.example` file documents the environment values expected by Docker Compose. The MVC project also contains its own README with additional project-specific notes.

## 9. Run without Docker

For development using the .NET CLI:

1. Configure a SQL Server instance and the API connection string.
2. Configure the API JWT settings using User Secrets or environment variables.
3. Configure the Azure Blob Storage connection string if image features are required.
4. Apply the database migrations.
5. Start the API.
6. Start the MVC application in a second terminal.

Example commands from the repository root:

```powershell
dotnet ef database update --project RaceDay/RaceDay.csproj
dotnet run --project RaceDay/RaceDay.csproj
```

In a second terminal:

```powershell
dotnet run --project RaceDay.Web/RaceDay.Web.csproj
```

The MVC API base URL must point to the running API address. See `RaceDay.Web/README.md` for the development configuration notes.

## 10. Automated build and test workflow

The GitHub Actions workflow is located at:

` .github/workflows/raceday-part3.yml `

It is triggered by pushes and pull requests targeting `part-3-mvc`. The workflow:

1. Checks out the repository.
2. Installs the .NET 10 SDK.
3. Restores the API, MVC and test projects.
4. Builds the API in Release configuration.
5. Builds the MVC application in Release configuration.
6. Runs the automated test project.
7. Builds the API Docker image.
8. Builds the MVC Docker image.

**Latest recorded successful workflow:**  
https://github.com/Code-withBanele/Prog-6212-POE-P1/actions/runs/43203480536

A successful CI run confirms the steps in that workflow completed for the commit shown in the run. It is not a substitute for demonstrating application workflows in the video.

## 11. Troubleshooting

| Issue | Suggested check |
|---|---|
| Missing environment variables | Confirm that `.env` exists in the repository root and required values are populated. |
| SQL Server does not become healthy | Inspect `docker compose logs raceday-db` and confirm Docker has sufficient memory. |
| Migration service fails | Inspect `docker compose logs raceday-migrate`; do not delete the database volume as a first response. |
| API does not start | Inspect `docker compose logs raceday-api`; check database and JWT configuration. |
| MVC cannot reach the API | Confirm the API is healthy and MVC uses `http://raceday-api:8080/` inside Compose. |
| Event creation reports no categories or routes | Configure at least one category through the Categories API and create a route before creating an event. |
| Image upload or retrieval fails | Check that the API has a valid Azure Storage connection string and that the storage account is reachable. |
| Port is already in use | Stop the process using host port 8080, 8081 or 1433, or adjust the host-side port mapping. |

## 12. Known configuration and deployment considerations

- A fresh database may not contain the category and route records required for event creation. These records must be configured before creating the first event.
- Azure Blob Storage functionality requires a valid storage connection string. The application can be started without it, but image operations will not be available.
- Docker Compose is intended for local demonstration and development. A production deployment requires HTTPS, secure secret management, suitable cookie settings, protected database networking and production hosting configuration.
- The MVC application accesses data through the API; it does not connect directly to SQL Server.

## 13. Submission information

| Item | Submission value |
|---|---|
| GitHub repository | https://github.com/Code-withBanele/Prog-6212-POE-P1 |
| Submission branch | `part-3-mvc` |
| Latest Part 3 commit | `235e81c0a8a97066a6e4e95b92e57b47d8766dba` |
| Demonstration video URL | **[PASTE UNLISTED YOUTUBE URL HERE]** |
| CI evidence | https://github.com/Code-withBanele/Prog-6212-POE-P1/actions/runs/43203480536 |

### Final LMS submission checklist

- [ ] Confirm the official assessment brief's required submission format.
- [ ] Confirm the repository is accessible to the assessor.
- [ ] Confirm the `part-3-mvc` branch is the branch being assessed.
- [ ] Upload any additional files explicitly required by the assessment brief.
- [ ] Record the demonstration using the accompanying script.
- [ ] Upload the video as Unlisted (if permitted by the institution).
- [ ] Replace the video URL placeholder above with the final video link.
- [ ] Include the same video URL in the LMS submission field, if one is provided.
- [ ] Keep a copy of the final submission details.

---

## 14. Demonstration video

**Video title:** RaceDay — Programming 2B POE Part 3 Demonstration  
**Video URL:** [INSERT UNLISTED YOUTUBE URL HERE]  
**Suggested duration:** 8–10 minutes  
**Script:** [docs/VIDEO-DEMONSTRATION-SCRIPT.md](docs/VIDEO-DEMONSTRATION-SCRIPT.md)

The demonstration should explain the system architecture and show the application from both organiser and participant perspectives. It should also show the repository, automated workflow and Docker deployment evidence. Do not show secrets, passwords, access tokens or private connection strings in the recording.
