# SkyLabIdP Project Context

## Overview
SkyLabIdP is a high-performance, multi-tenant Identity Provider (IdP) built on **.NET 10** using **Clean Architecture**. It provides centralized authentication and authorization services for the SkyLab ecosystem, supporting multiple tenants (e.g., SkyLabmgm, SkyLabdevelop, SkyLabcommittee) through a single unified API.

### Key Architectural Pillars
- **Hybrid Data Access**: Combines **EF Core** (for Identity management and schema migrations) with **Dapper** (for high-performance business logic queries and writes).
- **Multi-Tenancy**: Implements logical isolation using the `X-Tenant-Id` header. Services are resolved dynamically via **Keyed Services** (e.g., `ITenantUserServiceFactory`).
- **CQRS**: Uses the **Mediator** library for command and query separation, with a pipeline-driven approach for validation, performance monitoring, and exception handling.
- **Security**: 
  - **RSA-256 JWT**: Standard-compliant tokens with automated background key rotation.
  - **JWKS Endpoint**: Exposed for public key discovery.
  - **Hybrid MultiAuth**: Intelligent switching between JWT (API) and Cookies (OAuth/Web).

## Project Structure
- **`src/core/SkyLabIdP.Domain`**: Core entities (`ApplicationUser`, `UserTenant`), Enums (`Tenants`, `Permissions`), and Domain-specific settings. No external dependencies.
- **`src/core/SkyLabIdP.Application`**: Business use cases (MediatR Handlers), DTOs, FluentValidation, and Mapperly profiles. Defines `IUnitOfWork` and `IUserService`.
- **`src/infrastructure/SkyLabIdP.Data`**: Implementation of repositories and `UnitOfWork` using Dapper. Contains `ApplicationDbContext` for EF Core. Handles DbUp migrations.
- **`src/infrastructure/SkyLabIdP.Identity`**: Implementation of `IJwtService` (RSA signing), Google OAuth integration, and Token storage in Redis.
- **`src/infrastructure/SkyLabIdP.Shared`**: Common infrastructure like `IEmailService` (MailKit) and `IDistributedCache` (Redis).
- **`src/presentation/SkyLabIdP.WebApi`**: Entry point. Contains Controllers, Filters (Global Exception Handling), and DI Extension methods.

## Building and Running

### Prerequisites
- .NET 10 SDK
- SQL Server (LocalDB or Instance)
- Redis Server

### Key Commands
```bash
# Build the solution
dotnet build SkyLabIdP.sln

# Run the Web API (Watch mode)
dotnet watch run --project src/presentation/SkyLabIdP.WebApi/SkyLabIdP.WebApi.csproj

# Apply Database Migrations (Auto-applied on startup via DbUp)
# If manual migration is needed:
dotnet ef database update --project src/infrastructure/SkyLabIdP.Data/

# Run Unit Tests
dotnet test tests/Application.UnitTests/

# Run Integration Tests
dotnet test tests/Application.IntegrationTests/

# Run E2E Tests (Requires API running at localhost:8083)
dotnet test tests/PlaywrightTests/
```

## Development Conventions

### Coding Standards
- **Feature Addition**: Every new feature should be a MediatR Command or Query. Place these in `Application/SystemApps/{Module}/`.
- **Validation**: Every Command must have a corresponding `AbstractValidator<T>`.
- **Data Access**: 
  - Business logic MUST use `IUnitOfWork` and Dapper repositories. 
  - `ApplicationDbContext` is reserved for Identity management and internal EF operations.
- **Naming**: Use PascalCase for C# classes/methods. Prefix interfaces with `I`.

### Multi-Tenant Logic
- Tenant-specific logic is encapsulated in services like `SkyLabMgmLoginUserInfoService`. 
- Resolve these services using the `ITenantUserServiceFactory` or via Keyed Dependency Injection.

### Error Handling
- Use `ApiExceptionFilterAttribute` for global exception mapping to RFC 7807 Problem Details.
- For business logic errors, throw a `ValidationException` or `NotFoundException` from the Application layer.

### Testing
- **Unit Tests**: Mock external dependencies. Use `TransactionalTestDatabaseFixture` for EF-related logic (uses InMemory).
- **Integration Tests**: Use a real SQL Server instance with the Transactional Fixture to ensure database state is rolled back after each test.
- **E2E Tests**: Use Playwright to verify full API flows.
