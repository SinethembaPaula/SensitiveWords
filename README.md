# SensitiveWords API

A RESTful microservice that sanitises messages by replacing sensitive words with asterisks. Built with ASP.NET Core 9, Dapper, and MSSQL.

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- SQL Server or SQL Server Express

### Database Setup

Run the scripts in the `database/` folder in order against your SQL Server instance:

```sql
-- 1. Create the database
database/001_CreateDatabase.sql

-- 2. Create the schema
database/002_CreateSchema.sql

-- 3. Seed sensitive words
database/003_Seed.sql
```

### Configuration

Copy `appsettings.example.json` to `appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "SensitiveWordsDb": "Server=.\\SQLEXPRESS;Database=SensitiveWordsDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Run

```bash
dotnet run --project src/SensitiveWords.Api
```

Navigate to `https://localhost:{port}/swagger` to explore the API.

---

## API Endpoints

### Business Logic — External Consumption

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/messages/sanitise` | Sanitise a message |

**Request:**
```json
{ "input": "SELECT * FROM users" }
```

**Response:**
```json
{ "output": "************* users" }
```

### CRUD — Internal Consumption

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/sensitive-words` | List all sensitive words |
| GET | `/api/sensitive-words/{id}` | Get a word by ID |
| POST | `/api/sensitive-words` | Add a new word |
| PUT | `/api/sensitive-words/{id}` | Update a word |
| DELETE | `/api/sensitive-words/{id}` | Delete a word |

---

## Architecture

The solution follows Clean Architecture with 3 layers:

```
SensitiveWords.Application    — Interfaces, DTOs, Validators, Services (no dependencies)
SensitiveWords.Infrastructure — Dapper repository, cache decorator
SensitiveWords.Api            — Controllers, middleware, Program.cs
```

### Key Design Decisions

**Caching — Decorator Pattern**
The `CachedSensitiveWordRepository` wraps the Dapper repository using the decorator pattern. The word list is loaded from the database once and cached in `IMemoryCache` with a 5-minute TTL. Any CRUD write operation immediately invalidates the cache.

**Sanitisation Algorithm — Compiled Regex with Longest-Match-First**
Words are sorted by length descending before building the Regex pattern. This ensures multi-word phrases like `SELECT * FROM` are matched before their component words (`SELECT`). The pattern uses `\b` word boundaries so substrings are never incorrectly starred — `SELECTION` is not affected by the word `SELECT`. A 2-second regex timeout prevents ReDoS attacks on malicious input.

**Validation — Dedicated Validator Classes**
Input validation lives in the Application layer, not in controllers. Controllers call validators and return `ValidationProblemDetails` (RFC 7807) on failure. This keeps controllers thin and validation independently testable.

**Error Handling — Global Middleware**
All unhandled exceptions are caught by `ExceptionHandlingMiddleware` and returned as RFC 7807 `ProblemDetails` responses. No exception details leak to the client.

**Logging — Structured Serilog**
All logs are structured and written to rolling daily files under `logs/`. Every log entry carries `RequestId`, `ConnectionId`, and `SourceContext` — allowing full request tracing from a single log file. The bootstrap logger captures startup failures before the host is built.

---

## Production Deployment

### Infrastructure

```
[Client Applications]
        │
        ▼
[Azure API Management]
   ├── External route: /api/messages/*     → Rate limited, API key required
   └── Internal route: /api/sensitive-words/*  → VNet restricted, internal only
        │
        ▼
[Azure App Service / AKS]
   SensitiveWords API (containerised)
        │
        ▼
[Azure SQL — SensitiveWordsDb]
```

### Why Azure API Management (APIM)?

The assessment specifies two consumption types — internal (CRUD) and external (sanitise). APIM enforces this split at the infrastructure level:

- External consumers hit `/api/messages/sanitise` with an API key — rate limited to prevent abuse
- Internal consumers are restricted to the VNet — the CRUD endpoints are never publicly reachable
- No code changes required to enforce this separation

### Secrets Management

Connection strings and API keys are stored in **Azure Key Vault** and injected at runtime via App Service configuration. Nothing sensitive is ever committed to source control.

### CI/CD Pipeline

```
Push to main
    → Build
    → Unit Tests
    → Docker image build
    → Push to Azure Container Registry
    → Deploy to App Service (zero-downtime slot swap)
```

### Observability

- **Logs** — Serilog structured logs shipped to Azure Application Insights
- **Health checks** — `/health` endpoint for load balancer probes (future enhancement)
- **Alerts** — Application Insights alerts on 5xx error rate and response time

### Scale Considerations

The current implementation uses `IMemoryCache` - suitable for a single instance. For horizontal scaling (multiple API instances behind a load balancer), replace with `IDistributedCache` backed by **Azure Cache for Redis** so all instances share one cache and invalidation is immediate across all nodes.

For very large word lists, replace the compiled Regex with an **Aho-Corasick** finite state machine - O(n) scan regardless of word list size with no backtracking risk.

---

## Additional Questions

### a. Performance Enhancements

- **In-memory cache** - word list is loaded from the database once per cache miss, not per request
- **Compiled Regex** - built once per cache population, reused across all requests until invalidation
- **Longest-match-first ordering** - both in the DB query (`ORDER BY LEN(Word) DESC`) and in code, as defense in depth
- **Redis** - replace `IMemoryCache` with distributed cache for multi-instance deployments
- **Aho-Corasick** - replace Regex for datasets with thousands of words

### b. Additional Enhancements

- **API versioning** - `/api/v1/` prefix so the contract can evolve without breaking clients
- **Word categories** - group words by type (SQL, profanity, custom) and filter by category on sanitise
- **Audit log** - track who added, updated, or deleted words and when
- **Bulk import endpoint** - `POST /api/sensitive-words/bulk` for loading large word lists
- **Health checks endpoint** - `/health` for load balancer and uptime monitoring
- **Rate limiting** - `AddRateLimiter` on the sanitise endpoint to prevent abuse
