# Backend overview

The backend is a .NET 9 Web API that follows a small clean-architecture layout (Domain, Application, Infrastructure, WebApi). It uses SQLite through EF Core with real migrations, hashes passwords with PBKDF2, and issues JWTs so administrators can create and manage events.

When the API starts it runs migrations, seeds the default admin account (`admin / Admin@123`), and exposes Swagger at `https://localhost:5001/swagger`. CORS is limited to the Angular client on `https://localhost:4200`, which mirrors the dockerized setup.

## Running the API locally

```bash
dotnet dev-certs https --trust
cd WebApi
dotnet restore
dotnet run
```

The SQLite database lives in `WebApi/Data/events.db` and is created automatically.
