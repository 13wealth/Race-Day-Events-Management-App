# RaceDay Events Management App

## Overview

RaceDay is an Events Management application designed to manage race events, organisers, participants, event categories, enrolments and race results.

The project was delivered in three parts:

| Part | Focus |
| ---- | ----- |
| [Part 1](#part-1-database-design-and-documentation) | SQL Server database design, ERD and API endpoint documentation |
| [Part 2](#part-2-rest-api) | ASP.NET Core Web API (.NET 10) with authentication, role-based authorisation and unit tests |
| [Part 3](#part-3-mvc-web-application-docker-and-ci) | ASP.NET Core MVC front end, Azure Blob Storage support in the API, Docker and CI |

## Repository Structure

```plaintext
ST10472346-RaceDay-Events-Management-App/
│
├── .github/
│   └── workflows/
│       └── ci.yml
│
├── docs/
│   ├── RaceDay.sql
│   ├── race_day_api_endpoints.pdf
│   └── race_day_erd.pdf
│
├── RaceDay.Contracts/        # Shared DTOs (auth, categories, enrolments, events, profiles, results)
│
├── RaceDayAPI/               # REST API project (RaceDay.API)
│   ├── Controllers/          # API endpoint controllers
│   ├── Data/                 # EF Core DbContext
│   ├── Migrations/           # EF Core migrations
│   ├── Models/               # Data entities and database models
│   ├── Security/             # Authentication and authorization
│   ├── Services/             # Business logic
│   ├── Storage/              # Storage integration
│   ├── appsettings.json      # Configuration
│   ├── Program.cs            # Application startup
│   └── RaceDay.API.csproj    # API project file
│
├── RaceDay.MVC/              # MVC web application
│   ├── Controllers/
│   ├── Models/
│   ├── Services/             # API client services
│   ├── ViewModels/
│   ├── Views/
│   ├── wwwroot/
│   ├── appsettings.json      # Contains ApiSettings:BaseUrl
│   ├── Dockerfile            # Container build instructions for the MVC app
│   ├── Program.cs
│   └── RaceDay.MVC.csproj
│
├── RaceDayAPI.Test/
│   ├── AuthControllerTests.cs
│   ├── EnrollmentControllerTests.cs
│   ├── EventControllerTests.cs
│   ├── RoleTests.cs
│   └── RaceDayAPI.Test.csproj
│
├── .dockerignore             # Files excluded from the Docker build context
├── RaceDayAPI.slnx           # Solution file
└── README.md
```

### Folder Description

| Folder/File                       | Part | Description                                                                          |
| --------------------------------- | ---- | ------------------------------------------------------------------------------------ |
| `docs/RaceDay.sql`                | 1    | SQL Server database creation script and sample seed data.                            |
| `docs/race_day_api_endpoints.pdf` | 1    | API endpoint documentation for the RaceDay application.                              |
| `docs/race_day_erd.pdf`           | 1    | Entity Relationship Diagram showing the database structure.                          |
| `RaceDayAPI/`                     | 2    | Main .NET 10 ASP.NET Core API project with controllers, models and business logic.  |
| `RaceDayAPI.Test/`                | 2    | Unit tests for the API using the xUnit framework.                                    |
| `RaceDay.Contracts/`              | 2, 3 | Class library with the DTOs shared by the API and the MVC app.                       |
| `RaceDay.MVC/`                    | 3    | .NET 10 ASP.NET Core MVC web application that consumes the API.                      |
| `.dockerignore`                   | 3    | Keeps build output and other unneeded files out of the Docker build.                 |
| `.github/workflows/`              | 3    | Contains the GitHub Actions workflow (`ci.yml`) that builds the solution and runs the tests. |
| `RaceDayAPI.slnx`                 | 2, 3 | Visual Studio solution containing all four projects.                                 |
| `README.md`                       |      | Project documentation and repository information.                                    |

## Prerequisites

- .NET 10 SDK
- SQL Server Express (the connection string is in `RaceDayAPI/appsettings.json`; update `DefaultConnection` to match your SQL Server instance)
- The database created with `docs/RaceDay.sql`
- Docker Desktop (only needed for the Docker steps in Part 3)

---

## Part 1: Database Design and Documentation

Part 1 covers the design of the RaceDay system. All supporting documentation is stored in the `/docs` folder.

### Database

The `RaceDay.sql` script creates the RaceDay SQL Server database and its related tables.

The database includes entities for:

* Users
* Organisers
* Participants
* Profiles
* Events
* Categories
* Routes
* Enrolments
* Results
* Performance History

The script also contains sample data for testing and demonstration purposes.

### Documentation

| File                              | Description                                                   |
| --------------------------------- | ------------------------------------------------------------- |
| `docs/RaceDay.sql`                | SQL Server database creation script and sample seed data.     |
| `docs/race_day_api_endpoints.pdf` | API endpoint documentation for the RaceDay application.       |
| `docs/race_day_erd.pdf`           | Entity Relationship Diagram showing the database structure.   |

### Illustration video

[▶️ RaceDay illustration video](https://youtu.be/mhHBowUxImE)

### Repository structure check

In Part 1, a GitHub Actions workflow validated the required repository structure whenever changes were pushed. It checked that:

* The `docs` folder exists.
* `RaceDay.sql` exists.
* `race_day_erd.pdf` exists.
* `race_day_api_endpoints.pdf` exists.

The following screenshot shows a successful run of that check:

![GitHub Actions workflow run](image.png)

The current workflow is `ci.yml`, described in Part 3.

---

## Part 2: REST API

The `RaceDayAPI` project provides a complete REST API implementation in .NET 10.

### Key Features

- **Authentication**: User registration and login with session management
- **Authorization**: Role-based access control (Participant/Organizer roles)
- **User Management**: Create and manage user profiles with proper type safety (RaceDayUser model)
- **Password Security**: Hashing using ASP.NET Core Identity's PasswordHasher
- **Testing**: Comprehensive unit tests with xUnit framework

### Project Structure

- **Controllers**: API endpoints for authentication and event management
- **Models**: Entity definitions aligned with database schema
- **Data**: EF Core DbContext for database operations
- **Contracts**: Request/Response models (in the `RaceDay.Contracts` project)
- **Security**: Custom authorization attributes and authentication logic
- **Tests**: Unit tests ensuring reliability

### Building & Running

```bash
# Build the solution
dotnet build RaceDayAPI.slnx

# Run tests
dotnet test RaceDayAPI.slnx

# Run the API
dotnet run --project RaceDayAPI
```

By default the API listens on `http://localhost:5093` and `https://localhost:7054`.

### Build Status

✅ **All tests passing**  
✅ **Solution builds successfully**  
✅ **Type safety verified** (RaceDayUser model properly configured)

### Testing & Validation Checklist

The following functionality has been tested and verified to be working correctly using Swagger document.
Complete evidence of testing and results is documented in [docs/api-test-results-evidence.pdf](docs/api-test-results-evidence.pdf).

#### Completed Checks

- ✅ **RaceDay.Api builds with no errors** - Project successfully compiles in .NET 10
- ✅ **SQL Server connection works** - Database connectivity verified and functional
- ✅ **Swagger opens** - API documentation endpoint accessible via Swagger UI
- ✅ **Registration works** - User registration endpoint tested and operational
- ✅ **Login works and creates the API session** - Authentication and session management verified
- ✅ **GET /api/events works** - Event retrieval endpoint functional
- ✅ **Organiser can create/update/delete an Event** - Event management operations verified for organiser role
- ✅ **Participant can enrol** - Enrollment functionality tested and working
- ✅ **Organiser can view enrolments** - Organiser access to enrollment data verified
- ✅ **Organiser can capture results** - Result recording functionality operational
- ✅ **Participant can view own results** - Result retrieval for participants verified
- ✅ **All Part 2 unit tests pass** - Comprehensive unit test suite passing successfully

#### Test Evidence

Detailed test results, screenshots, and validation evidence are available in:
📄 **[docs/api-test-results-evidence.pdf](docs/api-test-results-evidence.pdf)**

### Project Demonstration

[▶️ Watch the RaceDay project demonstration on YouTube](https://youtu.be/sulZqaSXf58)

---

## Part 3: MVC Web Application, Docker and CI

Part 3 adds the MVC web front end, Azure Blob Storage support in the API, a Docker container for the MVC app, and a GitHub Actions workflow that builds and tests the solution.

### Architecture

```plaintext
Browser  →  RaceDay.MVC  →  RaceDayAPI  →  SQL Server
                                   └─────→  Azure Blob Storage
```

- `RaceDay.MVC` does not access the database directly. All data comes from the API over HTTP, using the address set in `ApiSettings:BaseUrl`.
- `RaceDay.Contracts` holds the data transfer objects (DTOs) shared by the API and the MVC app.
- Azure Blob Storage is used by the API for image storage (container `raceday-images`).

### MVC Web Application

The `RaceDay.MVC` project is the web front end. It calls the API for all data, including login, events, enrolments and results. The API address is configured in `RaceDay.MVC/appsettings.json`:

```json
"ApiSettings": {
  "BaseUrl": "http://localhost:5093/"
}
```

### User Roles and Features

RaceDay supports two user roles: Organiser and Participant. Each role has access to functionality designed for its specific responsibilities.

#### 1. Organiser

Organisers are responsible for managing race events and overseeing participant activities.

**Features:**

* Access the organiser dashboard.
* Create, view, update and delete race events.
* Upload and manage event banner images.
* Manage event categories.
* View and manage participant enrolments.
* Record and manage race results.

#### 2. Participant

Participants can browse available events, enrol in races and track their participation history.

**Features:**

* Browse available race events.
* Search and filter events.
* View event details.
* Enrol in available events.
* View personal event enrolments.
* View race history and finishing positions.
* Update personal profile information.
* Upload and update profile pictures.

### Login and Session Flow

RaceDay uses session-based authentication to manage user access and maintain login state.

1. **Login:** The user enters their email address and password on the login page.
2. **Authentication:** The MVC application sends the login credentials to the RaceDay API for verification.
3. **Session Creation:** Once authenticated, the application establishes a session containing the user's relevant information, including their role.
4. **Role-Based Redirection:** The user is redirected to the appropriate interface:
   * Organiser → Organiser Dashboard
   * Participant → Participant Dashboard
5. **Authorisation:** The application uses the authenticated user's role to control access to the relevant pages and functionality.
6. **Session Persistence:** The session maintains the user's authenticated state while navigating between pages.
7. **Logout:** When the user logs out, the session is cleared and the user is redirected to the login page.

Unauthenticated users are required to log in before accessing protected functionality.

### Azure Blob Storage configuration

The API stores images in the Azure Blob Storage container `raceday-images`. Real Azure connection details are **never committed to GitHub**. Add them with .NET user secrets on the API project (see the `AzureBlobStorage` section of `RaceDayAPI/appsettings.json` for the setting names) or with environment variables.

### Running the MVC app locally

With the API running (see Part 2), in a second terminal:

```bash
dotnet run --project RaceDay.MVC
```

Open the URL printed in the console.

### Running the MVC App with Docker

The MVC app runs in a Docker container. The API keeps running on the host machine, and the container reaches it through `host.docker.internal`. The API address is passed in with the `ApiSettings__BaseUrl` environment variable, so no code change or rebuild is needed to change it.

#### Requirements

- Docker Desktop running with Linux containers (WSL 2 backend)
- The API running on the host machine on port 5000

#### 1. Build the image

Run from the repository root (the folder containing `RaceDay.MVC`, `RaceDayAPI` and `RaceDay.Contracts`):

```bash
docker build -f RaceDay.MVC/Dockerfile -t raceday-mvc:part3 .
```

#### 2. Start the API on the host

In a separate terminal, from the repository root:

```bash
dotnet run --project RaceDayAPI --urls http://0.0.0.0:5000
```

#### 3. Run the MVC container

```bash
docker run -d --name raceday-mvc -p 8080:8080 -e ApiSettings__BaseUrl=http://host.docker.internal:5000/ raceday-mvc:part3
```

The container shows as running with `docker ps`:

![docker ps showing the raceday-mvc container running on port 8080](docs/docker-ps.png)

#### 4. Open the app

Browse to http://localhost:8080

The RaceDay MVC app running from the container and showing data from the API:

![RaceDay MVC app running at localhost:8080](docs/app-localhost-8080.png)

#### Useful Docker commands

| Command                    | Purpose                                                             |
| -------------------------- | ------------------------------------------------------------------- |
| `docker ps`                | Show the running container                                          |
| `docker logs raceday-mvc`  | Show the container logs                                             |
| `docker stop raceday-mvc`  | Stop the container                                                  |
| `docker start raceday-mvc` | Start the stopped container again                                   |
| `docker rm raceday-mvc`    | Remove the container (required before running step 3 a second time) |

The container logs show the MVC app calling the API through `host.docker.internal:5000` and receiving `200` responses:

![docker logs for raceday-mvc](docs/docker-logs.png)

#### Troubleshooting

- **Container name already in use**: run `docker rm -f raceday-mvc`, then run step 3 again.
- **Port 8080 already in use**: stop the other container or program using it, or map a different host port, for example `-p 8081:8080`.
- **Pages load but data fails**: check that the API is running on port 5000, then run `docker logs raceday-mvc`.
- **Old page shown in the browser**: clear the browser cache or use a private window.

✅ **Verified:** the MVC app builds as a Docker image and runs in a container. Login and the events pages work against the API through `host.docker.internal`.

### GitHub Actions / CI

GitHub Actions builds the whole solution and runs the unit tests whenever changes are pushed to the repository or a pull request is created.

The workflow is `.github/workflows/ci.yml`. It:

1. Checks out the code.
2. Sets up the .NET 10 SDK.
3. Builds the solution with `dotnet build RaceDayAPI.slnx` (API, Contracts, MVC and test projects).
4. Runs the unit tests with `dotnet test RaceDayAPI.slnx`.

#### Local verification

The same two commands were run locally on 12 November 2026:

* `dotnet build RaceDayAPI.slnx`: build succeeded.
* `dotnet test RaceDayAPI.slnx`: 17 tests, 17 passed, 0 failed, 0 skipped.

![dotnet build result](docs/dotnet-build.png)

![dotnet test result showing 17 tests passed](docs/dotnet-test.png)

> **Note:** At the time of submission, GitHub Actions runs are blocked by a billing lock on the GitHub account ("The job was not started because your account is locked due to a billing issue"). This is an account issue on GitHub's side and not a problem with the workflow or the code.

![GitHub Actions run blocked by the account billing lock](docs/github-actions-billing-error.png)

---

## Technologies

* .NET 10
* ASP.NET Core Web API
* ASP.NET Core MVC
* Entity Framework Core
* SQL Server
* Azure Blob Storage
* Docker
* xUnit (Testing)
* SQL
* Git
* GitHub
* GitHub Actions

## Author

**ST10472346**