# RaceDay MVC Web Application

The Part 3 MVC application is an ASP.NET Core MVC front end for the RaceDay REST API. The MVC project does not connect directly to the database or Azure Blob Storage.

## Requirements

- .NET 10 SDK
- The RaceDay API from the `RaceDay/` project
- SQL Server connection string and JWT settings for the API
- Azure Storage account connection string for image uploads

## Configure the API URL

Update `RaceDayApi:BaseUrl` in `RaceDay.Web/appsettings.json` to the address where the API is running. The default development value is:

```json
{
  "RaceDayApi": {
    "BaseUrl": "https://localhost:7192/"
  }
}
```

## Configure Azure Blob Storage

The API owns all Azure Storage operations. The MVC application uploads images to API endpoints and never receives Azure credentials.

Set the connection string as a user secret on the API project:

```bash
dotnet user-secrets set "AzureBlobStorage:ConnectionString" "<your-storage-connection-string>" --project RaceDay/RaceDay.csproj
```

Or provide the environment variable:

```text
AzureBlobStorage__ConnectionString
```

The API creates these private containers on first upload:

- `event-banners`
- `profile-pictures`

Image upload endpoints accept JPG, PNG and WebP files up to 5 MB. Image delivery is proxied through API and MVC routes so browser clients do not need Azure credentials or direct access to the storage account.

If the connection string is not configured, upload and image retrieval endpoints return a clear service-unavailable response. Do not commit storage connection strings to source control.

## Apply database migrations

From the repository root:

```bash
dotnet ef database update --project RaceDay/RaceDay.csproj
```

The current migrations add the event-to-category join table (preserving each event's existing category) and nullable blob-name fields for event banners and participant profile pictures.

## Run locally

Start the API first:

```bash
dotnet run --project RaceDay/RaceDay.csproj
```

In another terminal, start the MVC application:

```bash
dotnet run --project RaceDay.Web/RaceDay.Web.csproj
```

Open the MVC URL printed by the .NET CLI.

## Current implementation

- MVC registration and login communicate with the API; JWTs are stored in server-side session.
- Cookie authentication maintains MVC sign-in state, with separate Organiser and Participant navigation and route policies.
- Participants can browse events, view details, choose from event-specific categories, enrol, view race history and update their profile.
- Organisers can create routes and events, assign multiple categories, edit and delete events, view event enrolments and record finish times and positions.
- The API validates that an enrolment category is offered by the selected event.
- Organisers can upload event banners; participants can upload profile pictures.
- Azure Blob Storage is integrated through the API using private containers. The MVC application does not access Azure directly.
- Event category and blob metadata database migrations are included.
- GitHub Actions builds the API and MVC projects and runs the unit test project.

Docker deployment instructions are in the repository-root README. A live deployment still requires the user to configure their SQL password, JWT secret and optional Azure Storage connection string in a local .env file.
