# Evaluation Checklist

Optional self-review appendix for Lemax evaluators. **Primary reviewer entry point:** [SUBMISSION.md](SUBMISSION.md).

Engineering self-review of the Hotel Search API PoC against typical take-home criteria.

**Status values:** **Implemented** · **Partially implemented** · **Not implemented**

**Scope labels** (used where something is deliberately not built):

| Label | Meaning |
|-------|---------|
| **Out of scope for this PoC** | Not required for the assignment; omitted to keep focus |
| **Not implemented by design** | A conscious product or architecture choice for this version |
| **Future improvement** | Reasonable next step if the project continued beyond a take-home |

See the [summary](#summary) for a one-page view.

---

## Functionality

**Status: Implemented** — with known limits on prompt parsing (see scope decisions below).

| Delivered | Notes |
|-----------|-------|
| Hotel CRUD | Create, read, update, delete with appropriate status codes (`201`, `200`, `204`, `404`) |
| Search | Returns name, price, distance, and ranking score for CRUD-created hotels |
| Paging | `page`, `pageSize`, `totalCount`, `totalPages`; defaults and max page size enforced |
| Ranking | Price and distance drive rank; budget penalty when prompt includes a budget |
| Health | `/health` includes PostgreSQL connectivity |

| Scope decision | Detail |
|----------------|--------|
| **Not implemented by design** | Prompt parsing uses **fixed regex formats**, not open-ended natural language (e.g. “somewhere near the cathedral” will not parse) |
| **Out of scope for this PoC** | No seed endpoint, bulk import, soft delete, or audit history |
| **Not implemented by design** | Search reads only hotels created via CRUD — no external hotel catalogue |

---

## Technical design

**Status: Implemented** — Clean Architecture at assignment depth, not a full DDD or microservices layout.

| Delivered | Notes |
|-----------|-------|
| Layering | Four projects; inward dependency rule enforced in project references |
| Domain model | `Hotel` aggregate; `GeoLocation` and `Money` value objects; domain exceptions |
| Ports and adapters | `IHotelRepository`, `IPromptParser` implemented in Infrastructure |
| Separation | Application has no ASP.NET Core or EF Core references |

| Scope decision | Detail |
|----------------|--------|
| **Out of scope for this PoC** | No CQRS, domain events, or specification pattern |
| **Not implemented by design** | Search loads all hotels and ranks in memory — no read model or geo index |
| **Not implemented by design** | Minimal APIs instead of controller-based MVC (appropriate for this surface area) |

---

## Technology

**Status: Implemented** — stack matches assignment expectations.

| Delivered | Notes |
|-----------|-------|
| Runtime | .NET 10, C# |
| Web | ASP.NET Core Minimal APIs |
| Data | PostgreSQL 16, EF Core, Npgsql |
| Validation | FluentValidation |
| Testing | xUnit, FluentAssertions, Moq, Testcontainers |
| Containers | Docker Compose, multi-stage Dockerfile |
| CI | GitHub Actions (build + test) |

| Scope decision | Detail |
|----------------|--------|
| **Not implemented by design** | Swashbuckle/OpenAPI at runtime in **Development only**; committed spec in `docs/openapi/` |
| **Out of scope for this PoC** | No Redis, message bus, or external search engine |

---

## Standards

**Status: Partially implemented** — core HTTP and error conventions are in place; several production-hardening standards were not in assignment scope.

| Delivered | Notes |
|-----------|-------|
| Errors | RFC 7807 ProblemDetails with `traceId` |
| JSON | camelCase property names |
| Resources | RESTful paths under `/api/hotels` |
| Logging | HTTP request logging (method, path, status, duration) |
| Health | Documented `/health` response shape |

| Scope decision | Detail |
|----------------|--------|
| **Out of scope for this PoC** | No URL version prefix (`/api/v1/...`); single-version routes are intentional — see [architecture.md — API versioning](architecture.md#api-versioning) |
| **Not implemented by design** | No HATEOAS beyond `Location` on create |
| **Future improvement** | OpenTelemetry / W3C trace context export |
| **Future improvement** | Separate liveness and readiness probes (Kubernetes-style) |
| **Future improvement** | Client-propagated correlation IDs (today: ASP.NET Core trace identifier only) |

---

## Coding style

**Status: Partially implemented** — EditorConfig and shared MSBuild props; no third-party style enforcer or CI format gate.

| Delivered | Notes |
|-----------|-------|
| EditorConfig | 4-space indent, UTF-8, naming, using order, file-scoped namespaces |
| Directory.Build.props | Nullable reference types, shared TFM, code style in build |
| Git hygiene | `.gitattributes`, expanded `.gitignore` |
| Consistency | File-scoped namespaces used throughout |

| Scope decision | Detail |
|----------------|--------|
| **Not implemented by design** | Style rules are warnings/suggestions, not build-breaking errors |
| **Out of scope for this PoC** | No StyleCop or custom analyzer packages |
| **Future improvement** | `dotnet format` check in CI |
| **Not implemented by design** | Some types remain `public` (e.g. validators) for test and DI access |

---

## Source code organization

**Status: Implemented**

| Delivered | Notes |
|-----------|-------|
| Solution layout | `HotelSearch.sln` with `src/` and `tests/` |
| Feature grouping | Endpoints, Validation, Infrastructure, Persistence, Search folders |
| Tests | Mirror layers — Domain, Application, Infrastructure, Api (unit + integration) |
| Configuration | `appsettings.{Environment}.json`, `.env.example`, Docker Compose |

| Scope decision | Detail |
|----------------|--------|
| **Not implemented by design** | Cross-cutting filters live in the Api project (standard for this architecture) |

---

## Performance

**Status: Partially implemented** — appropriate for demo catalogues; not measured or optimised at scale.

**Performance review (final pass):** Async/await used throughout; no blocking calls. Reads use `AsNoTracking`; updates use tracked entities; deletes use `ExecuteDeleteAsync` (single round trip). Search is O(n) load + O(n log n) in-memory rank — paging applied after sort. Low-risk improvements applied: consolidated ranking loops, pre-sized lists, tracked update path.

| Delivered | Notes |
|-----------|-------|
| EF Core reads | `AsNoTracking()` on `GetByIdAsync` and `GetAllAsync` |
| EF Core writes | Tracked `GetByIdForUpdateAsync` + `SaveChangesAsync`; no attach-on-detached |
| EF Core deletes | `ExecuteDeleteAsync` — one round trip, no existence pre-fetch |
| Async | All repository and service methods are async with cancellation tokens |
| Search ranking | Single pass for distances/min-max; `List.Sort` instead of chained LINQ |
| Response bounds | Search paging via `GetRange`; max `pageSize` 100 |

| Scope decision | Detail |
|----------------|--------|
| **Not implemented by design** | Search loads all rows, ranks in memory, then pages — **O(n log n)** CPU, **O(n)** memory |
| **Not implemented by design** | `GET /api/hotels` returns full table — no list pagination |
| **Out of scope for this PoC** | No load tests, benchmarks, or performance budgets in CI |
| **Future improvement** | Geo-bounded SQL or PostGIS to filter before ranking |
| **Future improvement** | Server-side pagination for list and search |
| **Future improvement** | Caching, read replicas, compiled queries |

---

## Security

**Status: Partially implemented** — practical PoC baselines; not a production security programme.

**Security review (final pass):** No SQL injection or mass-assignment issues found. Production error masking and scoped HTTP logging are in place. Low-risk hardening applied: search prompt max length (500), SHA-256 API key comparison, Production startup guard for write key, connection string removed from base config, package vulnerability audit in CI.

| Delivered | Notes |
|-----------|-------|
| Input validation | FluentValidation plus domain rules; hotel name max 200; search prompt max 500 |
| Configuration | Secrets via environment variables; base `appsettings.json` has no connection string |
| Write protection | `X-Api-Key` on POST/PUT/DELETE; SHA-256 hash compared with `FixedTimeEquals` |
| Production guard | Startup fails if `Production` and `ApiKey:WriteKey` is unset |
| Error disclosure | Production `500` responses omit stack traces and exception detail |
| Dependencies | CI audits NuGet packages with `--vulnerable --include-transitive` |

| Scope decision | Detail |
|----------------|--------|
| **Not implemented by design** | Read and search endpoints are public |
| **Not implemented by design** | API key is a single shared secret when enabled, not per-client credentials |
| **Not implemented by design** | Docker Compose runs `Development` for reviewer convenience (Swagger, open writes unless `.env` sets key) |
| **Out of scope for this PoC** | No OAuth/JWT, RBAC, user accounts, or rate limiting |
| **Out of scope for this PoC** | No CORS policy or security headers (HSTS, CSP) in the app |
| **Future improvement** | TLS termination at reverse proxy; not configured in-app |
| **Future improvement** | Rate limiting on public search/list endpoints |
| **Future improvement** | Geo-bounded search at DB layer to reduce DoS surface on large catalogues |

---

## Test coverage

**Status: Partially implemented** — strong automated coverage for assignment scope; not exhaustive production QA.

| Delivered | Notes |
|-----------|-------|
| Automated tests | **112** tests — domain, application, infrastructure parser, API validation, integration, security |
| Integration | Testcontainers against real PostgreSQL |
| Scenarios | CRUD, search, ranking, paging, validation, optional write auth |
| Parser | Theory tests for supported prompt formats |

| Scope decision | Detail |
|----------------|--------|
| **Out of scope for this PoC** | No performance or load tests |
| **Out of scope for this PoC** | No mutation testing or coverage thresholds in CI |
| **Future improvement** | Explicit integration test for Production `500` body masking |
| **Not implemented by design** | `HotelRepository` not unit-tested in isolation — exercised via integration tests |
| **Not implemented by design** | Assembly-level test parallelisation disabled for Testcontainers stability (slower, more reliable) |

Integration tests require Docker. Unit and validation tests do not.

---

## Documentation

**Status: Implemented** — sufficient for a reviewer to run and understand the project quickly.

| Document | Purpose |
|----------|---------|
| [README.md](../README.md) | Quick start, runbook, API summary, ranking, trade-offs |
| [architecture.md](architecture.md) | Design decisions, lifecycles, scope rationale |
| [api-examples.md](api-examples.md) | HTTP examples and error responses |
| [openapi/](openapi/) | Committed OpenAPI 3.1 spec |
| [sample-requests.http](sample-requests.http) / [postman/](postman/) | Manual API exploration |
| [ai-usage.md](ai-usage.md) | AI disclosure and prompt log |
| [testing.md](testing.md) | Test matrix and documented scenarios |
| [SUBMISSION.md](SUBMISSION.md) | Repo link and Lemax assignment mapping |

| Scope decision | Detail |
|----------------|--------|
| **Out of scope for this PoC** | No separate ADR files — decisions live in README and architecture.md |
| **Not implemented by design** | Interactive Swagger UI only in Development |

---

## Processes

**Status: Partially implemented** — CI and local reproducibility; no delivery pipeline.

| Delivered | Notes |
|-----------|-------|
| CI | GitHub Actions: restore, Release build, test, package audit, **Docker image build** |
| Local stack | Docker Compose with health checks |
| Migrations | Documented manual commands; auto-apply in Development only |
| Configuration | `.env.example` for Compose defaults |

| Scope decision | Detail |
|----------------|--------|
| **Out of scope for this PoC** | No CD, deploy, release, or smoke-test workflow |
| **Out of scope for this PoC** | No PR/issue templates or CONTRIBUTING.md |
| **Not implemented by design** | Migrations not executed in CI (tests use Testcontainers with their own DB lifecycle) |

---

## AI usage

**Status: Implemented** — disclosed and reviewed.

| Delivered | Notes |
|-----------|-------|
| Disclosure | [ai-usage.md](ai-usage.md) — tool, purpose, prompt log |
| Ownership | “Manual Review and Ownership” section documents review and test validation |
| Transparency | Distinguishes verbatim vs consolidated prompts; notes rejected suggestions |

| Scope decision | Detail |
|----------------|--------|
| **Not implemented by design** | Prompt 2 in ai-usage.md is a consolidated reconstruction of follow-ups, not a full session export (stated in that document) |

---

## Summary

| Criterion | Status | Primary scope boundary |
|-----------|--------|------------------------|
| Functionality | **Implemented** | Regex-limited prompts; CRUD-only data source |
| Technical design | **Implemented** | PoC layering; in-memory search |
| Technology | **Implemented** | No auxiliary infra (cache, bus, search engine) |
| Standards | **Partially implemented** | No URL versioning, OTel, or split health probes |
| Coding style | **Partially implemented** | No StyleCop / CI format gate |
| Source code organization | **Implemented** | — |
| Performance | **Partially implemented** | O(n) search; no load testing |
| Security | **Partially implemented** | Optional API key only; public reads |
| Test coverage | **Partially implemented** | 112 tests; matrix in testing.md |
| Documentation | **Implemented** | — |
| Processes | **Partially implemented** | CI build/test/package; no CD (expected for take-home) |
| AI usage | **Implemented** | — |

### Overall assessment

The submission **meets the assignment’s functional and architectural requirements** at PoC depth: CRUD, PostgreSQL persistence, search with distance and ranking, Docker, tests, and Clean Architecture are all present and runnable.

What is **not** here — URL versioning, production auth, geo indexing, load testing, deploy pipelines — reflects **documented scope decisions**, not unfinished requirements. Those items are listed above with an explicit label and expanded in [README — Trade-offs](../README.md#trade-offs) and [Future improvements](../README.md#future-improvements).

**Reviewer path:** [README — Quick start](../README.md#quick-start-5-minutes) → `docker compose up --build` → `dotnet test -c Release` → this checklist → [ai-usage.md](ai-usage.md).
