# Contributing

Thanks for taking a look at this portfolio project.

## Development workflow

1. Fork the repository and create a feature branch.
2. Run `dotnet restore` and `dotnet build`.
3. Keep changes focused and consistent with the existing MVC/repository structure.
4. Update documentation and migrations when behaviour or the data model changes.
5. Open a pull request describing the change and how you verified it.

## Code style

- Nullable reference types are enabled; initialise non-nullable strings.
- Prefer async EF Core APIs (`ToListAsync`, `FirstOrDefaultAsync`, …).
- Keep controllers thin; place reusable logic in services or repositories.
- Use `[ValidateAntiForgeryToken]` on every POST action.
- Validate uploads through `IImageUploadService`.
- Use the configured `ILogger<T>`/Serilog instead of `Console.WriteLine`.

## Database changes

Create a migration with the Infrastructure project as the migrations project and the web project as startup:

```bash
dotnet ef migrations add <Name> --project Portfolio.Infrastructure --startup-project portfolio.web
```

Commit the migration and snapshot together.

## Commit messages

Use short conventional prefixes:

- `feat:` new behaviour
- `fix:` bug fixes
- `refactor:` internal changes
- `docs:` documentation
- `chore:` tooling/configuration
