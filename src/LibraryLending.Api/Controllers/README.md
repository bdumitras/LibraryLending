# API Controllers

Controllers stay thin and delegate work to Application-layer handlers.

Implemented so far:
- `BooksController`
- `MembersController`
- `LoansController`

Rules:
- no business rules inside controllers
- no direct EF Core usage inside controllers
- translate HTTP input to application requests
- return consistent HTTP responses
- rely on centralized exception handling for error mapping


List endpoints support feature-specific filtering, search, paging, and sorting through dedicated query models that map cleanly into Application-layer filter contracts.
