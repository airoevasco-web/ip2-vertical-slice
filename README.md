# Vertical Slice Architecture — Starter Project

A starter API for building features with Vertical Slice Architecture in .NET 10, made for educational use.

This project is **intentionally empty**. It gives you the plumbing (result pattern, CQRS interfaces,
database context, API docs), and you build the entities and features yourself.

---

## Tech Stack

| Component      | Technology                          |
| -------------- | ----------------------------------- |
| Framework      | .NET 10                             |
| Architecture   | Vertical Slice + Clean Architecture |
| ORM            | EF Core 10                          |
| Database       | InMemory (for demo)                 |
| Validation     | FluentValidation                    |
| CQRS           | Custom ICommand/IQuery (No MediatR) |
| API Docs       | Scalar (No Swashbuckle)             |
| Result Pattern | Domain.Common.Result\<T\>           |

---

## Project Structure — What's Provided

```
Midterm-Project-LastName/
├── VerticalSlice.slnx
├── Domain/                          # Entities (ZERO dependencies)
│   ├── Common/
│   │   ├── BaseEntity.cs           # Base class with Guid Id
│   │   └── Result.cs               # Result<T>, Result, and Error
│   └── Entities/                   # ← your entities go here
├── Application/                     # Handlers, Validators, Abstractions
│   ├── Abstractions/
│   │   ├── Data/
│   │   │   └── IAppDbContext.cs    # Database abstraction (add DbSets here)
│   │   └── Messaging/
│   │       ├── ICommand.cs         # Write operations interface
│   │       ├── ICommandHandler.cs  # Command handler interface
│   │       ├── IQuery.cs           # Read operations interface
│   │       └── IQueryHandler.cs    # Query handler interface
│   └── Features/                   # ← your feature slices go here
├── Infrastructure/                  # EF Core, Persistence
│   └── Persistence/
│       └── AppDbContext.cs         # EF Core context (implement DbSets + configuration)
└── Api/                             # Endpoints, DI, Extensions
    ├── Common/
    │   └── Extensions/
    │       └── ResultExtensions.cs # Converts a failed Result into ProblemDetails
    ├── Features/                   # ← your endpoint groups go here
    └── Program.cs                  # DI registrations, seed data, endpoint mapping
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- IDE: Visual Studio 2022, VS Code with C# extension, or JetBrains Rider

### Run the starter

1. Open a terminal in the project folder (the one containing `VerticalSlice.slnx`).

2. Restore and build:

   ```bash
   dotnet restore
   dotnet build
   ```

3. Run the API:

   ```bash
   dotnet run --project Api
   ```

4. Open Scalar in your browser:

   ```
   http://localhost:5205/scalar/v1
   ```

On a fresh starter, Scalar shows no endpoints yet. That's expected. Opening
`http://localhost:5205/` should show the welcome message.

> Stop the running API (`Ctrl+C`) before running `dotnet build` again. On Windows, a running API
> locks its files and the build will fail.

---

## How to Add a Feature — Checklist

Each feature (e.g. _CreateStudent_, _GetStudentById_) is one **vertical slice**: its own folder
with everything it needs. Follow these steps in order. `{Entity}` is your entity name (e.g.
`Student`) and `{Entities}` is its plural (e.g. `Students`).

### 1. Domain — create the entity (once per entity)

`Domain/Entities/{Entity}.cs`

```csharp
namespace Domain.Entities;

using Domain.Common;

public sealed class {Entity} : BaseEntity
{
    public string Name { get; set; } = default!;
    // other properties...
}
```

### 2. Persistence — expose the entity to the database (once per entity)

In `Application/Abstractions/Data/IAppDbContext.cs`:

```csharp
DbSet<{Entity}> {Entities} { get; }
```

In `Infrastructure/Persistence/AppDbContext.cs`:

```csharp
public DbSet<{Entity}> {Entities} => Set<{Entity}>();
```

and configure it inside `OnModelCreating`:

```csharp
modelBuilder.Entity<{Entity}>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
});
```

Add `using Domain.Entities;` to both files.

### 3. Application — create the slice folder

`Application/Features/{Entities}/{FeatureName}/` containing:

| File                                                               | Purpose                                        |
| ------------------------------------------------------------------ | ---------------------------------------------- |
| `{FeatureName}Command.cs` or `{FeatureName}Query.cs`               | The request record **and** its response record |
| `{FeatureName}CommandHandler.cs` or `{FeatureName}QueryHandler.cs` | The logic; returns `Result<TResponse>`         |
| `{FeatureName}Validator.cs`                                        | FluentValidation rules (commands only)         |

Command and response:

```csharp
public sealed record Create{Entity}Command(string Name) : ICommand<Result<Create{Entity}Response>>;

public sealed record Create{Entity}Response(Guid Id, string Name);
```

Handler:

```csharp
public sealed class Create{Entity}CommandHandler(IAppDbContext dbContext)
    : ICommandHandler<Create{Entity}Command, Result<Create{Entity}Response>>
{
    public async Task<Result<Create{Entity}Response>> HandleAsync(
        Create{Entity}Command command,
        CancellationToken cancellationToken = default)
    {
        var entity = new {Entity} { Name = command.Name };

        dbContext.{Entities}.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<Create{Entity}Response>.Success(
            new Create{Entity}Response(entity.Id, entity.Name));
    }
}
```

### 4. Api — add the endpoint

`Api/Features/{Entities}/{Entity}Endpoints.cs`: one static class per entity, with a
`Map{Entity}Endpoints` extension method that creates a route group and maps each feature:

```csharp
public static class {Entity}Endpoints
{
    public static void Map{Entity}Endpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/{entities}")
            .WithTags("{Entities}");

        group.MapPost("/", Create)
            .WithName("Create{Entity}")
            .WithSummary("Create a new {entity}");
    }

    private static async Task<IResult> Create(
        Create{Entity}Command command,
        ICommandHandler<Create{Entity}Command, Result<Create{Entity}Response>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ToProblemDetails();
    }
}
```

### 5. Program.cs — register and map

```csharp
// Handlers
builder.Services.AddScoped<ICommandHandler<Create{Entity}Command, Result<Create{Entity}Response>>, Create{Entity}CommandHandler>();

// Map endpoints
app.Map{Entity}Endpoints();
```

Every handler needs its own registration line. If you forget one, the endpoint fails at runtime
with a "service not registered" error.

### 6. Build, run, and test in Scalar

```bash
dotnet build
dotnet run --project Api
```

Then send test requests from `http://localhost:5205/scalar/v1`.

---

## Error Handling

Handlers never throw for business errors. They return a failed `Result` instead:

```csharp
return Result<T>.Failure(Error.NotFound("{Entity}.NotFound", "{Entity} not found."));
```

`ResultExtensions.ToProblemDetails()` picks the HTTP status code from the **error code text**:

| Error code contains | HTTP status               |
| ------------------- | ------------------------- |
| `NotFound`          | 404 Not Found             |
| `Validation`        | 400 Bad Request           |
| `Conflict`          | 409 Conflict              |
| anything else       | 500 Internal Server Error |

Name your error codes so they match the status you intend.

---

## Architecture Rules

1. **Domain has ZERO external dependencies**
2. **All data access through DbContext** — No repository pattern
3. **Result\<T\> for business errors** — Not exceptions
4. **Records for DTOs** — Primary constructors for DI
5. **Always pass CancellationToken** through async chains

---

## Key Concepts

### Vertical Slice Architecture

Code is organized by feature (CreateStudent, GetStudentById), not by technical layer (Controllers,
Services, Repositories). Each feature is self-contained in its own folder.

### Custom CQRS

Simple interfaces without external dependencies:

- `ICommand<TResponse>` — for write operations (create, update, delete)
- `IQuery<TResponse>` — for read operations
- `ICommandHandler<TCommand, TResponse>` — handles commands
- `IQueryHandler<TQuery, TResponse>` — handles queries

### FluentValidation

Validation rules live in their own class next to the command:

```csharp
public sealed class Create{Entity}Validator : AbstractValidator<Create{Entity}Command>
{
    public Create{Entity}Validator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
```

`Program.cs` already registers every validator in the Application project, so you don't need a
registration line for each one.

### Seed Data

The database is in-memory, so it is **empty every time the API starts** and anything you created
is lost on restart. To start with sample records, add them in the `// Seed data` block of
`Program.cs`:

```csharp
dbContext.{Entities}.AddRange(
    new Domain.Entities.{Entity} { Id = Guid.NewGuid(), Name = "Sample" }
);
```

Seeded IDs are generated fresh on every run. Get the current IDs from your GetAll endpoint after
each restart.

---

## Common Mistakes to Avoid

1. **Don't use MediatR** — This project uses custom ICommand/IQuery
2. **Don't throw exceptions** — Return Result.Failure() instead
3. **Don't skip validation** — Always add a validator
4. **Don't forget to register** — Every handler needs an `AddScoped` line, and every endpoint group needs `app.Map{Entity}Endpoints()` in Program.cs
   on't use MediatR\*\* — This project uses custom ICommand/IQuery
5. **Don't throw exceptions** — Return Result.Failure() instead
6. **Don't skip validation** — Always add a validator
7. **Don't forget to register** — Add `app.Map{FeatureGroup}Endpoints()` in Program.cs
8. **Don't put business logic in endpoints** — Handlers do the work
9. **Don't use controllers** — This project uses Minimal APIs
10. **Don't use Repository pattern** — Use DbContext directly

---

## Resources

- [Vertical Slice Architecture by Jimmy Bogard](https://www.jimmybogard.com/vertical-slice-architecture/)
- [CodeWithMukesh Clean Architecture Template](https://github.com/iammukeshm/CleanArchitecture.WebApi)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [Minimal APIs in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Scalar API Documentation](https://scalar.com/)

---

## License

This project is for educational purposes.

---

**Developed by Elvin Manuel R. Luces, MIT**
