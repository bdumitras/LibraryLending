# Architecture Conventions

This document closes **Phase 1 (Foundation)** by making the architectural conventions explicit before implementation work begins.

## 1. Solution layout

```text
LibraryLending/
├── docs/
│   ├── architecture/
│   └── spec/
├── src/
│   ├── LibraryLending.Domain/
│   ├── LibraryLending.Application/
│   ├── LibraryLending.Infrastructure/
│   └── LibraryLending.Api/
└── tests/
    ├── LibraryLending.UnitTests/
    └── LibraryLending.IntegrationTests/
```

## 2. Project responsibilities

### LibraryLending.Domain
Owns the core business model and business rules.

Contains:
- entities
- value objects
- domain exceptions
- domain services, if needed
- base abstractions shared by the domain model

Must not depend on any other project.

### LibraryLending.Application
Owns use cases and application orchestration.

Contains:
- commands and queries
- handlers
- DTOs
- validators
- repository and persistence abstractions
- pagination contracts

Depends only on `LibraryLending.Domain`.

### LibraryLending.Infrastructure
Owns external concerns and technical implementations.

Contains:
- EF Core `DbContext`
- entity configurations
- repository implementations
- migrations
- seed data
- logging-related infrastructure configuration

Implemented so far in Infrastructure:
- `ApplicationDbContext`
- PostgreSQL provider wiring
- EF Core configurations for `Book`, `Member`, and `Loan`
- unique indexes for ISBN and member email
- foreign keys from loans to books and members
- database-level check constraints for key invariants
- repository implementations for books, members, and loans
- shared query pagination helper used by repositories
- initial migration scaffold committed to source control
- design-time `ApplicationDbContextFactory` for `dotnet ef` operations

Depends on `LibraryLending.Application` and `LibraryLending.Domain`.

### LibraryLending.Api
Owns the HTTP boundary.

Contains:
- controllers or minimal endpoints
- exception mapping
- request/response serialization concerns
- OpenAPI / Swagger setup
- composition root and DI wiring

Depends on `LibraryLending.Application` and `LibraryLending.Infrastructure`.

### Test projects
- `LibraryLending.UnitTests`: domain and application tests in isolation
- `LibraryLending.IntegrationTests`: API + persistence behavior end-to-end

## 3. Feature organization

The project will use **vertical slices inside the Application layer**.

Planned structure:

```text
LibraryLending.Application/
├── Common/
│   ├── Abstractions/
│   └── Models/
└── Features/
    ├── Books/
    │   ├── Commands/
    │   └── Queries/
    ├── Members/
    │   ├── Commands/
    │   └── Queries/
    └── Loans/
        ├── Commands/
        └── Queries/
```

Each feature slice will keep together:
- request model
- validator
- handler
- response DTO

Task 7 standardizes the contract layer further:
- request contracts for create/update operations
- response DTOs for read operations
- mapping helpers from domain entities to response DTOs

This keeps behavior close to the feature it belongs to and avoids large generic service classes.

## 4. CQRS convention

The Application layer will use a lightweight CQRS style with **custom request/handler abstractions**.

Decision:
- no MediatR dependency in the initial version
- commands represent writes
- queries represent reads
- handlers execute one request each

Naming conventions:
- `CreateBookCommand`
- `CreateBookCommandHandler`
- `GetBooksQuery`
- `GetBooksQueryHandler`
- `ReturnLoanCommand`
- `ReturnLoanCommandHandler`

## 5. Validation convention

Validation will live in the **Application layer**, close to the corresponding use case.

Decision:
- use `FluentValidation`
- one validator per command/query when input validation is needed
- controllers stay thin and do not duplicate business validation

Naming convention:
- `CreateBookCommandValidator`
- `UpdateMemberCommandValidator`
- `CreateLoanCommandValidator`

Validation categories:
- shape/input validation: required fields, ranges, formats
- business rules: handled by domain/application logic, not only validators

## 6. Error handling convention

The solution will use **typed exceptions + centralized API exception mapping**.

Decision:
- domain rule violations use explicit typed exceptions derived from `DomainException`
- not-found situations use explicit typed exceptions
- validation failures are returned consistently from the API boundary
- unexpected exceptions are mapped to a generic 500 response

The API layer maps errors centrally through a global exception handler to problem-style JSON responses.

Current implemented mapping categories:
- `ValidationException` -> `400 Bad Request`
- `NotFoundException` -> `404 Not Found`
- `ConflictException` -> `409 Conflict`
- `DomainValidationException` -> `400 Bad Request`
- domain rule violations -> `409 Conflict`
- unexpected exceptions -> `500 Internal Server Error`

## 7. Pagination and listing convention

All list endpoints will use the same request and response shape.

Request contract:
- `page`: 1-based index
- `pageSize`: requested page size

Defaults:
- `page = 1`
- `pageSize = 20`
- `maxPageSize = 100`

Response contract:
- `items`
- `page`
- `pageSize`
- `totalCount`
- `totalPages`
- `hasPreviousPage`
- `hasNextPage`

Filtering and search are feature-specific on top of this common pagination shape.

Implemented feature-specific filter contracts:
- `BookListFilter`: free-text search, title, author, ISBN, publication year range, availability, and sorting
- `MemberListFilter`: free-text search, full name, email, active/inactive status, and sorting
- `LoanListFilter`: book id, member id, effective loan status, loan/due date ranges, free-text search across related data, and sorting

Repository contracts in Application:
- `IBookRepository`
- `IMemberRepository`
- `ILoanRepository`
- `IUnitOfWork`

These live in the Application layer so Infrastructure can implement them without reversing dependencies.


## 7.1 Sorting convention

List endpoints support a lightweight sorting contract:
- `sortBy`: feature-specific field name
- `sortDirection`: `Asc` or `Desc`

Examples:
- books: `title`, `author`, `publicationYear`, `isbn`, `availableCopies`, `createdAt`, `updatedAt`
- members: `fullName`, `email`, `createdAt`, `updatedAt`
- loans: `loanDate`, `dueDate`, `returnedAt`, `status`, `createdAt`, `updatedAt`

Sorting is validated in the Application layer and executed in Infrastructure repositories.

## 8. Auditing and time convention

All persisted entities will use UTC timestamps.

Naming convention:
- `CreatedAtUtc`
- `UpdatedAtUtc`

Rules:
- timestamps are set in UTC only
- `CreatedAtUtc` is set once when the entity is created
- `UpdatedAtUtc` is refreshed on mutation

## 9. Identifier convention

Primary keys will use `Guid`.

Rules:
- entity ids are generated in application/domain code unless infrastructure requires otherwise
- public API routes will expose ids as GUIDs

## 10. API convention

REST-style endpoint naming:
- `GET /api/books`
- `GET /api/books/{id}`
- `POST /api/books`
- `PUT /api/books/{id}`
- `DELETE /api/books/{id}`
- `POST /api/loans/{id}/return`

Implemented so far in Api:
- `BooksController` with CRUD endpoints for books
- `MembersController` with CRUD endpoints for members
- `LoansController` with create, get, list, and return endpoints for loans
- query models for paging and filtering on all list endpoints

Response behavior:
- `200 OK` for successful reads and updates
- `201 Created` for successful creates
- `204 No Content` for successful deletes where appropriate
- `400 Bad Request` for invalid input
- `404 Not Found` for missing resources
- `409 Conflict` for business rule violations when appropriate


## 11. Naming and code style

General conventions:
- one public type per file
- file name matches type name
- namespaces mirror folder structure
- `Async` suffix for async methods
- avoid generic `Helper`, `Manager`, `Service` names unless they are truly cross-cutting

Logging conventions:
- use structured templates
- include key identifiers, not opaque text blobs
- avoid logging full payloads unless clearly safe and useful

## 12. Testing convention

### Unit tests
Focus on:
- domain rules
- validators
- handlers

Naming:
- `MethodName_Should_DoSomething_When_Condition`

### Integration tests
Focus on:
- HTTP behavior
- persistence behavior
- end-to-end business rules

Tests must use isolated infrastructure and must never depend on a developer's local database.

## 13. Phase 1 exit criteria

Phase 1 is considered complete when:
- the solution skeleton exists
- project references are correct
- the API can boot with basic DI wiring
- the core architectural conventions are documented
- the solution now has a stable foundation for domain and feature implementation


## 13. Domain modeling clarifications

### Books and copies
The initial spec includes both inventory counters (`TotalCopies`, `AvailableCopies`) and a stricter sentence about a single active loan for a book.

To keep the current domain coherent in the first implementation, the model uses these rules:
- `Book` is a catalog entry, not a physical copy entity
- lending availability is driven by `AvailableCopies`
- a borrow operation decrements `AvailableCopies`
- a return operation increments `AvailableCopies`
- the stricter “single active loan” interpretation is postponed unless the model is later expanded with `BookCopy`

This is the most consistent interpretation of the current spec without over-modeling the problem too early.

### Cross-aggregate loan rules
Some loan rules involve both `Book` and `Member`. For those, the domain currently uses a small domain service (`LoanDomainService`) instead of pushing orchestration into controllers or infrastructure.

This allows the project to keep:
- entity invariants inside entities
- orchestration of multi-entity domain behavior inside the domain layer
- infrastructure concerns out of business rule execution

### Domain exception hierarchy
The domain exposes a small typed exception hierarchy so that later layers can map business failures consistently.

Base type:
- `DomainException`

Current specializations:
- `DomainValidationException` for invalid entity state/input at domain level
- `BookHasNoAvailableCopiesException`
- `BookInventoryOverflowException`
- `InactiveMemberCannotBorrowException`
- `LoanAlreadyReturnedException`
- `InvalidLoanDateRangeException`
- `InvalidLoanReturnDateException`

This keeps domain failures explicit and avoids scattering generic `ArgumentException` / `InvalidOperationException` usage across the model.


## 13. Contract and DTO convention

The Application layer owns feature contracts that sit between the HTTP boundary and the domain model.

Rules:
- request contracts describe external input shape
- response DTOs describe external output shape
- domain entities are not returned directly from API endpoints
- mapping from domain entities to response DTOs should be explicit and local to the feature

Current contract folders:
- `Features/Books/Contracts`
- `Features/Members/Contracts`
- `Features/Loans/Contracts`

A common `ErrorResponse` contract is also defined so the API layer can later return a predictable error shape.


## 13. Handler registration and validation execution

Because the solution currently uses custom CQRS abstractions instead of MediatR, handlers and validators are registered directly from the Application assembly.

Current convention:
- handlers are registered through reflection in `AddApplication()`
- validators are registered with FluentValidation's DI extensions
- handlers invoke their validator explicitly before performing repository or domain work

This keeps the implementation straightforward until a dedicated pipeline behavior or mediator abstraction is introduced later.

## 14. Book feature slice status

Task 8 implements the full Books application slice:
- create book
- get book by id
- get paged books list
- update book
- delete book

Books currently use feature-specific application exceptions for missing resources and duplicate ISBN conflicts.


## 13. Application slice status

Implemented so far in Application:
- Books slice: CRUD-style commands/queries, validators, handlers, and feature exceptions
- Members slice: CRUD-style commands/queries, validators, handlers, and feature exceptions

This keeps the feature implementation style consistent before Infrastructure and API are added.

- Loan use cases follow the same vertical-slice pattern as Books and Members: commands/queries, validators, handlers, feature exceptions, and mappings.


## 13. Persistence baseline

Task 11 establishes the first Infrastructure persistence baseline:
- `ApplicationDbContext` lives in `LibraryLending.Infrastructure/Persistence`
- it exposes `DbSet<Book>`, `DbSet<Member>`, and `DbSet<Loan>`
- it implements `IUnitOfWork` through EF Core
- the provider is PostgreSQL via `UseNpgsql(...)`
- entity configurations will be added separately and discovered with `ApplyConfigurationsFromAssembly(...)`

This keeps the current step intentionally narrow: create the DbContext and provider wiring first, then add mapping/configuration details in the next task.


## 12. Migration convention

EF Core migrations live in `LibraryLending.Infrastructure/Persistence/Migrations`.

Conventions:
- migrations are committed to source control
- the Infrastructure project is the migrations assembly
- `ApplicationDbContextFactory` supports design-time `dotnet ef` commands
- schema changes should be introduced through new migrations, not by editing old migration history after it is shared


## Database initialization and seed conventions

- Database initialization is handled in Infrastructure, not in controllers or handlers.
- Local development can apply migrations and seed data automatically on startup through configuration.
- Seed execution must be idempotent: if the database already contains data, the seed step must skip cleanly.
- Seed data should be deterministic and realistic enough to support manual review in Swagger and upcoming integration tests.
- The seed set currently targets at least:
  - 100+ books
  - a representative member set
  - active, overdue, and returned loans


## API composition root

The API project acts as the composition root for the solution. Service registration is centralized through `AddApiServices(...)`, while HTTP pipeline configuration is centralized through `UseApiPipeline()` and `MapApiEndpoints()`.

Current API conventions:
- MVC controllers are used for HTTP endpoints.
- Controllers inherit from `BaseApiController`.
- JSON serializes enums as strings.
- Health checks are exposed at `/health`.
- Swagger/OpenAPI is enabled in Development.
- URLs are lowercase by convention.


### API controller convention progress

Implemented controllers now follow the same pattern:
- `BooksController`
- `MembersController`

Each controller:
- stays thin
- translates HTTP input into Application requests
- returns HTTP-native responses
- relies on global exception handling for error mapping


## Structured logging conventions

The solution uses structured logging rather than interpolated free-text messages.

Guidelines:
- command handlers log successful state-changing operations at `Information` level
- read-only queries log at `Debug` level to reduce noise
- request pipeline logs method, path, status code, and duration
- validation failures are logged as handled warnings with error details
- unexpected exceptions are logged as errors
- message templates use named placeholders such as `{BookId}`, `{MemberId}`, `{LoanId}`, `{StatusCode}`

The API startup is configured for JSON console output so logs are machine-friendly out of the box.


## Domain testing conventions

Domain unit tests live in `tests/LibraryLending.UnitTests` and should test entities and domain services directly, without EF Core, controllers, or HTTP infrastructure.

Guidelines:
- prefer deterministic dates over `DateTime.UtcNow` in tests
- assert typed domain exceptions for rule violations
- cover both happy paths and invariant-breaking paths
- keep domain tests fast and isolated so they can be used as the first safety net for business rules


## Testing note

Task 24 adds unit tests around the main FluentValidation-based command validators so validation rules can be verified independently from handlers, repositories, and the HTTP layer.


## 11. Testing convention

Unit tests are layered by responsibility:
- Domain tests validate entity rules and domain service behavior in isolation
- Validator tests verify request shape and input rules
- Handler tests verify Application-layer orchestration using lightweight in-memory test doubles

Task 25 extends the unit test suite with handler-level tests for key command flows. These tests avoid EF Core and HTTP concerns and focus on:
- repository lookups
- duplicate/not-found handling
- unit of work persistence calls
- domain mutations triggered by handlers


## 12. Integration testing convention

Integration tests use the real ASP.NET Core host plus PostgreSQL-backed persistence.

Current Task 26 baseline:
- a custom `WebApplicationFactory` runs the API in a dedicated `Testing` environment
- the integration suite starts an isolated PostgreSQL container
- EF Core migrations are applied before tests run
- Respawn resets the database back to a clean state between tests
- shared helpers provide seeded scenarios and direct `ApplicationDbContext` execution for assertions

This keeps the integration tests close to production behavior while avoiding reliance on the development or production databases.


Task 27 extends the integration suite with end-to-end Books API coverage for:
- create book
- get book by id
- update book
- delete book
- duplicate ISBN conflicts
- not-found behavior for missing resources
- list behavior with search, filtering, sorting, and paging


Task 28 adds end-to-end integration coverage for the Members API, including CRUD flows, duplicate email conflicts, not-found handling, and list filtering for active/inactive members.


Task 29 adds end-to-end integration coverage for the Loans API, including create loan, return loan, missing loan handling, inventory and inactive-member conflicts, and list filtering for active, returned, and overdue loans.
