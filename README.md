> Continuation of the original group project. Frozen version: [BrickBreaker-Group](https://github.com/lanavirsen/BrickBreaker-Group)

# BrickBreaker – Extended Edition

![CI](https://github.com/lanavirsen/BrickBreaker-Extended/actions/workflows/ci.yml/badge.svg)  [![codecov](https://codecov.io/gh/lanavirsen/BrickBreaker-Extended/branch/main/graph/badge.svg)](https://codecov.io/gh/lanavirsen/BrickBreaker-Extended)

Play it live: https://brickbreaker-extended.netlify.app

BrickBreaker is a Blazor WebAssembly remake of the classic paddle-and-bricks arcade game, deployed as a responsive web app that streams the .NET gameplay loop into a `<canvas>`. The browser client handles login, registration, CAPTCHA, score submission, and leaderboard views while sharing the same `GameEngine` used by the desktop builds. Console and WinForms shells remain in the repo to demonstrate how multiple UI layers can plug into the shared gameplay/session architecture without forking core logic.

## Gameplay

- **5 levels** with increasing brick density (15 → 25 → 35 → 45 → 55 bricks). Beyond level 5 the layout repeats while the level counter and ball speed keep climbing.
- **Scoring** – each brick is worth 10 points × the current streak multiplier. The multiplier grows with every consecutive brick hit and resets to 1 when the ball touches the paddle. Floating `+N` and `×N` popups appear at the hit site.
- **Ball speed** rises each level on a sub-linear curve (`7.5 + √(level − 1)`) applied whenever the ball bounces off the paddle, so the game gets faster without sudden spikes.
- **Power-ups** drop from destroyed bricks with a 20% chance and fall until caught by the paddle or lost off-screen:
  - **Multiball (M)** – splits into three balls by spawning two additional ones from the current ball's position.
  - **Paddle Extender (E)** – widens the paddle for 10 seconds; the paddle blinks in the final second as a warning before it shrinks back.
- **Game over** when the last ball falls below the paddle. There are no lives — keep all balls alive to keep playing.

## Screenshots

### Web client

<p align="center">
  <img src="docs/images/web-gameplay.png">
</p>

### WinForms client

<p align="center">
  <img src="docs/images/winforms-gameplay.png">
</p>

### Console client

<p align="center">
  <img src="docs/images/console-app-menu.png" width="49%">
  <img src="docs/images/console-app-leaderboard.png" width="49%">
</p>
<p align="center">
  <img src="docs/images/console-app-gameplay.png">
</p>

## Highlights

- **Web-first experience** – `BrickBreaker.WebClient` is a Blazor WebAssembly front-end that centers the canvas inside a responsive two-column layout, keeps the leaderboard/account panels aligned, and shows warm-up placeholders while the API wakes up.
- **Shared gameplay loop** – `BrickBreaker.Gameplay` wraps the `GameEngine` in a reusable `GameSession` + `GameRenderState` model so all three clients (web, WinForms, console) share identical physics, level progression, power-up logic, and scoring without any duplicated game code.
- **WinForms renderer @ 60 FPS** – `BrickBreaker.WinFormsClient` still ships with a borderless, high-frame-rate renderer and launcher UI to demonstrate a desktop host reusing the shared game session.
- **Spectre.Console shell** – `BrickBreaker.ConsoleClient` renders the full game in a 60×24 character grid driven by the shared engine, alongside a Spectre.Console menu shell for auth, Quick Play, and leaderboard browsing.
- **Supabase/PostgreSQL persistence** – When a connection string is available, credentials are hashed, scores are stored through `BrickBreaker.Storage`, and all clients can submit/query the same leaderboard. Disabled stores keep gameplay working offline.
- **Automated tests** – `BrickBreaker.Tests` exercises authentication, password hashing, profanity filtering, and leaderboard ordering so domain logic stays correct regardless of the UI host.

## Project layout

```
BrickBreaker/
├── BrickBreaker.sln             Solution root (net9.0)
├── BrickBreaker.Game/           Pure gameplay engine - physics, ball/paddle/brick logic, entities
├── BrickBreaker.Gameplay/       Shared GameSession + render models (used by all clients)
├── BrickBreaker.Core/           Domain models + services (Auth, Leaderboard, abstractions)
├── BrickBreaker.Storage/        Supabase/PostgreSQL stores + configuration helpers
│   ├── StorageConfiguration.cs  Resolves Supabase connection strings
│   ├── UserStore.cs             Npgsql-backed implementation
│   ├── LeaderboardStore.cs      Npgsql-backed implementation
│   └── Disabled*.cs             Null-object stores for offline play
├── BrickBreaker.Api/            ASP.NET Minimal API - auth, leaderboard, CAPTCHA endpoints
├── BrickBreaker.ConsoleClient/  Spectre.Console client with terminal renderer + Supabase auth
├── BrickBreaker.WinFormsClient/ WinForms client (launcher + Form1 gameplay)
│   ├── Hosting/                 IGame implementation for desktop play
│   └── WinUI/                   WinForms forms, drawing, input, assets
├── BrickBreaker.WebClient/      Blazor WebAssembly canvas client for browsers
├── BrickBreaker.Tests/          xUnit tests for Auth + Leaderboard logic
└── README.md
```

## Run the game locally

Prerequisites: .NET 9 SDK and (optionally) access to the Supabase/PostgreSQL instance referenced below.

### For full functionality (auth + leaderboard)

1. **Configure the API:**
   ```bash
   # Copy the template and configure JWT secret + Supabase connection string
   cp BrickBreaker.Api/appsettings.Template.json BrickBreaker.Api/appsettings.json
   ```

2. **Run the API and WebClient:**
   ```bash
   # Terminal 1: Run the API backend
   dotnet run --project BrickBreaker.Api

   # Terminal 2: Run the Blazor WebAssembly client
   dotnet run --project BrickBreaker.WebClient
   ```

### For offline/guest play

```bash
# Just run the WebClient (auth/leaderboard features disabled)
dotnet run --project BrickBreaker.WebClient

# Or try the desktop clients
dotnet run --project BrickBreaker.WinFormsClient
dotnet run --project BrickBreaker.ConsoleClient
```

### Build

```bash
dotnet restore
dotnet build BrickBreaker.sln
```

## Tests

Run `dotnet test BrickBreaker.sln` to execute the xUnit suite. Tests rely on the storage abstractions, so they run without real database access.

## Deployment

- `.github/workflows/ci.yml` restores, builds, runs the tests, and pushes the API container to GHCR before updating the Azure Container Apps revision.
- `.github/workflows/frontend-deploy.yml` publishes `BrickBreaker.WebClient` and deploys the static site to Netlify.
