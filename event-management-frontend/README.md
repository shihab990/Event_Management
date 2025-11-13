# Frontend overview

The Angular app in this folder powers the public event list and the small admin console. It builds with Angular CLI 20, but in day-to-day work you usually interact with it through Docker where the production build is copied into an Nginx image.

The frontend automatically talks to `https://localhost:5001/api` whenever it detects that it is being served from `localhost`. If you ever need a different target while running a standalone build (e.g., serving `dist/` from another port), you can set `window.__API_BASE_URL__` in a tiny script tag before Angular boots to point at the desired API root. When the app is hosted on any other domain, it defaults to calling `/api`, which is how the dockerized Nginx proxy forwards requests to the backend container.

## Local development

If you want to run the SPA without Docker:

```bash
npm install
npm start
```

That launches the dev server with HTTPS on `https://localhost:4200`. The app talks to the backend at `https://localhost:5001/api`, matching the default ASP.NET Core dev settings.

## Production build

```bash
npm run build
```

The compiled assets end up in `dist/event-management-frontend/browser/`. Docker uses the same command during `docker compose up --build` and then serves the result through Nginx.
