# Customer Payments API

> Production-style ASP.NET Core Web API built with **.NET 10** to demonstrate modern backend engineering practices, clean architecture principles, security, testing and containerized deployment.

## Highlights

- ASP.NET Core 10 REST API
- JWT Authentication + Refresh Token Rotation
- Dockerized application
- Entity Framework Core + SQLite
- Repository & Service Pattern
- AutoMapper
- FluentValidation
- Global Exception Handling (RFC 7807)
- API Versioning
- Output Caching
- Rate Limiting
- Correlation ID
- Health Checks
- Unit & Integration Tests

## Project Overview

Customer Payments API is a portfolio project that demonstrates enterprise-grade backend development practices using ASP.NET Core.

## Features

- Customer Management
- Payment Management
- JWT Authentication
- Refresh Token Rotation
- Refresh Token Revocation
- Refresh Tokens stored as SHA-256 hashes
- Role-based Authorization
- Soft Delete
- Optimistic Concurrency
- AutoMapper
- FluentValidation
- Global Exception Handling
- RFC 7807 Problem Details
- API Versioning
- Output Cache
- Rate Limiting
- Correlation ID
- Health Checks
- Swagger/OpenAPI
- Docker Support
- Unit Tests
- Integration Tests

## Architecture

```text
src/
└── CustomerPayments.Api
    ├── Application
    ├── Controllers
    ├── Data
    ├── Domain
    ├── DTOs
    ├── ExceptionHandling
    ├── Extensions
    ├── Infrastructure
    ├── Interfaces
    ├── Mappings
    ├── Middleware
    ├── Options
    ├── Repositories
    ├── Services
    └── Validators

tests/
├── CustomerPayments.UnitTests
└── CustomerPayments.IntegrationTests
```

## Technologies

| Technology | Purpose |
|------------|---------|
| .NET 10 | Backend Framework |
| ASP.NET Core | REST API |
| Entity Framework Core | ORM |
| SQLite | Database |
| JWT | Authentication |
| Docker | Containerization |
| AutoMapper | DTO Mapping |
| FluentValidation | Validation |
| Swagger | API Documentation |
| xUnit | Unit Testing |
| Moq | Mocking |
| FluentAssertions | Assertions |

## Authentication

- POST /api/v1/auth/login
- POST /api/v1/auth/refresh
- POST /api/v1/auth/revoke

Refresh Tokens are rotated and stored as SHA-256 hashes.

## Running Locally

```bash
dotnet restore
dotnet build
dotnet run --project src/CustomerPayments.Api
```

Swagger:
https://localhost:5001/swagger

## Running with Docker

```bash
docker compose up --build
docker compose up -d
docker compose down
docker compose down -v
```

Swagger:
http://localhost:8080/swagger

## Environment Variables

```env
JWT_SECRET_KEY=your-secret-key
DEMO_USER_ID=1
DEMO_USER_EMAIL=admin@customerpayments.com
DEMO_USER_PASSWORD=your-password
```

## Database Migrations

```bash
dotnet ef database update --project src/CustomerPayments.Api --startup-project src/CustomerPayments.Api
```

## Testing

```bash
dotnet test
dotnet test tests/CustomerPayments.UnitTests
dotnet test tests/CustomerPayments.IntegrationTests
```

## Security

- JWT Authentication
- Refresh Token Rotation
- Token Revocation
- SHA-256 Refresh Token hashing
- Role-based Authorization
- FluentValidation
- Correlation IDs

## Future Improvements

- CI/CD Pipeline
- OpenTelemetry
- Distributed Cache (Redis)
- API Metrics
- Audit Logging
- Multi-factor Authentication

## License

This project is intended for educational and portfolio purposes.
