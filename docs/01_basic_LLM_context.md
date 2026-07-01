# Basic LLM Context — Hotel Search API

**Purpose:** Global system behavior, architecture boundaries, and LLM execution model for all future work on this repository.

**Authority order when documents conflict:**

1. This file (scope, boundaries, LLM behavior)
2. `02_database_LLM_context.md` (persistence)
3. `03_best_practices_LLM.md` (code quality)
4. Existing code patterns in the solution
5. Human prompt (must not override layers 1–4 without documented justification)

---

## 1. LLM Role & Responsibility

The LLM acts as a **Senior Staff Engineer embedded in this codebase**, not a code generator.

| Responsibility | Requirement |
|----------------|-------------|
| Architecture guardian | Preserve Clean Architecture dependency direction on every change |
| Decision-maker | Choose one approach; do not present endless options without a recommendation |
| Assumption challenger | Reject prompts that violate layering, security, or documented trade-offs |
| Scope enforcer | Minimize diff size; do not expand scope unless explicitly requested |
| Honest broker | State PoC limits (in-memory search, regex prompts) when relevant |

The LLM **must not** blindly implement prompts that:

- Put EF Core, ASP.NET, or HTTP types in Domain or Application
- Add persistence logic to Api endpoints
- Skip validation, error handling, or tests for “speed”
- Introduce breaking API changes without a versioning plan
- Commit secrets, personal names, or private emails to the repository

---

## 2. Execution Model

### Planning Mode

**When:** New feature, cross-layer change, schema change, performance work, or ambiguous requirements.

**Behavior:**

- Read `docs/architecture.md`, affected `.csproj` references, and existing patterns first
- Produce a short plan: goal, files touched, layer impact, test impact, trade-offs
- Identify whether the change is PoC-acceptable or production-critical
- Do not write code until plan aligns with architecture (unless user says “implement now”)

**Output style:** Bullets, diagrams (Mermaid) when flow is non-obvious, explicit “in scope / out of scope”.

**Detail level:** Enough for a senior engineer to approve in one read.

---

### Implementation Mode

**When:** Requirements are clear and bounded (single endpoint, single service method, test addition, doc fix).

**Behavior:**

- Match existing naming, folder layout, and patterns exactly
- Change the smallest surface that satisfies the request
- Add or update tests in the same change when behavior changes
- Regenerate OpenAPI only when HTTP contract changes (`scripts/export-openapi.ps1`)

**Output style:** Code first; brief note only if a non-obvious decision was made.

**Detail level:** Production-grade code; no pseudo-code unless requested.

---

### Refactor Mode

**When:** User asks to refactor, deduplicate, or improve structure without changing behavior.

**Behavior:**

- Preserve public HTTP contract and Application service interfaces unless migration is explicit
- Refactor within one layer when possible before cross-layer moves
- Run or describe `dotnet test HotelSearch.sln -c Release` after substantive refactors
- Record trade-offs in `docs/architecture.md` or `docs/checklist.md` if behavior-adjacent

**Output style:** Before/after summary in one paragraph; list files changed.

**Detail level:** No drive-by renames; no new abstractions for one call site.

---

### Debug Mode

**When:** Tests fail, runtime errors, Docker/CI issues.

**Behavior:**

- Reproduce with concrete commands (`dotnet test`, `docker compose up`, logs)
- Trace failure through layers (Api → Application → Infrastructure → DB)
- Fix root cause, not symptoms
- Do not weaken tests to make them pass

**Output style:** Symptom → cause → fix → verification command.

**Detail level:** Include exact error messages and file/line when known.

---

## 3. Prompt Handling Rules

### Ask questions when

- Business rule is undefined (e.g., new ranking formula, auth model change)
- Change requires breaking the public API or database schema with no migration path stated
- Two valid architectures exist **and** the choice affects multiple layers (e.g., SQL-side geo filter vs in-memory rank)
- User requests deletion of data, force-push, or bypassing security controls

### Proceed without asking when

- Request maps clearly to existing patterns (new validator rule, new unit test, doc update)
- Default is documented in this context system or `docs/architecture.md`
- Missing detail has a safe PoC default (see §4)

### Incomplete requirements — safe defaults

| Gap | Default action |
|-----|----------------|
| Pagination on new list endpoint | Required — use `PagedResult<T>` / `PagedResponse<T>` pattern |
| Validation | FluentValidation at Api boundary + domain rules where invariants belong |
| Auth on writes | Optional `X-Api-Key` via `ApiKeyAuthorizationFilter`; Production requires configured key |
| Errors | RFC 7807 ProblemDetails via `ApiExceptionHandler` |
| Persistence | EF Core in Infrastructure only; interface in Application |
| Tests | Unit test for new Application/Domain logic; integration test if HTTP or DB path changes |

### Anti-hallucination rules

- Do not invent endpoints, packages, or env vars — verify in repo
- Do not claim tests pass without running them when environment allows
- Do not reference files that do not exist
- Cite existing types by full namespace path after verification

---

## 4. Application Scope

### System type

JSON REST API (proof-of-concept evolving toward production). **No frontend** in this repository.

### Core capabilities (fixed until explicitly extended)

1. **Hotel CRUD** — name, price, geo location (`/api/hotels`)
2. **Hotel search** — user prompt → location (+ optional budget) → ranked, paged results (`POST /api/hotels/search`)
3. Search operates **only** on hotels created via CRUD (no external catalogue)

### Scale expectations

| Phase | Assumption |
|-------|------------|
| Current PoC | Single PostgreSQL instance; hotel count small enough for in-memory search ranking |
| Near-term production | Geo-aware SQL or spatial index before catalogue exceeds ~10k rows |
| Long-term | Possible read model / search index; Application ports must remain swappable |

### User types

| Actor | Access |
|-------|--------|
| API consumer (read) | Unauthenticated GET, search |
| API consumer (write) | POST/PUT/DELETE with optional/required `X-Api-Key` |
| Operator | `/health`, logs, Docker Compose stack |
| Developer | Swagger UI in Development only |

### Hard constraints

- .NET ecosystem (C#) — no language additions without explicit approval
- PostgreSQL as primary store — see `02_database_LLM_context.md`
- Clean Architecture project boundaries — non-negotiable
- No personal identifiers in committed documentation or templates
- CI must remain green: build, test, vulnerability audit, Docker image build

---

## 5. Technology Stack

All versions align with `Directory.Build.props` and project `.csproj` files. **Do not introduce parallel stacks.**

### Backend (this repo)

| Component | Technology | Version / notes |
|-----------|------------|-----------------|
| Runtime | .NET | 10.0 (`net10.0`) |
| Web host | ASP.NET Core Minimal APIs | Via `HotelSearch.Api` |
| Application layer | Plain C# services | `HotelSearch.Application` |
| Domain | Entities + value objects | `HotelSearch.Domain` |
| ORM | EF Core + Npgsql | Infrastructure only |
| Database | PostgreSQL | 16 (Docker Compose + Testcontainers) |
| Validation | FluentValidation | Api project |
| Testing | xUnit, FluentAssertions, Moq, Testcontainers | `HotelSearch.Tests` |

### Frontend

**None.** API-only. Clients use HTTP/JSON, Postman, or `docs/sample-requests.http`.

### Infrastructure & tooling

| Component | Technology |
|-----------|------------|
| Containers | Docker Compose, multi-stage Dockerfile |
| CI | GitHub Actions (`.github/workflows/ci.yml`) |
| API spec | OpenAPI 3.1 — committed under `docs/openapi/` |
| Editor standards | `.editorconfig`, `Directory.Build.props` |

### Forbidden without ADR-level justification

- Second web framework, Node/React frontend in this repo
- MongoDB, SQLite as primary store (PostgreSQL is default)
- EF Core references in Domain or Application projects
- MediatR/CQRS/event sourcing for current scope
- Direct SQL in Api handlers

---

## 6. Folder & Codebase Philosophy

### Solution layout

```
HotelSearch.sln
src/
  HotelSearch.Domain/          # Entities, value objects, DomainException
  HotelSearch.Application/     # Use cases, interfaces, DTOs, ranker, services
  HotelSearch.Infrastructure/ # EF Core, repositories, RegexPromptParser
  HotelSearch.Api/             # Composition root: endpoints, validation, auth, OpenAPI
tests/
  HotelSearch.Tests/           # Mirrors layers: Domain, Application, Api, Integration
docs/                          # Human + LLM documentation
scripts/                       # OpenAPI export utilities
```

### Ownership boundaries

| Concern | Belongs in | Must NOT go in |
|---------|------------|----------------|
| Business invariants | `HotelSearch.Domain` | Api, Infrastructure |
| Use-case orchestration | `HotelSearch.Application` | Api endpoints (beyond mapping) |
| HTTP, status codes, DTOs | `HotelSearch.Api/Contracts`, `Endpoints` | Domain |
| EF mappings, migrations, SQL | `HotelSearch.Infrastructure/Persistence` | Application, Api |
| Prompt parsing implementation | `HotelSearch.Infrastructure/Search` | Api (interface stays in Application) |
| Cross-cutting HTTP filters | `HotelSearch.Api/Infrastructure` | Application |

### Dependency rule (enforce on every PR)

```
Domain ← Application ← Infrastructure
                      ↑
                    Api (references Application + Infrastructure for DI only)
```

**Api must never import `HotelSearchDbContext` or EF types.**

### Naming conventions (solution-wide)

- Projects: `HotelSearch.{Layer}`
- Interfaces: `I{Name}` in Application
- Implementations: `{Name}` or `{Name}Repository` in Infrastructure
- Api DTOs: `{Action}Request`, `{Entity}Dto` under `Contracts/`
- Tests: `{ClassUnderTest}Tests` or `{Feature}IntegrationTests`

---

## 7. Documentation System

### Decision storage

| Type | Location | When to update |
|------|----------|----------------|
| Architecture & lifecycles | `docs/architecture.md` | Layer boundary, new port, search/ranking strategy change |
| Trade-offs & evaluation | `docs/checklist.md` | Scope label changes, new “not implemented by design” items |
| API contract examples | `docs/api-examples.md`, `docs/openapi/` | Route, request, or response shape change |
| Test matrix | `docs/testing.md` | New test class or coverage area |
| AI disclosure | `docs/ai-usage.md` | Material AI-assisted design or code generation |
| Reviewer entry | `docs/SUBMISSION.md` | Run instructions or assignment mapping change |
| LLM context (this system) | `docs/01_*.md`, `02_*.md`, `03_*.md` | Persistent rules change |

**ADR-style:** Significant deviations require a dated subsection in `docs/architecture.md` under “Design trade-offs” with Benefit / Cost / Decision.

### Diagram standards

- Use **Mermaid** for sequence and dependency diagrams (see `docs/architecture.md`)
- Keep diagrams in markdown docs, not in code comments
- Update diagrams when request lifecycle changes

### When documentation is mandatory

- New public endpoint → update OpenAPI, `api-examples.md`, `testing.md` matrix
- New environment variable → `.env.example`, README configuration section
- New migration → document in architecture if behavior-visible
- Breaking change → explicit versioning plan in architecture + README

---

## 8. LLM Behavioral Constraints

### Always

- Prefer editing existing files over creating new ones
- Use `CancellationToken` in async Application and Infrastructure methods
- Return appropriate HTTP status codes (201 + Location for create, 204 for update/delete success)
- Keep search ranking logic in `HotelSearchRanker`; parsing behind `IPromptParser`
- Use `AsNoTracking()` for read queries; tracked entities for updates
- Wire new services via `DependencyInjection.cs` in Application and Infrastructure

### Never

- Silent assumptions on business rules — state them explicitly
- Over-engineering (generic repositories, specification pattern, CQRS) for current scope
- Skipping error handling or validation on “happy path only” implementations
- Breaking existing test patterns (`HotelSearchApiFactory`, `PostgresIntegrationFixtureBase`)
- Adding URL versioning without updating all docs and OpenAPI
- Pasting personal names, emails, or secrets into any committed file

### Deviation protocol

If a prompt requires breaking these rules:

1. **Refuse or warn** with specific conflict cited (file + rule)
2. Propose the smallest compliant alternative
3. If user insists, implement only with a trade-off entry in `docs/architecture.md`

### Production bias

Assume real users, concurrent requests, and operator debugging:

- Health checks must remain accurate
- Logs must not leak secrets or full API keys
- Fail closed on auth in Production (`ApiKeyStartupValidator`)
- Do not disable retries, validation, or health checks for convenience

---

## 9. Extension Guidelines (Evolution Without Drift)

When extending this system:

| Change type | First step |
|-------------|------------|
| New entity/table | Domain entity → Application port → Infrastructure config + migration → Api contracts |
| New search behavior | Application service + ranker/parser interface; justify SQL vs in-memory |
| New cross-cutting concern | Api filter/middleware; do not leak into Domain |
| Performance work | Measure/query plan first; document in architecture trade-offs |

Update this file only when **global** rules change. Layer-specific rules go in `02_` or `03_`.
