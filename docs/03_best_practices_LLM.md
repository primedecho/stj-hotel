# Best Practices LLM Context — Hotel Search API

**Purpose:** Coding standards, architecture patterns, and code generation quality bar for this solution.

**Principle:** Generated code must look like it was written by the same team that built the existing codebase.

---

## 1. Core Engineering Principles

| Priority | Rule |
|----------|------|
| Clarity > cleverness | Prefer readable LINQ and explicit steps over dense one-liners |
| Explicitness > abstraction | No new base classes or generic frameworks for single use cases |
| Maintainability > speed | Small focused diffs; tests with behavior changes |
| Correctness > coverage | Meaningful tests over arbitrary percentage targets |
| Documented trade-offs > silent debt | PoC shortcuts allowed only when labeled in architecture docs |

---

## 2. Code Quality Standards

### Language & project settings

- C# with `nullable` enabled, `implicit usings` enabled
- Target: `net10.0`
- Follow `.editorconfig` and analyzer warnings (`AnalysisLevel: latest-recommended`)
- XML doc comments on **public** Api types only (`HotelSearch.Api` generates XML)

### Function & method size

| Guideline | Target |
|-----------|--------|
| Endpoint handler | Thin: map → service call → map result (see `HotelEndpoints.cs`) |
| Service method | One use case per public method; extract private helpers if > ~40 lines |
| Ranker/parser | Pure logic separated (`HotelSearchRanker`, `RegexPromptParser`) |

### Naming rules

| Element | Convention |
|---------|------------|
| Types | PascalCase |
| Interfaces | `I` prefix |
| Private fields | `_camelCase` only if used (prefer primary constructors) |
| Async methods | `Async` suffix |
| Constants | `SearchConstants`, centralized shared limits |
| Tests | `{Method}_{Condition}_{Expected}` or descriptive `[Fact]` names |

### Comment philosophy

- **Do not** narrate obvious code
- **Do** comment non-obvious business rules (ranking weights, regex format assumptions)
- **Do** use XML summary on Domain aggregates and public Application interfaces
- **Do not** leave TODO comments without issue reference

---

## 3. Error Handling (STRICT)

### Never omit error handling on

- External I/O (database, HTTP)
- User input parsing (prompt parser)
- Configuration missing in Production

### Exception mapping

| Source | Handling |
|--------|----------|
| `DomainException` | Map to 400 via Application/`AppException` |
| `NotFoundException` | 404 ProblemDetails |
| FluentValidation failures | 400 with validation errors in ProblemDetails |
| Unhandled exceptions | `ApiExceptionHandler` → 500 with generic message (non-Development) |

### ProblemDetails

- Include `traceId` extension (configured in `Program.cs`)
- Use `ProducesProblem` metadata on endpoints
- Consistent shape via `ApiProblemDetails` helpers where applicable

### Logging

| Level | Use |
|-------|-----|
| Information | Startup summary (`StartupLogger`), successful operations sparingly |
| Warning | Auth failures, recoverable issues |
| Error | Unhandled exceptions (handler logs) |

- Structured JSON logging in Production
- HTTP logging excludes `/health`
- **Never log** API keys, connection strings, or full request bodies with secrets

---

## 4. Architecture Patterns

### REQUIRED

| Pattern | Where | Why |
|---------|-------|-----|
| Clean Architecture layers | Solution structure | Testability, swap Infrastructure |
| Repository port | `IHotelRepository` | Decouple Application from EF |
| Application services | `HotelService`, `HotelSearchService` | Use-case orchestration |
| Value objects | `Money`, `GeoLocation` | Enforce invariants |
| Endpoint filters | Validation, API key | Cross-cutting without controller base |
| ProblemDetails errors | Api pipeline | REST standard error shape |
| Primary constructors | Services, repositories | Modern C# style already in use |
| `CancellationToken` propagation | All async public methods | Host shutdown correctness |

### OPTIONAL (justify before use)

| Pattern | When |
|---------|------|
| Decorator repository | Caching, metrics |
| Separate read interface | Search scale-out |
| Vertical slice folders | Api grows beyond ~15 endpoints |
| URL versioning (`/api/v1`) | Breaking contract change |

### FORBIDDEN (current scope)

| Pattern | Why |
|---------|-----|
| Anemic domain + all logic in services | Domain already encodes invariants |
| DbContext in Application/Api | Breaks dependency rule |
| CQRS / MediatR | Overhead for CRUD + search PoC |
| Generic `IRepository<T>` | Leaks persistence concerns |
| Static service locator | DI is established |
| Business logic in endpoint lambdas | Endpoints map only |
| Dual ORM | Single EF Core path |

---

## 5. Separation of Concerns

### Domain (`HotelSearch.Domain`)

**MUST contain:**

- Entities: `Hotel`
- Value objects: `Money`, `GeoLocation`
- Domain exceptions: `DomainException`
- Pure domain calculations: `GeoLocation.DistanceToKilometers()`

**MUST NOT contain:**

- EF attributes, JSON attributes, HTTP concepts, DTOs

### Application (`HotelSearch.Application`)

**MUST contain:**

- Use cases: `HotelService`, `HotelSearchService`
- Ports: `IHotelRepository`, `IPromptParser`, service interfaces
- Application DTOs/responses: `HotelResponse`, `SearchHotelResult`, `PagedResult<T>`
- Ranking algorithm: `HotelSearchRanker`
- Shared limits: `SearchConstants`

**MUST NOT contain:**

- ASP.NET types, EF Core, Npgsql, configuration binding

### Infrastructure (`HotelSearch.Infrastructure`)

**MUST contain:**

- EF Core DbContext, configurations, migrations
- Repository implementations
- `RegexPromptParser` (implements `IPromptParser`)
- `DependencyInjection.AddInfrastructure`

### Api (`HotelSearch.Api`)

**MUST contain:**

- `Program.cs` composition root
- `HotelEndpoints` — routing + mapping only
- Request/response contracts + FluentValidation
- Auth, exception handling, OpenAPI, health

**MUST NOT contain:**

- Business ranking rules, SQL, domain invariant enforcement duplicated without reason

### Duplication rule

HTTP validation (FluentValidation) may overlap Domain bounds intentionally:

- Api: request shape and limits
- Domain: invariant enforcement regardless of entry point

Extract shared numeric limits to `HotelRequestValidationRules` / `SearchConstants` when duplicated.

---

## 6. Testing Strategy

### Mandatory

| Change type | Required tests |
|-------------|----------------|
| Domain invariant | Unit test in `Domain/` |
| Application service behavior | Unit test with Moq repositories |
| Ranking / parsing logic | Dedicated tests (`HotelSearchRankerTests`, `RegexPromptParserTests`) |
| New validation rule | `HotelApiValidationTests` or validator unit test |
| New/modified endpoint | Integration test if DB or full pipeline affected |
| Auth behavior | Filter tests + secured integration tests |

### Optional

- Performance benchmarks (not required for PoC)
- Contract tests beyond OpenAPI file

### Mocking rules

| Layer | Mock |
|-------|------|
| Application unit tests | Mock `IHotelRepository`, `IPromptParser` |
| Api validation tests | `HotelSearchApiFactory` replaces repository/parser |
| Integration tests | **Real PostgreSQL** via Testcontainers — no mock DB |
| Domain tests | No mocks |

### Integration test patterns (do not break)

- `PostgresIntegrationFixtureBase` + `IntegrationWebApplicationFactory`
- `[Collection]` fixtures for shared container lifecycle
- `ResetDatabaseAsync` / `TRUNCATE TABLE hotels` between tests
- `AssemblyInfo`: parallelization disabled for Testcontainers stability
- Testing environment connection string via factory + `appsettings.Testing.json`

### Coverage expectations (qualitative)

- Every public Application service method has success + key failure path tests
- Every HTTP status code documented on endpoints has at least one test proving it
- Search ordering and paging have unit **and** integration coverage
- No tests that only assert mocks were called without behavior assertion

### Run command

```bash
dotnet test HotelSearch.sln -c Release
```

Integration tests require Docker.

---

## 7. Code Generation Rules

When the LLM writes code, it **must**:

1. Match existing file placement and naming
2. Use `sealed` classes for implementations unless extension is designed
3. Use `internal` for Infrastructure types not needed outside assembly
4. Handle edge cases: empty catalogue, not found, invalid prompt, unauthorized write
5. Add `ArgumentNullException.ThrowIfNull` on endpoint injected dependencies where pattern exists
6. Map entities via existing mappers (`HotelMapper`, `ApiMappingExtensions`) — no inline duplicate mapping blocks
7. Register new services in correct `DependencyInjection.cs`
8. Update OpenAPI + docs when HTTP contract changes

When the LLM **must not**:

- Introduce new NuGet packages without explicit request
- Change public route paths without versioning plan
- Remove or weaken existing tests
- Use `#pragma warning disable` without comment justification
- Generate placeholder `NotImplementedException` in production paths

### Api endpoint checklist

- [ ] FluentValidation rules added/updated
- [ ] `Produces` / `ProducesProblem` metadata
- [ ] Write routes have `ApiKeyAuthorizationFilter` when mutating
- [ ] Correct status codes (201 + Location for create)
- [ ] Mapping extension methods for request/response

### Infrastructure checklist

- [ ] Configuration class updated for schema changes
- [ ] Migration added
- [ ] Repository method on interface + implementation
- [ ] Read vs tracked write query correct

---

## 8. Refactoring Rules

### Allowed when

- User requests refactor explicitly
- Duplication crosses files and shared helper exists pattern (`HotelRequestValidationRules`, `OpenApiRequestExamples`)
- Test improvement without behavior change

### Process

1. State intended behavior preservation
2. Refactor in smallest commits logically grouped by layer
3. Run full test suite
4. No public API change unless documented

### Avoid breaking changes

- Do not rename JSON properties without versioning
- Do not change ranking algorithm output without updating tests and documenting in architecture
- Do not change repository method semantics without updating all callers and tests

### Safe improvement proposals

Present as: **Problem → Minimal change → Risk → Test impact**

Do not rewrite working layers for aesthetic reasons.

---

## 9. Consistency Enforcement

### Follow existing patterns over “better” alternatives

Examples already established in repo:

| Concern | Existing pattern |
|---------|------------------|
| Paged responses | `PagedResult<T>` (Application), `PagedResponse<T>` (Api) |
| Shared validation | `HotelRequestValidationRules` |
| OpenAPI examples | `OpenApiRequestExamples` |
| Search limits | `SearchConstants` |
| Integration setup | `PostgresIntegrationFixtureBase` |
| Ranked search | `HotelSearchRanker.Rank` |

New code must extend these before introducing parallel types.

### Style drift prevention

- Match brace style and file-scoped namespaces in each project
- Use same HTTP client patterns in tests (`HotelApiClient`)
- Keep test arrange/act/assert spacing consistent with neighboring tests
- Do not mix controller pattern into Minimal API codebase

### Dependency drift prevention

Before adding a project reference, verify against:

```text
Domain → (none)
Application → Domain
Infrastructure → Application, Domain
Api → Application, Infrastructure
Tests → all as needed
```

---

## 10. Security & Privacy Coding Rules

- Validate all external input at Api boundary
- Search prompt max length: 500 characters (`SearchConstants`)
- Compare API keys with fixed-time comparison (`ApiKeyAuthorizationFilter`)
- No secrets in `appsettings.json` committed to repo
- No personal names or private emails in committed markdown templates
- Write operations protected when key configured; Production requires key at startup

---

## 11. CI & Delivery Expectations

Any change that breaks the following must be fixed before considering work complete:

- `dotnet build HotelSearch.sln -c Release`
- `dotnet test HotelSearch.sln -c Release`
- `docker compose build`
- No new vulnerable packages without documented exception

Do not disable CI steps to pass locally.

---

## 12. Deviation & Review Checklist

Before finishing any LLM-generated change, verify:

| Question | Expected |
|----------|----------|
| Did logic land in the correct layer? | Yes |
| Are tests updated? | If behavior changed |
| Is docs/OpenAPI updated? | If contract changed |
| Is migration included? | If schema changed |
| Are PoC limits still honest? | Document if search/load pattern changes |
| Minimal diff? | No unrelated files |

If any answer fails, fix before presenting work as complete.
