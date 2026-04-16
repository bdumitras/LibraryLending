# LibraryLending

A .NET 9 Web API for managing a library catalog, members, and lending flows, implemented with Clean Architecture, CQRS-style application handlers, EF Core, PostgreSQL, validation, structured logging, Swagger, unit tests, and integration tests.

This README focuses on two things:
- what the project does
- how to run it successfully on a real machine

It also includes the exact setup flow that worked on Windows during validation.

---

## What the project does

The API models three main concepts:
- **Book**: a catalog entry with inventory counters
- **Member**: a library member who can be active or inactive
- **Loan**: a lending record that connects a book and a member

### Main capabilities

- Books CRUD
- Members CRUD
- Create loan
- Return loan
- Filtering, search, sorting, and paging
- Centralized exception handling
- Structured logging
- Deterministic development seed data
- Unit tests
- Integration tests

### Important modeling decision

The original challenge mixes two ideas:
- inventory fields such as `TotalCopies` and `AvailableCopies`
- a rule example that says the same book cannot be loaned twice

In this implementation, a `Book` represents a catalog entry with inventory. That means lending eligibility is driven by available inventory.

So the effective lending rule in this project is:
- a book can be borrowed while copies are available
- a borrow operation decreases `AvailableCopies`
- a return operation increases `AvailableCopies`

This keeps the model coherent without introducing a separate physical `BookCopy` entity.

---

## Solution structure

```text
src/
  LibraryLending.Domain
  LibraryLending.Application
  LibraryLending.Infrastructure
  LibraryLending.Api

tests/
  LibraryLending.UnitTests
  LibraryLending.IntegrationTests
```

### Responsibilities by project

#### `LibraryLending.Domain`
Contains:
- entities
- enums
- domain services
- domain exceptions
- core business rules

#### `LibraryLending.Application`
Contains:
- commands and queries
- handlers
- validators
- DTOs and mappings
- repository abstractions
- paging/filtering models

#### `LibraryLending.Infrastructure`
Contains:
- `ApplicationDbContext`
- EF Core configurations
- repository implementations
- migrations
- seed data
- runtime database initialization

#### `LibraryLending.Api`
Contains:
- controllers
- API composition root
- Swagger/OpenAPI setup
- global exception handling
- request logging middleware

#### `LibraryLending.UnitTests`
Contains:
- domain unit tests
- validator tests
- handler tests

#### `LibraryLending.IntegrationTests`
Contains:
- real host integration tests
- PostgreSQL test container setup
- API integration tests

---

## Tech stack

- **Framework**: .NET 9
- **Persistence**: EF Core 9 + PostgreSQL
- **Validation**: FluentValidation
- **API documentation**: Swagger / OpenAPI
- **Logging**: JSON console logging + structured log messages
- **Integration test database**: PostgreSQL via Testcontainers
- **Database reset in integration tests**: Respawn

---

## Business rules currently enforced

Examples of rules already implemented in the project:
- book title, author, and ISBN are required
- `AvailableCopies` cannot be negative
- `AvailableCopies` cannot exceed `TotalCopies`
- a member must be active to borrow a book
- loan due date must be after loan date
- a loan cannot be returned twice
- a loan return date cannot be before the loan date
- duplicate ISBNs are not allowed
- duplicate member emails are not allowed
- a book cannot be borrowed when no copies are available

---

## API endpoints

### Books
- `GET /api/books`
- `GET /api/books/{id}`
- `POST /api/books`
- `PUT /api/books/{id}`
- `DELETE /api/books/{id}`

### Members
- `GET /api/members`
- `GET /api/members/{id}`
- `POST /api/members`
- `PUT /api/members/{id}`
- `DELETE /api/members/{id}`

### Loans
- `GET /api/loans`
- `GET /api/loans/{id}`
- `POST /api/loans`
- `POST /api/loans/{id}/return`

### Utility
- `GET /health`

---

## Filtering, search, sorting, and paging

### Books list
Supports:
- `page`
- `pageSize`
- `searchTerm`
- `title`
- `author`
- `isbn`
- `publicationYearFrom`
- `publicationYearTo`
- `isAvailable`
- `sortBy`
- `sortDirection`

### Members list
Supports:
- `page`
- `pageSize`
- `searchTerm`
- `fullName`
- `email`
- `isActive`
- `sortBy`
- `sortDirection`

### Loans list
Supports:
- `page`
- `pageSize`
- `searchTerm`
- `bookId`
- `memberId`
- `status`
- `loanDateFromUtc`
- `loanDateToUtc`
- `dueDateFromUtc`
- `dueDateToUtc`
- `sortBy`
- `sortDirection`

---

## Error handling

The API uses centralized exception handling and returns consistent problem-style JSON responses.

Examples of mapped errors:
- `400 Bad Request` for validation failures
- `404 Not Found` for missing resources
- `409 Conflict` for domain/application conflicts such as duplicate ISBN, duplicate email, inactive member borrow attempt, or no copies available
- `500 Internal Server Error` for unexpected failures

---

## Logging

The project includes:
- JSON console logging
- HTTP request logging middleware
- structured logs for command/query flows
- warning/error logging in the global exception handler

---

## Seed data

The project includes deterministic seed data for local development.

Seed contents:
- **120 books**
- **24 members**
- a mix of:
  - active loans
  - overdue loans
  - returned loans

Database initialization behavior:
- `appsettings.json`
  - migrations disabled on startup
  - seed disabled on startup
- `appsettings.Development.json`
  - migrations enabled on startup
  - seed enabled on startup

So in `Development`, the API is prepared to initialize the database automatically at startup.

If the database already contains data, the seeder skips reseeding.

---

## What needs to be installed before running the project

This is the part that matters most if someone clones the repository and just wants it to work.

### Required for all setups

You need:
- **.NET 9 SDK**

Check it with:

```bash
 dotnet --version
```

The project targets .NET 9 and will not run correctly on older SDKs.

### Required if you want to use Docker for the database

You need:
- **Docker Desktop** on Windows or macOS
- Docker must be running before you start the API

Check it with:

```bash
 docker --version
```

### Required if you do not want to use Docker

You need:
- **PostgreSQL** installed locally
- a local PostgreSQL server running
- a database named `library_lending_dev`
- credentials that match the connection string, or you must edit the connection string accordingly

### Required if you want to run integration tests

You need:
- **Docker Desktop**

The integration tests use **Testcontainers**, which starts a real PostgreSQL container automatically.
Without Docker, integration tests will not run.

### Optional but very useful

- **Git** — if you want to clone/push the repo
- **DBeaver** or **pgAdmin** — if you want a UI to inspect PostgreSQL tables and data

---

## Quick start that was validated on Windows

This is the exact route that worked locally during validation.

### 1. Install prerequisites

Install:
- .NET 9 SDK
- Docker Desktop

Then open Docker Desktop and make sure Docker is actually running.

### 2. Start PostgreSQL in Docker

Open `cmd` or PowerShell in the repository root and run:

#### Windows CMD

```cmd
 docker run --name library-lending-postgres ^
   -e POSTGRES_USER=postgres ^
   -e POSTGRES_PASSWORD=postgres ^
   -e POSTGRES_DB=library_lending_dev ^
   -p 5432:5432 ^
   -d postgres:17
```

#### PowerShell

```powershell
 docker run --name library-lending-postgres `
   -e POSTGRES_USER=postgres `
   -e POSTGRES_PASSWORD=postgres `
   -e POSTGRES_DB=library_lending_dev `
   -p 5432:5432 `
   -d postgres:17
```

If the container already exists and you only need to start it again:

```bash
 docker start library-lending-postgres
```

### 3. Restore packages

```bash
 dotnet restore
```

### 4. Build

```bash
 dotnet build
```

### 5. Run the API in Development

#### Windows CMD

```cmd
 set ASPNETCORE_ENVIRONMENT=Development
 dotnet run --project src\LibraryLending.Api
```

#### PowerShell

```powershell
 $env:ASPNETCORE_ENVIRONMENT="Development"
 dotnet run --project src/LibraryLending.Api
```

When it starts successfully, you should see lines similar to:
- `Now listening on: http://localhost:5136`
- `Now listening on: https://localhost:7136`
- `Application started`

### 6. Open Swagger

Use either:
- `http://localhost:5136/swagger`
- `https://localhost:7136/swagger`

During validation, both worked, but plain HTTP is usually the simplest if the HTTPS developer certificate is not trusted yet.

### 7. Verify health endpoint

```text
 http://localhost:5136/health
```

---

## Running on macOS or Linux with Docker

### 1. Install prerequisites

Install:
- .NET 9 SDK
- Docker Desktop (macOS) or Docker Engine (Linux)

### 2. Start PostgreSQL

```bash
 docker run --name library-lending-postgres \
   -e POSTGRES_USER=postgres \
   -e POSTGRES_PASSWORD=postgres \
   -e POSTGRES_DB=library_lending_dev \
   -p 5432:5432 \
   -d postgres:17
```

If it already exists:

```bash
 docker start library-lending-postgres
```

### 3. Restore, build, run

```bash
 dotnet restore
 dotnet build
 ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/LibraryLending.Api
```

### 4. Open Swagger

```text
 http://localhost:5136/swagger
```

or

```text
 https://localhost:7136/swagger
```

---

## Running without Docker

Yes, the API can also run **without Docker**.

In that case, Docker is not needed for the application itself, but you must provide PostgreSQL yourself.

### What you need

Install:
- .NET 9 SDK
- PostgreSQL locally

Then create a database:
- `library_lending_dev`

Make sure the connection string in:

```text
 src/LibraryLending.Api/appsettings.Development.json
```

matches your local PostgreSQL setup.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=library_lending_dev;Username=postgres;Password=postgres"
  }
}
```

Then run:

#### Windows CMD

```cmd
 dotnet restore
 dotnet build
 set ASPNETCORE_ENVIRONMENT=Development
 dotnet run --project src\LibraryLending.Api
```

#### macOS / Linux

```bash
 dotnet restore
 dotnet build
 ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/LibraryLending.Api
```

### Important note

Running without Docker is fine for the API itself.

However, **integration tests still require Docker**, because they use Testcontainers to spin up a real PostgreSQL database automatically.

---

## Connection string used by default in development

The project is configured around this development database shape:

- Host: `localhost`
- Port: `5432`
- Database: `library_lending_dev`
- Username: `postgres`
- Password: `postgres`

If your local PostgreSQL setup is different, update the connection string accordingly.

---

## EF Core migrations

If startup database initialization is enabled in `Development`, the app is prepared to apply migrations automatically at startup.

You can also run them manually.

### Install EF CLI tool if needed

```bash
 dotnet tool install --global dotnet-ef
```

### Apply migrations manually

```bash
 dotnet ef database update \
   --project src/LibraryLending.Infrastructure \
   --startup-project src/LibraryLending.Api
```

### Create a new migration

```bash
 dotnet ef migrations add <MigrationName> \
   --project src/LibraryLending.Infrastructure \
   --startup-project src/LibraryLending.Api
```

---

## Running tests

### Run all tests

```bash
 dotnet test
```

### Run only unit tests

```bash
 dotnet test tests/LibraryLending.UnitTests/LibraryLending.UnitTests.csproj
```

### Run only integration tests

```bash
 dotnet test tests/LibraryLending.IntegrationTests/LibraryLending.IntegrationTests.csproj
```

### Important note about integration tests

Integration tests require:
- Docker installed
- Docker running

because they start PostgreSQL containers dynamically through Testcontainers.

---

## Inspecting the database

If you want a tree-based UI similar to SQL Server Management Studio, install one of these:
- **DBeaver**
- **pgAdmin**

Connection values:
- Host: `localhost`
- Port: `5432`
- Database: `library_lending_dev`
- Username: `postgres`
- Password: `postgres`

With a GUI client you can:
- view tables
- browse rows
- run SQL queries
- inspect indexes and constraints

---

## Common issues and how to fix them

### `docker` is not recognized

Cause:
- Docker Desktop is not installed, or not available in PATH

Fix:
- install Docker Desktop
- start Docker Desktop
- reopen the terminal
- verify with:

```bash
 docker --version
```

### Build succeeds but the API fails at startup

Possible causes:
- PostgreSQL is not running
- wrong connection string
- database does not exist when using local PostgreSQL without Docker
- Docker container is not started

### Swagger opens but HTTPS shows certificate warnings

This is normal on a fresh machine.
You can either:
- use `http://localhost:5136/swagger`
- or trust the development certificate:

```bash
 dotnet dev-certs https --trust
```

### Integration tests fail locally

Most likely:
- Docker is not running
- Docker Desktop is not installed

### Port 5432 is already in use

You already have PostgreSQL running locally on that port.
You can either:
- stop the local PostgreSQL service
- or change the Docker port mapping and update the connection string

---

## Example manual API flow to verify everything

Once Swagger is open, a good end-to-end manual check is:

1. `GET /api/books`
2. `POST /api/members`
3. take one `bookId`
4. take the new `memberId`
5. `POST /api/loans`
6. `GET /api/loans?status=Active`
7. `POST /api/loans/{id}/return`
8. `GET /api/loans?status=Returned`

This verifies the main business flow, not just isolated endpoints.

---

## Current validated state

At the end of local validation, the project reached the following state:
- `dotnet build` succeeds
- `dotnet test` succeeds
- API starts successfully in `Development`
- Swagger works
- unit tests pass
- integration tests pass

That means the repository is in a runnable and testable state.

---

## Notes about the local validation route used here

The path that was actually validated successfully was:
- install .NET 9 SDK
- install Docker Desktop
- start PostgreSQL in Docker
- run `dotnet restore`
- run `dotnet build`
- run the API in `Development`
- open Swagger
- run `dotnet test`

Other routes may also work, especially using a locally installed PostgreSQL instead of Docker.
But the Docker-based route above is the one that was actually confirmed step by step."# LibraryLending" 
