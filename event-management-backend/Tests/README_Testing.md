# Testing notes

Run everything with:

```bash
dotnet test --logger "console;verbosity=normal"
```

If you only need a specific suite, point `dotnet test` at the project (for example `Tests/UnitTests` or `Tests/IntegrationTests`). Integration tests spin up the Web API with the custom test host, swap in the in-memory SQLite database, and bypass authentication through `TestAuthHandler`, so make sure the WebApi project still builds on .NET 9 before launching them.
