# Event Management – Docker Notes

This repository holds two applications that works together: an ASP.NET Core Web API inside `event-management-backend` and an Angular client that ships through Nginx in `event-management-frontend`. Everything is wired with `docker-compose.yml`, so once Docker Desktop is running you can build the pair from the workspace root with:

```bash
docker compose up --build
```

Docker builds the backend (restore, publish, runtime image) and the frontend (production Angular build dropped into Nginx). The default publish map exposes the API on `https://localhost:5001` and the UI on `https://localhost:4200`. When Nginx receives a request for `/api`, it forwards it to the backend container on port 8080, so the browser does not need direct access to `5001`.

## Getting ready

Before the first run you only need three tools: Git, Node/npm (if you ever want to build the Angular app outside Docker), and Docker Desktop 4.24 or later. Clone the repo, keep the default folder structure, and stay in the project root when running compose commands.

The containers rely on the local development certificate in `certs/`. Trust `certs/local-dev-ca.pem` on your machine once (Keychain on macOS, certutil on Windows, `update-ca-certificates` on Linux) so the browser stops warning about HTTPS.

## Daily workflow

- Build and start everything: `docker compose up --build` (add `-d` to detach).
- Stop containers: hit `Ctrl+C` in the compose terminal or run `docker compose down`.
- Rebuild only after code changes: rerun `docker compose up --build`; Docker will reuse cached layers when possible.
- Inspect logs: `docker compose logs -f backend` or `docker compose logs -f frontend`.
- Clean up volumes and images if you want a fresh start: `docker compose down --volumes --rmi local`.

Swagger is always available at `https://localhost:5001/swagger`, and the seeded admin user is `admin / Admin@123`. Event data lives inside the `backend-data` named volume, so the SQLite file survives container restarts.

## Testing quick reference

If you do not have the .NET SDK installed locally, you can still run the backend tests inside the SDK container:

```bash
docker run --rm \
  -v "$(pwd)/event-management-backend:/src" \
  -w /src mcr.microsoft.com/dotnet/sdk:9.0 \
  dotnet test event-management-backend.sln
```

Swap the mount syntax for PowerShell if you are on Windows. Use `--filter` to target a specific test project.

## Tweaking ports or URLs

Internally the backend listens on 8080/8443 and the frontend listens on 80/443. The `ports` entries in `docker-compose.yml` publish those to 5001 and 4200 on the host. Change the mappings there if you need different host ports, and adjust the Angular base URL logic in `src/app/core/api.service.ts` only if you move off the `localhost:5001` default.

That is all you need. Start Docker, run `docker compose up --build`, wait for the two containers to report running fine, and open `https://localhost:4200` to manage events.
