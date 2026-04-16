# Submission checklist

This document is a final delivery checklist for the LibraryLending challenge repository.

## Core challenge requirements

- [x] .NET 9 solution
- [x] Clean Architecture project structure
- [x] CRUD API for relevant entities
- [x] Consistent validation strategy
- [x] Business rules enforced in the domain/application flow
- [x] EF Core ORM with relational persistence
- [x] Migrations included in the repository
- [x] Initial seed data included
- [x] Filtering, search, sorting, and paging on list endpoints
- [x] Structured logging
- [x] Unit tests
- [x] Integration tests isolated from development/production database
- [x] README with setup and run instructions
- [x] Works cross-platform in a standard .NET + PostgreSQL setup

## Manual verification steps before publishing

1. Run `dotnet restore`
2. Run `dotnet build`
3. Run `dotnet test`
4. Run `dotnet ef database update --project src/LibraryLending.Infrastructure --startup-project src/LibraryLending.Api`
5. Run `dotnet run --project src/LibraryLending.Api --launch-profile LibraryLending.Api`
6. Open Swagger and verify:
   - books endpoints
   - members endpoints
   - loans endpoints
   - `/health`
7. Confirm development seed data loads as expected
8. Confirm integration tests run with Docker available

## Notes

- The implementation models lending availability through `AvailableCopies`, not through a separate `BookCopy` entity.
- Integration tests use PostgreSQL via Testcontainers and therefore require Docker on the machine where tests are executed.
