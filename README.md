# RaceDay Events Management App

## Overview

RaceDay is an Events Management application designed to manage race events, organisers, participants, event categories, enrolments and race results.

The project includes a SQL Server database design, API endpoint documentation and an Entity Relationship Diagram (ERD).

Illustration video: [▶️ RaceDay illustration video](https://youtu.be/mhHBowUxImE)

## Repository Structure


```plaintext
ST10472346-RaceDay-Events-Management-App/
│
├── .github/
│   └── workflows/
│       └── validate-repository-structure.yml
│
├── docs/
│   ├── RaceDay.sql
│   ├── race_day_api_endpoints.pdf
│   └── race_day_erd.pdf
│
├── RaceDayAPI/
│   ├── Controllers/          # API endpoint controllers
│   ├── Models/               # Data entities and database models
│   ├── Data/                 # EF Core DbContext and migrations
│   ├── DTO/                  # Data transfer objects
│   ├── Security/             # Authentication and authorization
│   ├── Services/             # Business logic
│   ├── appsettings.json      # Configuration
│   ├── Program.cs            # Application startup
│   └── RaceDayAPI.csproj     # Main API project
│
├── RaceDayAPI.Test/
│   ├── AuthAndRoleTests.cs   # Authentication and authorization tests
│   ├── TestHelpers.cs        # Test utilities and fixtures
│   └── RaceDayAPI.Test.csproj
│
└── README.md
```
### Folder Description

| Folder/File                       | Description                                                                                    |
| --------------------------------- | ---------------------------------------------------------------------------------------------- |
| `.github/workflows/`              | Contains the GitHub Actions workflow used to validate the repository structure.                |
| `docs/RaceDay.sql`                | SQL Server database creation script and sample seed data.                                      |
| `docs/race_day_api_endpoints.pdf` | API endpoint documentation for the RaceDay application.                                        |
| `docs/race_day_erd.pdf`           | Entity Relationship Diagram showing the database structure.                                    |
| `RaceDayAPI/`                     | Main .NET 10 ASP.NET Core API project with controllers, models, and business logic.            |
| `RaceDayAPI.Test/`                | Unit and integration tests for the API using xUnit framework.                                 |
| `README.md`                       | Project documentation and repository information.                                              |

## Database

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

## API Implementation

The `RaceDayAPI` project provides a complete REST API implementation in .NET 10 with the following features:

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
- **DTO**: Request/Response models for API communication
- **Security**: Custom authorization attributes and authentication logic
- **Tests**: Unit and integration tests ensuring reliability

### Building & Running

```bash
# Build the solution
dotnet build

# Run the API
dotnet run

# Run tests
dotnet test
```

### Build Status

✅ **All tests passing**  
✅ **Solution builds successfully**  
✅ **Type safety verified** (RaceDayUser model properly configured)

## GitHub Actions / CI

GitHub Actions is used to automatically validate the required repository structure whenever changes are pushed to the repository or a pull request is created.

The workflow checks that:

* The `docs` folder exists.
* `RaceDay.sql` exists.
* `race_day_erd.pdf` exists.
* `race_day_api_endpoints.pdf` exists.

### CI/CD Workflow Screenshot

The following screenshot shows the successful GitHub Actions workflow execution:

![alt text](image.png)

## Project Demonstration

[▶️ Watch the RaceDay project demonstration on YouTube](https://youtu.be/sulZqaSXf58)

## Technologies

* .NET 10
* ASP.NET Core
* Entity Framework Core
* SQL Server
* xUnit (Testing)
* SQL
* Git
* GitHub
* GitHub Actions

## Project Documentation

All supporting project documentation is stored in the `/docs` folder.

RaceDayAPI project documentation (RaceDayAPI/docs):

- No documents currently present in RaceDayAPI/docs. Place project-specific API docs here if available.



## Author

**ST10472346**
