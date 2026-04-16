# Spec: Library Lending CRUD API

## 1. Overview

This document defines the specification for a **scalable CRUD API** built with **.NET 9** and **Clean Architecture**. The API manages a small library lending domain and is designed to satisfy the requirements of the technical challenge while remaining realistic, extensible, and production-minded.

The chosen domain is **Library Lending** because it supports:
- clear CRUD operations,
- meaningful domain rules,
- filtering and search,
- realistic validation,
- data seeding,
- unit and integration testing,
- clean separation of business rules from infrastructure concerns.

The API will allow clients to manage **Books**, **Members**, and **Loans**.

---

## 2. Outcome

When this work is complete, the solution should provide the following outcomes:

1. A working **REST API** built on **.NET 9**.
2. The solution follows **Clean Architecture** with a clear separation between:
   - Domain
   - Application
   - Infrastructure
   - API / Presentation
3. Validation is handled consistently across commands, queries, and API input.
4. Domain rules are explicitly enforced in the system.
5. The persistence layer uses **EF Core** with migrations included in the repository.
6. The application ships with a usable database seed containing **at least 100 records**.
7. The API supports CRUD, filtering, and search for the core entities.
8. The system produces **structured logs** for relevant operations and errors.
9. The codebase includes both **unit tests** and **integration tests**, with test isolation from development and production data.
10. The repository includes a clear **README** with setup and run instructions for Mac and Windows.

Success means a reviewer can clone the repository, run the app, apply migrations, seed data, execute tests, and interact with the API without needing extra explanation.

---

## 3. Scope

### In scope

The first version will include the following capabilities.

#### Entities
- **Book**
- **Member**
- **Loan**

#### Core features
- Create, read, update, delete books
- Create, read, update, delete members
- Create and return loans
- Read loan history and active loans
- Search books by title / author / ISBN
- Filter books by availability and publication year
- Filter members by status
- Filter loans by active / returned / overdue status

#### Cross-cutting concerns
- Clean Architecture
- Consistent validation
- Structured logging
- Centralized exception handling
- Database migrations
- Seed data
- Unit tests
- Integration tests

#### Documentation
- README
- API endpoint summary
- Local setup instructions

### Explicitly out of scope

To keep the implementation focused and high quality, the following are out of scope for this challenge:
- Authentication and authorization
- UI / frontend
- Email notifications
- Background jobs
- Distributed caching
- Message brokers
- Rate limiting
- Multi-tenant support
- Soft deletes for all entities
- File uploads
- Advanced reporting dashboards

These can be added later, but they are not required to meet the challenge successfully.

---

## 4. Constraints

### Technical constraints
- Target framework: **.NET 9**
- Architecture: **Clean Architecture**
- ORM: **EF Core**
- Relational database: **PostgreSQL** or **SQL Server**
  - For local simplicity, PostgreSQL is preferred
- Migrations must be committed to the repo
- The solution must run on **Mac and Windows**
- The solution must be executable with standard local development tooling

### Quality constraints
- Validation must be applied consistently, not ad hoc in controllers only
- Business rules must live in the domain/application boundary, not only in the database
- Logging should be structured and useful for debugging and tracing
- Tests must not depend on the development or production database
- Seed data must be deterministic enough to make the app usable immediately

### Domain constraints
- A **book cannot have more than one active loan at the same time**
- A **member cannot borrow a book if marked inactive**
- A **loan cannot be returned twice**
- A **book ISBN must be unique**
- A **member email must be unique**
- A **loan due date must be after loan date**

These rules must be enforced in code and, where appropriate, backed by database constraints.

---

## 5. Decisions Already Made

### 5.1 Domain choice
The implementation will use a **Library Lending** domain.

Reason:
- Simple enough for a challenge
- Rich enough to demonstrate business rules
- Easy to explain during review
- Supports CRUD, filtering, search, and tests naturally

### 5.2 Architectural style
The solution will use **Clean Architecture**.

Planned project structure:
- `LibraryLending.Domain`
- `LibraryLending.Application`
- `LibraryLending.Infrastructure`
- `LibraryLending.Api`
- `LibraryLending.UnitTests`
- `LibraryLending.IntegrationTests`

Responsibilities:
- **Domain**: entities, value objects, domain rules, domain exceptions
- **Application**: use cases, DTOs, interfaces, validation, command/query handlers
- **Infrastructure**: EF Core, repositories, database configuration, logging integrations
- **API**: controllers or minimal endpoints, exception middleware, DI wiring, OpenAPI

### 5.3 API style
The API will be a REST API using JSON.

Representative endpoints:
- `GET /api/books`
- `GET /api/books/{id}`
- `POST /api/books`
- `PUT /api/books/{id}`
- `DELETE /api/books/{id}`
- `GET /api/members`
- `POST /api/members`
- `PUT /api/members/{id}`
- `DELETE /api/members/{id}`
- `GET /api/loans`
- `POST /api/loans`
- `POST /api/loans/{id}/return`

### 5.4 Validation strategy
Validation will be handled in a centralized and consistent way.

Decision:
- Use **FluentValidation** in the Application layer
- Validate commands and queries before handler execution
- Keep controllers thin
- Use middleware / filters for consistent validation error responses

### 5.5 Error handling strategy
Decision:
- Use centralized exception handling middleware
- Return consistent error responses using RFC-style problem details where practical
- Distinguish validation failures, not found cases, domain rule violations, and unexpected server errors

### 5.6 Persistence strategy
Decision:
- Use **EF Core** with relational database support
- Use code-first migrations
- Use entity configurations via `IEntityTypeConfiguration<T>`
- Enforce unique constraints in the database for ISBN and member email

### 5.7 Logging strategy
Decision:
- Use built-in `ILogger<T>` with structured message templates
- Log key application events such as:
  - book created / updated / deleted
  - member created / updated / deleted
  - loan created
  - loan returned
  - validation failures
  - unhandled exceptions

Logs should include identifiers and important context without exposing sensitive information.

### 5.8 Testing strategy
Decision:
- **Unit tests** for domain rules, validators, and application handlers
- **Integration tests** for API endpoints and persistence behavior
- Integration tests will use an isolated test database setup
  - preferably Testcontainers + PostgreSQL
  - acceptable fallback: SQLite in-memory, if behavior differences are understood and documented

### 5.9 Seed data strategy
Decision:
- Seed at least:
  - 100 books
  - 20 members
  - a small set of active and returned loans
- Seeding should make filtering and search easy to demonstrate

---

## 6. Functional Requirements

### 6.1 Books
A book must contain:
- Id
- Title
- Author
- ISBN
- PublicationYear
- TotalCopies
- AvailableCopies
- CreatedAt
- UpdatedAt

Rules:
- Title is required
- Author is required
- ISBN is required and unique
- Publication year must be reasonable
- Total copies must be greater than 0
- Available copies cannot be negative or exceed total copies

### 6.2 Members
A member must contain:
- Id
- FullName
- Email
- IsActive
- CreatedAt
- UpdatedAt

Rules:
- Full name is required
- Email is required and unique
- Inactive members cannot borrow books

### 6.3 Loans
A loan must contain:
- Id
- BookId
- MemberId
- LoanDate
- DueDate
- ReturnedAt
- Status

Rules:
- Book and member must exist
- Due date must be after loan date
- A book cannot be actively loaned more than once at the same time
- A returned loan cannot be returned again

Derived behavior:
- Loan status can be interpreted as Active / Returned / Overdue

---

## 7. Non-Functional Requirements

### Performance
- Should handle normal CRUD workloads efficiently
- Query endpoints should support paging to avoid returning unbounded result sets

### Maintainability
- Business logic should be easy to test in isolation
- Infrastructure details should not leak into the Domain layer
- New entities or use cases should be easy to add

### Observability
- Important operations should produce structured logs
- Errors should be traceable through logs and consistent API responses

### Developer Experience
- One-command or straightforward startup flow
- Minimal setup friction for reviewers
- Clear README and environment instructions

---

## 8. Tasks

### Task 1 — Solution skeleton
- Create solution and projects
- Set up project references according to Clean Architecture
- Configure dependency injection

### Task 2 — Domain modeling
- Define entities and enums
- Define domain rules and domain exceptions
- Add base entity/auditing primitives if needed

### Task 3 — Application layer
- Add DTOs
- Add commands and queries
- Add handlers
- Add validation with FluentValidation
- Add interfaces for repositories and unit of work if used

### Task 4 — Infrastructure layer
- Configure EF Core DbContext
- Add entity configurations
- Add repositories or query services
- Add migrations
- Implement data seed

### Task 5 — API layer
- Expose endpoints
- Add request/response contracts
- Add exception handling middleware
- Add logging configuration
- Enable Swagger / OpenAPI

### Task 6 — Filtering, search, and pagination
- Add query parameters for search/filtering
- Add pagination to list endpoints
- Ensure queries are efficient and readable

### Task 7 — Testing
- Unit tests for:
  - validators
  - domain rules
  - command handlers
- Integration tests for:
  - books endpoints
  - members endpoints
  - loans endpoints
  - happy path and failure path scenarios

### Task 8 — Documentation
- Write README
- Document how to run migrations
- Document how to seed data
- Document how to run tests
- Add sample API usage examples

---

## 9. Checks

The implementation is considered correct when the following checks pass.

### Functional checks
- Can create, read, update, and delete books
- Can create, read, update, and delete members
- Can create a loan for an available book and active member
- Cannot create a second active loan for the same book
- Cannot create a loan for an inactive member
- Cannot return the same loan twice
- Search and filtering return expected results

### Validation checks
- Invalid requests return consistent validation errors
- Duplicate ISBN is rejected
- Duplicate member email is rejected
- Invalid date ranges are rejected

### Persistence checks
- Migrations apply successfully
- Seed data is available after startup or initialization
- Data is stored and retrieved correctly through EF Core

### Observability checks
- Relevant actions produce structured logs
- Unhandled exceptions are logged and returned in a safe format

### Test checks
- Unit tests pass
- Integration tests pass
- Tests do not depend on the development or production database

### Reviewer checks
- Repository builds successfully on Mac and Windows
- README is sufficient to run the project without extra guidance
- API is discoverable through Swagger / OpenAPI

---

## 10. Acceptance Criteria

The work will be accepted when all of the following are true:

1. The solution is implemented in **.NET 9 or later**.
2. The architecture clearly follows **Clean Architecture** principles.
3. Validation is centralized and consistently applied.
4. Domain rules are enforced both behaviorally and, where appropriate, by database constraints.
5. EF Core migrations are present in the repository.
6. At least 100 seeded records are available out of the box.
7. CRUD endpoints exist for the core resources.
8. Filtering and search are implemented.
9. Structured logging is in place.
10. Unit tests and integration tests are included and passing.
11. The solution can be run locally on Mac or Windows using the README.

---

## 11. Risks and Mitigations

### Risk: scope becomes too large
Mitigation:
- Keep the domain small and focused
- Avoid unnecessary enterprise abstractions

### Risk: overengineering the architecture
Mitigation:
- Use Clean Architecture pragmatically
- Introduce only abstractions that have a clear purpose

### Risk: integration tests become fragile
Mitigation:
- Use isolated test infrastructure
- Keep test setup deterministic

### Risk: seeding becomes noisy or unrealistic
Mitigation:
- Seed meaningful but simple data
- Make sure seeded records support demo scenarios

---

## 12. Future Extensions

Possible future improvements after the challenge:
- Authentication and authorization
- Reservation / hold functionality
- Fine calculation for overdue books
- Audit trail
- Docker support
- CI pipeline
- Health checks
- Metrics / tracing
- Caching

---

## 13. Summary

This specification defines a pragmatic, review-friendly solution for the challenge: a **Library Lending CRUD API** built with **.NET 9**, **Clean Architecture**, **EF Core**, consistent validation, meaningful domain rules, structured logging, seeding, and automated tests.

The goal is not only to satisfy the checklist, but to deliver a solution that looks coherent, intentional, and production-aware.
