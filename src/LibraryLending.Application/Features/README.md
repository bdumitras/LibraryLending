# Features Folder Convention

Feature work will be organized by vertical slice.

Example:

```text
Features/
└── Books/
    ├── Commands/
    │   └── CreateBook/
    │       ├── CreateBookCommand.cs
    │       ├── CreateBookCommandValidator.cs
    │       └── CreateBookCommandHandler.cs
    └── Queries/
        └── GetBooks/
            ├── GetBooksQuery.cs
            └── GetBooksQueryHandler.cs
```

The same convention will be used for `Members` and `Loans`.
