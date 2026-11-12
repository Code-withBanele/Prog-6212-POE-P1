# RaceDay — Programming 2B POE Part 3 Demonstration Script

**Presenter:** Banele Innocent Mjayezi  
**Student number:** ST10489580  
**Target duration:** 8–10 minutes  
**Video title:** RaceDay — Programming 2B POE Part 3 Demonstration  
**YouTube URL:** [PASTE FINAL UNLISTED YOUTUBE URL HERE]

---

## Before recording

- [ ] Use the `part-3-mvc` branch.
- [ ] Start Docker Compose and wait for the services to start.
- [ ] Open the MVC site: http://localhost:8080
- [ ] Open API Swagger: http://localhost:8081/swagger
- [ ] Open the GitHub repository on `part-3-mvc`.
- [ ] Open the successful CI run: https://github.com/Code-withBanele/Prog-6212-POE-P1/actions/runs/43203480536
- [ ] Have organiser and participant accounts ready if available.
- [ ] If showing event creation, ensure at least one category and one route already exist. A fresh database requires these prerequisite records.
- [ ] If showing image upload, configure Azure Blob Storage. Otherwise explain the integration without claiming a live upload.
- [ ] Close notifications and unrelated windows.
- [ ] Never show passwords, JWTs, API keys, connection strings or the contents of `.env`.

**Recording rule:** Do not spend the recording troubleshooting. If a workflow is unavailable, state that accurately and show the relevant implementation evidence instead. Never claim an action succeeded unless it is visibly completed.

---

# Full video script

## 0:00–0:40 — Introduction

**Screen:** RaceDay home page.

**Narration:**

"Hello, my name is Banele Innocent Mjayezi, student number ST10489580. This video demonstrates my Programming 2B Portfolio of Evidence, Part 3.

The project is RaceDay, an event-management application. In this stage, I developed an ASP.NET Core MVC web application that consumes the RaceDay REST API developed in the earlier parts of the project.

I will demonstrate the application from the participant and organiser perspectives, explain its architecture and data flow, and finish by showing the repository, automated workflow and Docker deployment."

## 0:40–1:35 — Architecture and technology

**Screen:** Show the repository folders `RaceDay.Web`, `RaceDay`, `RaceDay.Tests`, and `docker-compose.yml`. Briefly show the README architecture section.

**Narration:**

"RaceDay is separated into a presentation layer, an API layer and a database layer.

The RaceDay.Web project is the ASP.NET Core MVC frontend. It contains the views, controllers, forms and role-specific navigation users interact with through a browser.

The RaceDay project is the REST API. It handles application requests, authentication, validation, database operations and integration with Azure Blob Storage.

Entity Framework Core is used for data access and database migrations, while SQL Server stores the application's relational data.

The MVC application communicates with the API over HTTP. It does not connect directly to SQL Server or directly to Azure Blob Storage. This separation keeps the presentation layer independent of the data-storage implementation.

Docker Compose brings the services together for local deployment."

## 1:35–2:30 — Account access and role separation

**Screen:** Show the login or registration page. If prepared, sign in as a participant and then an organiser.

**Narration:**

"Users access RaceDay through the account interface. Registration and login are handled through the API.

After authentication, the MVC application maintains the user's signed-in state using cookie authentication and server-side session. The API token is retained in the session for authenticated API requests.

The application separates the organiser and participant experiences. Navigation and protected areas depend on the authenticated user's role.

This allows organisers to manage event information and results, while participants access event discovery and their own participation features."

**If only one account is available:**

"I have prepared this environment with the account available for this recording. The application implements separate organiser and participant access policies. I will show the available area and the corresponding implementation evidence."

## 2:30–4:10 — Organiser experience

**Screen:** Sign in as an organiser and open the organiser dashboard. Show the route page, event form, event list, enrolments and result-entry area where available.

**Narration:**

"The organiser area provides functionality for managing RaceDay events.

Before an event can be created, the system needs supporting data, including categories and a route. On a fresh database, these records must first be configured. Categories are managed through the Categories API, and routes can be created through the organiser route interface.

Once this supporting data exists, an organiser can create an event and associate the relevant categories and route.

Organisers can also edit and delete events, review event enrolments and record race results, including finishing information. This supports the event administration workflow from initial setup through to recording race outcomes.

The application also supports event-banner image uploads through the API's Azure Blob Storage integration when storage is configured."

**Important:** If the event form reports that categories or routes are missing, explain the prerequisite once. Do not repeatedly retry it. Show the form and continue to the next section. Do not delete or modify important records just for the video.

## 4:10–5:35 — Participant experience

**Screen:** Sign in as a participant and open the events area. Show event listing, details, categories, enrolment, history and profile where available.

**Narration:**

"From the participant perspective, users can browse available events and view event information.

The event details include the categories available for participation. A participant can select an appropriate category and submit an enrolment.

Participants can also access their race history and update their profile. Profile-picture upload is supported through the API and Azure Blob Storage integration when the storage configuration is available.

The participant area provides access to event information and personal participation features without exposing organiser-only management functionality."

## 5:35–6:25 — API and data responsibilities

**Screen:** Open Swagger at http://localhost:8081/swagger. Show endpoint groups without executing destructive requests or exposing credentials.

**Narration:**

"The MVC frontend relies on the REST API for application data. The API exposes the endpoints used by the web application, while Entity Framework Core manages the relationship between application models and SQL Server.

The API is also responsible for validating application operations. For example, an enrolment category must be one of the categories offered by the selected event.

Image storage is handled through the API. The MVC application does not need Azure credentials, which keeps storage credentials out of the browser-facing layer."

## 6:25–7:20 — Database and Azure integration

**Screen:** Show migration files or the DbContext. Show configuration key names only, never their values.

**Narration:**

"SQL Server is the persistent relational database for RaceDay. Entity Framework Core migrations are included so the database schema can be updated in a controlled way.

The Part 3 implementation includes database changes for the event-to-category relationship and image metadata.

For images, the API integrates with Azure Blob Storage using private containers for event banners and participant profile pictures. The MVC application sends image requests through the API rather than connecting directly to Azure.

The Azure connection string is supplied through configuration and must be kept out of source control. Image operations require a valid storage connection string."

## 7:20–8:20 — Docker deployment

**Screen:** Show `docker-compose.yml`, then run or show `docker compose ps`. Do not display the local `.env` contents.

**Narration:**

"The application can be run as a multi-container setup using Docker Compose.

The Compose configuration starts SQL Server, waits for its health check, runs the Entity Framework Core migration service, starts the API and then starts the MVC application after the API becomes healthy.

The MVC website is available on localhost port 8080. The API is published on port 8081, and SQL Server uses port 1433.

The database uses a named Docker volume so normal container shutdown does not remove the stored data.

This provides a repeatable local deployment without requiring the API, MVC application and database to be started manually as separate processes."

**Screen direction:** If the migration container completed successfully, point out its successful completion. Do not describe an unsuccessful service as healthy.

## 8:20–9:15 — Automated workflow and repository

**Screen:** Show the repository on the `part-3-mvc` branch, then open the successful Actions run.

**Narration:**

"The source code for this submission is maintained in GitHub on the part-3-mvc branch.

The repository separates the API, MVC frontend and test project. It also includes Dockerfiles, Docker Compose configuration, an environment-variable example and the GitHub Actions workflow.

The Part 3 workflow restores the projects, builds the API and MVC applications in Release configuration, runs the automated test project and builds both Docker images.

Here is the recorded successful workflow for the latest validation-script fix. This provides evidence that the configured CI steps completed successfully for that commit."

**Screen direction:** Show the successful workflow summary and completed jobs. Keep the branch name and commit visible if possible.

## 9:15–9:40 — Conclusion

**Screen:** Return to the RaceDay home page or repository README.

**Narration:**

"To conclude, Part 3 adds an ASP.NET Core MVC web interface to the RaceDay system and connects it to the existing REST API.

The application provides separate organiser and participant experiences, event and enrolment management, result recording, profile functionality, Azure Blob Storage integration and a Docker-based local deployment.

The source code, setup instructions and demonstration documentation are available in the Part 3 GitHub repository on the part-3-mvc branch.

Thank you for watching."

---

## Final recording and upload checklist

- [ ] Introduction identifies the assessment and presenter.
- [ ] Architecture and separation between MVC, API and database are explained.
- [ ] Account access and role separation are shown or accurately explained.
- [ ] Organiser functionality is demonstrated.
- [ ] Participant functionality is demonstrated.
- [ ] API responsibilities are explained using Swagger or source code.
- [ ] Database migrations and Azure integration are explained.
- [ ] Docker Compose deployment is shown.
- [ ] Successful GitHub Actions workflow is shown.
- [ ] No passwords, JWTs, API keys or connection strings are visible.
- [ ] Audio and screen readability are checked.
- [ ] Video is uploaded using the visibility setting permitted by the institution.
- [ ] Final video URL is added to the root README and LMS submission.

## Final video details

**YouTube URL:** [PASTE FINAL URL HERE]  
**Upload visibility:** [UNLISTED / PUBLIC / OTHER APPROVED SETTING]  
**Date recorded:** [INSERT DATE]
