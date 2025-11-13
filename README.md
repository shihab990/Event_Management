# Event Management – Docker Notes

This repository holds two applications that works together: an ASP.NET Core Web API inside `event-management-backend` and an Angular client that ships through Nginx in `event-management-frontend`. Everything is wired with `docker-compose.yml`, so once Docker Desktop is running you can build the pair from the workspace root with:

```bash
docker compose up --build
```

Docker builds the backend (restore, publish, runtime image) and the frontend (production Angular build dropped into Nginx). The default publish map exposes the API on `https://localhost:5001` and the UI on `https://localhost:4200`. When Nginx receives a request for `/api`, it forwards it to the backend container on port 8080, so the browser does not need direct access to `5001`.

## Getting ready

Before the first run you only need three tools: Git, Node/npm (if you ever want to build the Angular app outside Docker), and Docker Desktop 4.24 or later. Clone the repo, keep the default folder structure, and stay in the project root when running compose commands.

### Generate local TLS certificates

The compose file bind-mounts `certs/localhost.pfx`, `certs/localhost.crt`, and `certs/localhost.key` into the containers. Because `certs/` is gitignored you must create those files locally **before** running Docker, otherwise Docker will create placeholder directories and fail with “are you trying to mount a directory onto a file?”.

Run the helper once after cloning:

```bash
./scripts/generate-dev-certs.sh --force
```

Add `--force` if you need to overwrite existing files, or pass `ASPNETCORE_HTTPS_CERT_PASSWORD=<value>` to change the PFX password. The script also drops `certs/local-dev-ca.pem`; trust that file on your machine (Keychain on macOS, certutil on Windows, `update-ca-certificates` on Linux) so browsers accept the HTTPS endpoints without warnings.

### Backend data directory

Docker bind-mounts `event-management-backend/WebApi/Data` into the backend container so the SQLite file is available on the host. The folder is checked in with a `.gitkeep` placeholder but remains ignored otherwise, so you can inspect or delete the data locally without touching Git history.

#### Trust the certificate authority

Chrome and other browsers only accept the self-signed certificates after the CA file is trusted. Import `certs/local-dev-ca.pem` once per machine:

- **macOS**: `sudo security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain certs/local-dev-ca.pem`
- **Windows**: Run `certmgr.msc`, right-click **Trusted Root Certification Authorities > Certificates**, choose **All Tasks > Import**, and select `certs\local-dev-ca.pem`.
- **Linux** (Debian/Ubuntu): `sudo cp certs/local-dev-ca.pem /usr/local/share/ca-certificates/event-management-local-dev-ca.crt && sudo update-ca-certificates`

Restart the browser if it was open. If you regenerate the certificates later (e.g., via `--force`), re-import the new CA file.

## Daily workflow

- Build and start everything: `docker compose up --build` (add `-d` to detach).
- Stop containers: hit `Ctrl+C` in the compose terminal or run `docker compose down`.
- Rebuild only after code changes: rerun `docker compose up --build`; Docker will reuse cached layers when possible.
- Inspect logs: `docker compose logs -f backend` or `docker compose logs -f frontend`.
- Clean up volumes and images if you want a fresh start: `docker compose down --volumes --rmi local`.

Swagger is always available at `https://localhost:5001/swagger`, the seeded admin user: `admin` and password: `Admin@123`. Event data lives inside `event-management-backend/WebApi/Data`, so the SQLite file survives container restarts and stays next to the backend project.

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
