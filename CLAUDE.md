# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build SkyLabIdP.sln

# Run (with hot reload)
dotnet watch run --project src/presentation/SkyLabIdP.WebApi

# Test all
dotnet test SkyLabIdP.sln

# Run a specific test project
dotnet test tests/Application.UnitTests
dotnet test tests/Application.IntegrationTests

# Run a single test class
dotnet test --filter "ClassName~AuthenticationFlow"

# E2E tests (requires API running on localhost:8083 first)
dotnet test tests/PlaywrightTests

# Docker
docker build -f src/Dockerfile -t skylabidp-api .
docker run -p 8080:8080 --env-file .env skylabidp-api
```

## Architecture

**Clean Architecture** with strict dependency direction: `WebApi` → `Application` → `Domain`. Infrastructure layers (`Data`, `Identity`, `Shared`) implement interfaces defined in `Application`.

```
src/core/SkyLabIdP.Domain          # Entities, enums, settings — no external deps
src/core/SkyLabIdP.Application     # CQRS handlers, DTOs, validators, interfaces
src/infrastructure/SkyLabIdP.Data       # EF Core + Dapper + UnitOfWork
src/infrastructure/SkyLabIdP.Identity   # JWT (RSA-256), JWKS, Google OAuth
src/infrastructure/SkyLabIdP.Shared     # Email, Redis, file handling, captcha
src/presentation/SkyLabIdP.WebApi       # Controllers, DI wiring, middleware
```

### Data Access: Hybrid EF Core + Dapper

- **EF Core** (`ApplicationDbContext`): ASP.NET Identity tables only
- **Dapper + UnitOfWork**: All business data queries via `IUnitOfWork`
  - Single `SqlConnection` shared across all Dapper repositories
  - Repositories are lazy-loaded properties on `UnitOfWork`
  - Transactions: `BeginTransactionAsync()` / `CommitAsync()` / `RollbackAsync()`
- **Database migrations**: DbUp (not EF migrations) — `db/baseline.sql` runs on empty DBs; `db/scripts/*.sql` applied alphabetically at startup

### CQRS

Uses **Mediator** (source-generator based, faster than MediatR). All business operations are Commands or Queries with corresponding Handlers and Validators in `src/core/SkyLabIdP.Application/SystemApps/`.

**Pipeline behaviors** are auto-applied (registered order matters):
1. `UnhandledExceptionBehavior` — global exception handling
2. `PerformanceBehavior` — logs slow requests
3. `ValidationBehavior` — FluentValidation, throws `ValidationException` before handler runs

### Multi-Tenancy

- Tenant identified via `X-Tenant-Id` request header
- Stored in `HttpContext.Items["Tenant"]`; accessible in controllers via `TenantId` property
- Tenant-specific services registered as **keyed services** (e.g., `SkyLabMgmLoginUserInfoService`, `SkyLabDevelopLoginUserInfoService`)
- Resolved via `TenantUserServiceFactory` and `DefaultPermissionServiceFactory`
- Current tenants: `SkyLabmgm`, `SkyLabdevelop` (see `Domain/Enums/Tenants.cs`)

### Authentication

- **Hybrid MultiAuth**: JWT Bearer + Cookie (both supported simultaneously)
- JWT uses RSA-256 with key rotation; JWKS endpoint exposed for consumers
- API key validated via `X-API-key` header
- Google OAuth 2.0 per tenant (configured in `.env`)

### Mapping & Validation

- **Mapperly** (`Riok.Mapperly`) — source-generated mappers, zero reflection. Add methods to `SkyLabIdPMapper`.
- **FluentValidation** — validators auto-registered from Application assembly.

## Adding New Features

1. Create `Command`/`Query` + `Handler` + `Validator` under `Application/SystemApps/<Feature>/`
2. Define repository interface in `Application/Interfaces/` if new data access is needed
3. Implement interface in `Data/Repositories/`, expose as lazy property on `UnitOfWork`
4. Register any new keyed/tenant-specific services in `Application/DependencyInjection.cs`
5. Add controller action in `WebApi/Controllers/`, inheriting from `ApiController`

## Environment Setup

Copy `.env.example` to `.env`. Required variables:
- `DATABASE_CONNECTION_STRING` — SQL Server
- `JWT_SECRET_KEY` — minimum 32 characters (HMAC fallback)
- `SKYLABIDP_APIKEY` — API key for `X-API-key` header
- `REDIS__HOST` / `REDIS__PASSWORD` — Redis (token storage + output caching)
- Tenant-specific Google OAuth client IDs/secrets

