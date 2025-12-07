> Continuation of the original group project. Frozen version: [BrickBreaker-Group](https://github.com/lanavirsen/BrickBreaker-Extended)

# BrickBreaker – Extended Edition

![CI](https://github.com/lanavirsen/BrickBreaker-Extended/actions/workflows/ci.yml/badge.svg)  [![codecov](https://codecov.io/gh/lanavirsen/BrickBreaker-Extended/branch/main/graph/badge.svg)](https://codecov.io/gh/lanavirsen/BrickBreaker-Extended)

BrickBreaker is a .NET 9 WinForms remake of the classic paddle-and-bricks arcade game.  
The gameplay runs inside a desktop window, while Spectre.Console menus handle login, Quick Play, and leaderboard actions. Every frame, power-up, and collision is authored directly in code.

## Gameplay screenshot

<p align="center">
  <img src="docs/images/gameplay.png">
</p>

## Highlights

- **WinForms renderer @ 60 FPS** – `Form1` maximizes to a borderless window, locks the frame rate with a Windows Forms timer, and uses custom fonts, rainbow borders, and score pop-ups.
- **WinForms client shell** – `BrickBreaker.WinFormsClient` layers a login/register menu, Quick Play toggle, and leaderboard viewer on top of the renderer while talking to the ASP.NET API for auth + score submission.
- **Shared gameplay loop** – `BrickBreaker.Gameplay` wraps `GameEngine` into a reusable session + render-state model that both the WinForms and Blazor clients consume.
- **Engine features** – `GameEngine` drives multi-ball, paddle-extender power-ups, brick layouts, score multipliers, and ball tethering before launch so runs stay fair on a keyboard.
- **Spectre.Console shell** – `BrickBreaker.ConsoleClient` offers registration, login, best-score lookup, leaderboard browsing, Quick Play, and exit flows using a small state machine.
- **Blazor web client** – `BrickBreaker.WebClient` reuses the `GameEngine` inside a `<canvas>` via WebAssembly so the browser build stays feature-complete with the desktop renderer.
- **Supabase/PostgreSQL persistence** – When a connection string is available, credentials are hashed, scores are written through `BrickBreaker.Storage`, and the UI surfaces per-user best scores plus a Top-10 leaderboard. When offline, disabled stores keep the game playable and the console warns that persistence is unavailable.
- **Automated tests** – `BrickBreaker.Tests` uses xUnit to validate authentication, password hashing, and leaderboard ordering via the shared abstractions so logic stays correct regardless of the backing store.

## Project layout

```
BrickBreaker/
├── BrickBreaker.sln             Solution root (net9.0)
├── BrickBreaker.Gameplay/       Shared GameSession + render models (used by WinForms + Blazor)
├── BrickBreaker.ConsoleClient/  Spectre.Console client with terminal renderer + Supabase auth
├── BrickBreaker.WinFormsClient/ WinForms client (launcher + Form1 gameplay)
│   ├── Hosting/                 IGame implementation for desktop play
│   └── WinUI/                   WinForms forms, drawing, input, assets
├── BrickBreaker.WebClient/      Blazor WebAssembly canvas client for browsers
├── BrickBreaker.Core/           Domain models + services (Auth, Leaderboard, abstractions)
├── BrickBreaker.Storage/        Supabase/PostgreSQL stores + configuration helpers
│   ├── StorageConfiguration.cs  Resolves Supabase connection strings
│   ├── UserStore.cs             Npgsql-backed implementation
│   ├── LeaderboardStore.cs      Npgsql-backed implementation
│   └── Disabled*.cs             Null-object stores for offline play
├── BrickBreaker.Tests/          xUnit tests for Auth + Leaderboard logic
└── README.md
```

## Run the game locally

Prerequisites: .NET 9 SDK and (optionally) access to the Supabase/PostgreSQL instance referenced below.

```bash
# Restore all projects
dotnet restore

# Launch the WinForms client (API-backed login + gameplay)
dotnet run --project BrickBreaker.WinFormsClient

# Launch the Spectre.Console client (terminal renderer)
dotnet run --project BrickBreaker.ConsoleClient

# Launch the Blazor WebAssembly client (canvas renderer)
dotnet run --project BrickBreaker.WebClient

# Optional: build every project or run the unit tests
dotnet build BrickBreaker.sln
dotnet test BrickBreaker.sln
```

The WinForms and Spectre console clients read the backend URL from the `BRICKBREAKER_API_URL` environment variable (defaults to the hosted Azure Container Apps instance at `https://brickbreaker-api.delightfulsky-8a169c96.swedencentral.azurecontainerapps.io`) and both allow overriding the value at runtime (launcher textbox or console prompt). You can also provide a `clientsettings.json` file next to the executable (or anywhere referenced by `BRICKBREAKER_CLIENT_CONFIG`) with `{ "ApiBaseUrl": "https://your-host" }`, or pass the desired URL as the first argument to `BrickBreaker.ConsoleClient`. Set the environment variable (or edit the launcher field) to point at `http://127.0.0.1:5080` when testing a local API build.

The Blazor client lets you edit the API base URL from the landing page (or set the `ApiBaseUrl` configuration entry when bootstrapping the WASM host, which now defaults to the same Azure endpoint) and uses those endpoints for login, registration, and score submission.

### Configure Supabase/PostgreSQL

Both the WinForms and Spectre console clients surface authentication + leaderboard journeys through the ASP.NET API, which in turn uses Supabase/PostgreSQL. Configure the connection string once and every client benefits:

> **API configuration template**: Copy `BrickBreaker.Api/appsettings.Template.json` to `appsettings.json` (and/or `appsettings.Development.json`) locally, then customize the values. The template is tracked in git, but concrete appsettings files remain ignored so secrets never leak into the repo. Be sure to update:
> - `Cors.AllowedOrigins` with the exact Netlify/localhost origins that need browser access.
> - `Jwt.Secret` (or `Jwt__Secret` when using environment variables) with a long, random string before deploying so issued tokens are trustworthy.
> - `Turnstile.SecretKey` (and set `Turnstile.Enabled` to `true`) if you want the API to enforce Cloudflare Turnstile challenges for `/register` and `/login` requests.

1. Update `BrickBreaker.Storage/Properties/appsettings.json` **or** set an environment variable named `Supabase` / `ConnectionString:Supabase`.
2. Provide a standard Npgsql connection string, for example:

```json
{
  "ConnectionString": {
    "Supabase": "Host=...;Port=5432;Database=...;Username=...;Password=...;Ssl Mode=Require;Trust Server Certificate=true"
  }
}
```

When present, registration/login, leaderboard submissions, per-user best scores, and the Top-10 table use the hosted database. When absent, the UI clearly warns that those features are disabled but gameplay/Quick Play still works via the disabled stores.

When Turnstile is enabled, every registration or login request must provide the one-time `TurnstileToken` generated by the widget; clients that cannot render the CAPTCHA should leave the feature disabled. The Blazor WebAssembly front-end reads the `Turnstile` section from `wwwroot/appsettings.json` (or injected configuration) to learn the site key and show the widget beside the login/registration buttons.

### Controls & tips

- `←` / `→` or `A` / `D` move the paddle, `↑` / `W` launches the tethered ball, `P` pauses the WinForms loop, `F` toggles borderless fullscreen, `Esc` exits, and `Space` restarts after game over.
- Quick Play skips authentication and does not attempt to submit scores.
- Logged-in players can open the gameplay menu after each run to view their best score or browse the Top-10 Spectre console table.

## Tests

Run `dotnet test BrickBreaker.sln` to execute the xUnit suite. Tests rely on the storage abstractions, so they run without real database access.
