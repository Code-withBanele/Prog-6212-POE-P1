# RaceDay MVC Web Application

The Part 3 MVC application is an ASP.NET Core MVC front end for the existing RaceDay REST API. It does not connect directly to the database.

## Requirements

- .NET 10 SDK
- The RaceDay API from the `RaceDay/` project
- A configured API database and JWT secret for the API

## Configure the API URL

Update `RaceDayApi:BaseUrl` in `appsettings.json` to the address where the API is running. The default development value is:

```json
{
  "RaceDayApi": {
    "BaseUrl": "https://localhost:7192/"
  }
}
```

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

- MVC account registration communicates with `POST /api/Auth/register`.
- MVC login communicates with `POST /api/Auth/login`.
- The JWT is stored in server-side session and forwarded to API requests.
- An authentication cookie maintains the MVC sign-in state.
- Organiser and Participant roles have separate navigation and route policies.
- Participants can browse events, view event details, select a category and enrol through the API.
- Organisers have an event overview with event and enrolment totals.

Azure Blob Storage, complete event management, participant history/profile, Docker deployment and automated tests remain planned work for this branch.
