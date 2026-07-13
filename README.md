# Customer Payments API

A production-style ASP.NET Core REST API built to demonstrate modern backend development practices using a layered architecture, clean code principles, and production-ready design patterns.

This project was created as a portfolio application to showcase enterprise-level development practices including authentication, validation, exception handling, testing, caching, rate limiting and clean separation of responsibilities.

---

## What this project demonstrates

- REST API design following best practices
- SOLID principles
- Dependency Injection
- Repository & Service Pattern
- Entity Framework Core
- JWT Authentication & Authorization
- DTO Mapping with AutoMapper
- Validation with FluentValidation
- Global Exception Handling
- API Versioning
- Output Caching
- Rate Limiting
- Health Checks
- Correlation ID
- Unit Testing
- Integration Testing
- Clean and maintainable architecture

---

## Features

- Customer management
- Payment management
- JWT Authentication
- Soft Delete
- Optimistic Concurrency (Version)
- Repository Pattern
- Service Layer
- AutoMapper
- FluentValidation
- Global Exception Handling using `IExceptionHandler`
- RFC 7807 Problem Details responses
- API Versioning
- Output Cache
- Rate Limiting
- Health Checks
- Swagger / OpenAPI
- Unit Tests
- Integration Tests

---

## Architecture

```
src/
└── CustomerPayments.Api
    ├── Application
    ├── Controllers
    ├── Data
    ├── Domain
    ├── DTOs
    ├── ExceptionHandling
    ├── Extensions
    ├── Filters
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

### Layers

- **Controllers** expose the HTTP endpoints.
- **Services** implement business rules.
- **Repositories** encapsulate data access.
- **Domain** contains the business entities.
- **DTOs** define the API contracts.
- **Validators** validate incoming requests.
- **Mappings** translate between entities and DTOs.

---

## Technologies

| Technology | Description |
|------------|-------------|
| .NET 10 | Backend framework |
| ASP.NET Core Web API | REST API |
| Entity Framework Core | ORM |
| SQLite | Database |
| JWT | Authentication |
| AutoMapper | Object Mapping |
| FluentValidation | Request Validation |
| Swagger / OpenAPI | API Documentation |
| xUnit | Unit Testing |
| Moq | Mocking |
| FluentAssertions | Assertions |
| WebApplicationFactory | Integration Testing |

---

## Authentication

Authenticate using the login endpoint:

```
POST /api/v1/auth/login
```

Example credentials (Development):

```
Email: admin@customerpayments.com
Password: ********
```

> **Note**
>
> For security reasons, credentials should be provided using User Secrets or environment variables rather than hardcoded configuration.

---

## REST Endpoints

### Authentication

| Method | Endpoint |
|---------|----------|
| POST | `/api/v1/auth/login` |

### Customers

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/customers` | Returns a paginated list of active customers |
| GET | `/api/v1/customers/{id}` | Returns a customer by ID |
| POST | `/api/v1/customers` | Creates a new customer |
| PUT | `/api/v1/customers/{id}` | Updates an existing customer |
| PATCH | `/api/v1/customers/{id}/deactivate` | Deactivates a customer using a soft-delete strategy |

#### Deactivate a customer

Customers are not physically deleted from the database. The API uses a
soft-delete strategy that marks the customer as inactive.

```http
PATCH /api/v1/customers/{id}/deactivate
Authorization: Bearer {token}
Content-Type: application/json

### Payments

| Method | Endpoint |
|---------|----------|
| GET | `/api/v1/customers/{customerId}/payments` |
| POST | `/api/v1/customers/{customerId}/payments` |

---

## Filtering, Sorting and Pagination

Example:

```
GET /api/v1/customers?pageNumber=1&pageSize=10&search=john&sortBy=lastName&sortDirection=desc
```

Supported features:

- Pagination
- Search
- Sorting

---

## Running the project

Restore packages:

```bash
dotnet restore
```

Build:

```bash
dotnet build
```

Run:

```bash
dotnet run --project src/CustomerPayments.Api
```

Open Swagger:

```
https://localhost:<port>/swagger
```

Health Check:

```
https://localhost:<port>/health
```

### Database migrations

In the Development environment, pending Entity Framework Core migrations
are applied automatically when the application starts.

They can also be applied manually:

```bash
dotnet ef database update \
  --project src/CustomerPayments.Api \
  --startup-project src/CustomerPayments.Api
  
---

## Running the tests

Run all tests:

```bash
dotnet test
```

Run Unit Tests:

```bash
dotnet test tests/CustomerPayments.UnitTests
```

Run Integration Tests:

```bash
dotnet test tests/CustomerPayments.IntegrationTests
```

---

## Error Handling

The API uses centralized exception handling through `IExceptionHandler` and returns standardized **RFC 7807 Problem Details** responses.

Example:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110",
  "title": "Validation failed.",
  "status": 400,
  "errors": {
    "FirstName": [
      "First Name is required."
    ]
  }
}
```

---

## Testing

The solution includes:

- Unit Tests for business logic
- Integration Tests using `WebApplicationFactory`
- SQLite In-Memory database for integration tests
- Moq for dependency mocking
- FluentAssertions for expressive assertions

---

## Design Goals

This project focuses on demonstrating enterprise backend development practices rather than business complexity.

Key objectives include:

- Clean Architecture
- SOLID principles
- Separation of Concerns
- Dependency Injection
- Testability
- Maintainability
- Production-ready API conventions
- Clean and readable code

---

## Future Improvements

Possible enhancements include:

- Refresh Tokens
- Docker support
- CI/CD pipeline
- OpenTelemetry
- Serilog integration
- Role-based authorization
- API metrics
- Audit logging

---

## License

This project is intended for educational and portfolio purposes.