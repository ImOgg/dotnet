# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a learning/reference repository for ASP.NET Core Web API development. It contains:
- A working ASP.NET Core Web API project (`API/`) with .NET 8.0, EF Core 9, and MySQL
- Technical documentation in `docs/` and `API/Examples/`
- Docker Compose setup for MySQL + API

## Common Commands

Run all commands from the `API/` directory unless noted otherwise.

```bash
# Restore packages
dotnet restore

# Run (development)
dotnet run

# Watch mode (auto-restart on changes)
dotnet watch run

# Build
dotnet build

# Publish
dotnet publish -c Release
```

### Entity Framework Migrations

```bash
# Must be run from API/ directory
dotnet ef migrations add <MigrationName>
dotnet ef database update
dotnet ef migrations list
dotnet ef migrations remove        # removes last unapplied migration
dotnet ef database update <Name>   # rollback to specific migration
```

### Dev Certificates (HTTPS)

```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

## Architecture

### Project Structure

```
dotnet.sln
API/
├── Program.cs              # Entry point: service registration + middleware pipeline
├── Controllers/            # API controllers (route: api/[controller])
├── Entities/               # EF Core entity classes (e.g. AppUser)
├── Data/
│   └── ApplicationDbContext.cs   # DbContext with DbSet<AppUser>
├── Migrations/             # EF Core auto-generated migrations
├── Models/                 # Additional model classes
├── appsettings.json        # DB connection string (DefaultConnection)
├── appsettings.Development.json
├── API.http                # VS Code REST Client test file
└── Dockerfile
docker-compose.yml          # MySQL + API services (uses env vars)
docs/                       # Reference documentation (Chinese)
```

### Key Patterns

- **Database**: MySQL via `Pomelo.EntityFrameworkCore.MySql`. Connection string in `appsettings.json` under `ConnectionStrings:DefaultConnection`. In Docker, overridden via `ConnectionStrings__DefaultConnection` env var.
- **DbContext**: `ApplicationDbContext` in `API/Data/`. Uses `DbContextOptions` (non-generic) constructor.
- **Entities**: Defined in `API/Entities/` namespace `API.Entities`.
- **Controllers**: Use `[Route("api/[controller]")]` prefix. The `[controller]` token maps to class name minus `Controller` suffix.
- **Swagger**: Enabled in Development environment at `/swagger`.

### Docker Compose

Reads environment variables for configuration — create a `.env` file with:
- `MYSQL_ROOT_PASSWORD`, `MYSQL_DATABASE`, `MYSQL_HOST_PORT`
- `ASPNETCORE_ENVIRONMENT`, `CONNECTION_STRING`, `API_PORT`

The API container depends on MySQL becoming healthy before starting.

## Notes

- The `docs/` and `API/Examples/` folders contain Chinese-language reference notes on ASP.NET concepts, EF migrations, DI, file storage, background jobs, etc. Consult them for project-specific conventions.
- `MySqlServerVersion` is hardcoded to `8.0.21` in `Program.cs` to avoid design-time DB connection issues.
