# Database LLM Context — Hotel Search API

**Purpose:** Data integrity, persistence strategy, and performance constraints for PostgreSQL + EF Core in this solution.

**Scope:** All persistence work flows through `HotelSearch.Infrastructure`. Application code depends on `IHotelRepository` only.

---

## 1. Database Philosophy

### Primary database (mandatory)

**PostgreSQL 16** — single relational store for hotel data.

| Rationale | Detail |
|-----------|------------|
| Assignment alignment | Persistent hotel CRUD with geo + price |
| Stack consistency | Npgsql + EF Core already integrated |
| Operational path | Docker Compose locally; Testcontainers in CI |
| Integrity | Check constraints enforce geo and price rules at DB level |

### Alternatives — allowed only when

| Alternative | Condition | Requirement |
|-------------|-----------|-------------|
| PostGIS / spatial queries | Catalogue scale requires SQL-side distance filter | Keep `IHotelRepository`; extend Infrastructure; document in architecture |
| Read replica | Read load dominates | Decorator repository; no Domain change |
| Cache (Redis) | Hot read paths proven by metrics | Cache behind repository interface; TTL + invalidation documented |
| Dapper / raw SQL | Identified hot path; EF generates poor SQL | Inside `HotelRepository` only; interface unchanged |
| Elasticsearch / external search | Full-text beyond regex prompts | New Application port; not a replacement for CRUD store |

### Forbidden as primary store

SQLite, in-memory EF provider, MongoDB, file-based JSON persistence — unless explicit human approval with migration plan.

---

## 2. Schema Design Rules

### Naming conventions (STRICT)

| Element | Convention | Example |
|---------|------------|---------|
| Table names | lowercase, plural, snake_case | `hotels` |
| Column names | lowercase, snake_case | `id`, `name`, `price`, `latitude`, `longitude` |
| Primary key constraint | EF default `PK_{table}` | `PK_hotels` |
| Check constraints | `CK_{table}_{purpose}` | `CK_hotels_price_positive` |
| EF configuration class | `{Entity}Configuration` | `HotelConfiguration` |
| Migration timestamp | EF-generated UTC prefix | `20260701161053_InitialCreate` |

### ID strategy

| Rule | Value |
|------|-------|
| Primary key type | **`Guid` (`uuid` in PostgreSQL)** |
| Generation | **Application-assigned** (`ValueGeneratedNever()` on `Hotel.Id`) |
| Rationale | Stable IDs across environments; no identity column leakage |

**Do not switch to `int` identity** without migration strategy and API contract review (routes use `{id:guid}`).

### Timestamp conventions

**Current schema:** No `created_at` / `updated_at` columns.

| Rule | Action |
|------|--------|
| Adding audit columns | Requires migration + explicit product decision; document in architecture |
| Default until then | Do not assume temporal ordering in queries |

### Soft delete policy

**Not implemented.** Deletes are hard deletes via `ExecuteDeleteAsync`.

| Rule | Action |
|------|--------|
| New soft delete request | Add `deleted_at` nullable column + filter in repository; update all queries; document breaking behavior |
| Default | Physical delete returns `404` on subsequent GET |

### Value object mapping

Owned types map to columns on parent table (not separate tables):

- `Money.Amount` → `price` (`numeric(18,2)`)
- `GeoLocation.Latitude` / `Longitude` → `latitude` / `longitude` (`double precision`)

---

## 3. Data Integrity Rules

### Database-level (required for new constraints)

| Constraint | Implementation |
|------------|----------------|
| Price positive | `CK_hotels_price_positive`: `price > 0` |
| Latitude range | `CK_hotels_latitude_range`: `-90` to `90` |
| Longitude range | `CK_hotels_longitude_range`: `-180` to `180` |
| Name length | `varchar(200)` + NOT NULL |
| Primary key | `id` NOT NULL |

New check constraints **must** be mirrored in:

1. `HotelConfiguration.cs`
2. EF migration `Up`/`Down`
3. Domain/Application validation where user-facing messages differ

### Foreign keys

**None currently** (single-table PoC). When relationships are added:

- Explicit FK in migration
- Navigation only in Infrastructure; Domain references by ID unless true aggregate boundary

### Validation boundaries

| Layer | Validates |
|-------|-----------|
| **Api (FluentValidation)** | Request shape, string lengths, numeric ranges, prompt length (max 500 chars) |
| **Domain** | Business invariants (`Hotel` name non-empty, `Money` non-negative, `GeoLocation` bounds) |
| **Database** | Last line of defense — constraints must not contradict Domain rules |

**Rule:** Never rely on DB alone for user-facing errors; Application/Api must surface meaningful ProblemDetails.

---

## 4. Data Access Strategy

### ORM default: EF Core

All standard CRUD and query work uses **EF Core 10** via `HotelSearchDbContext`.

| Location | Allowed |
|----------|---------|
| `HotelSearch.Infrastructure/Persistence/` | DbContext, configurations, migrations, repositories |
| `HotelSearch.Application` | **No EF references** |
| `HotelSearch.Api` | **No EF references** |

### Raw SQL / Dapper — allowed when

- Performance profiling shows EF bottleneck on a specific query
- Geo-distance filtering moves to SQL/PostGIS
- Implementation stays inside `HotelRepository` (or dedicated Infrastructure query class)
- Parameterized queries only — **no string concatenation of user input**

### Repository pattern — REQUIRED

| Aspect | Rule |
|--------|------|
| Interface | `IHotelRepository` in `HotelSearch.Application/Hotels` |
| Implementation | `HotelRepository` in Infrastructure — `internal sealed` |
| Registration | `DependencyInjection.AddInfrastructure` |
| Testing | Mock interface in Application tests; real DB in Testcontainers integration tests |

**Why:** Keeps Application testable and persistence swappable per `docs/architecture.md`.

**Forbidden:** Generic `IRepository<T>`, exposing `IQueryable<T>` to Application, or DbContext injection into Application services.

### Current repository contract

```
GetByIdAsync              → AsNoTracking read
GetByIdForUpdateAsync     → tracked read (updates)
GetAllAsync               → AsNoTracking, OrderBy Name
AddAsync                  → insert + SaveChanges
SaveChangesAsync          → persist tracked changes
DeleteAsync               → ExecuteDeleteAsync (single round-trip)
```

New methods must follow read vs write tracking rules above.

### Connection resilience

Npgsql retry policy is configured in `DependencyInjection.AddInfrastructure`:

- Max 5 retries, 10s max delay
- Do not remove without replacement strategy

---

## 5. Migration Strategy

### Tooling

```bash
dotnet ef migrations add {Name} \
  --project src/HotelSearch.Infrastructure \
  --startup-project src/HotelSearch.Api
```

Design-time factory: `HotelSearchDbContextFactory`.

### Forward-only in production

| Environment | Migration application |
|-------------|----------------------|
| **Development** | Auto-apply on startup via `DatabaseInitializer.ApplyMigrationsAsync` with retry |
| **Testing** | Testcontainers fixture applies migrations per test host |
| **Production** | **Explicit migration step** (CI/CD job or runbook) — not auto-apply on app boot |
| **CI build job** | Does not run migrations against shared DB (Testcontainers only) |

### Safe migration practices (mandatory)

| Practice | Rule |
|----------|------|
| Review generated SQL | Inspect migration before commit for large tables |
| Backward-compatible deploys | Add column nullable first → backfill → enforce NOT NULL in later migration when needed |
| Destructive changes | Require data migration script + human approval |
| Down migrations | Keep EF `Down` implemented but **do not rely on Down in production** |
| Index creation | Use `CONCURRENTLY` pattern for large tables (manual SQL migration when scale demands) |

### LLM must warn before

- `DROP TABLE`, `DROP COLUMN`, type narrowing
- Renaming columns without compatibility view/phase
- Removing check constraints
- Data truncation in tests that could be copied to prod scripts

---

## 6. Performance Rules

### Current PoC characteristics

| Operation | Pattern |
|-----------|---------|
| List / Get by id | Single query, `AsNoTracking()` |
| Update | Tracked fetch + `SaveChangesAsync` |
| Delete | `ExecuteDeleteAsync` (no load-then-delete) |
| Search | **`GetAllAsync()` + in-memory rank + page slice** |

### Indexing requirements

| Table | Current indexes | Future |
|-------|-----------------|--------|
| `hotels` | Primary key on `id` | Add index on `(latitude, longitude)` or PostGIS GIST when geo SQL is introduced |

**Rule:** New WHERE/ORDER BY columns used at scale **must** have index justification in migration notes.

### Pagination rules (never optional for search)

Search endpoint **must** paginate:

- Request: `page`, `pageSize` (validated via `SearchConstants`)
- Response: `items`, `page`, `pageSize`, `totalCount`, `totalPages`

**Current limitation:** Pagination applies **after** loading and ranking all hotels in memory. Acceptable for PoC only.

**Scale gate:** When hotel count exceeds PoC threshold (~10k) or latency SLO breached:

1. Move filtering/ranking to SQL or search index
2. Document in `docs/architecture.md`
3. Do not silently increase `pageSize` max without validation review

### Query optimization expectations

- No N+1 on current single-table model
- Avoid `SELECT *` patterns that load unnecessary columns when adding wide tables
- Profile before micro-optimizing LINQ
- Prefer `ExecuteDeleteAsync` / `ExecuteUpdateAsync` over fetch-delete for bulk ops

---

## 7. Environment Strategy

### Configuration files

| File | Purpose |
|------|---------|
| `appsettings.json` | Base logging; **no connection string** |
| `appsettings.Development.json` | Local connection string, dev settings |
| `appsettings.Testing.json` | Test host fallback connection string |
| `appsettings.Production.json` | Production overrides |
| `.env.example` | Docker Compose variable template |

### Connection string key

`ConnectionStrings:DefaultConnection` — required for Infrastructure registration.

Test hosts inject via:

- `appsettings.Testing.json` fallback
- `IntegrationWebApplicationFactory` / `HotelSearchApiFactory` in-memory config
- Testcontainers dynamic connection string for integration fixtures

### Secrets handling (STRICT)

| Rule | Detail |
|------|--------|
| Never commit | `.env`, real passwords, API keys, personal emails |
| Api key | `ApiKey:WriteKey` via environment / user secrets / deployment secrets |
| Production guard | `ApiKeyStartupValidator` fails startup if write key missing in Production |
| Docker | Compose reads from `.env` (gitignored) |

### Environment separation

| Env | Database |
|-----|----------|
| Local dev | PostgreSQL container (`docker-compose.yml`) |
| Integration tests | Ephemeral Testcontainers PostgreSQL per fixture |
| CI | Testcontainers on ubuntu-latest |
| Production | Dedicated PostgreSQL instance (not defined in repo) |

**Rule:** Integration tests **truncate** `hotels` via `TRUNCATE TABLE hotels;` — never point tests at production.

---

## 8. LLM Database Behavior Rules

### Before any schema change, LLM must

1. State impact on existing data and API
2. Generate EF migration + update `HotelConfiguration`
3. Confirm Domain entity alignment
4. Add/update integration test if CRUD or search path affected
5. Note whether Development auto-migrate covers local workflow

### Must warn when prompt implies

- Full table scan on every request at production scale (current search pattern)
- Removing retry policy or health check DB probe
- Storing secrets in columns
- Using float for money (use `numeric(18,2)`)
- Client-generated IDs without uniqueness handling

### Must justify in architecture doc when

- Adding tables or relationships
- Changing ranking to SQL
- Introducing soft delete or audit columns
- Changing ID type or delete semantics

### Test database hygiene

- Use `PostgresIntegrationFixtureBase.ResetDatabaseAsync` pattern between tests
- Migrations applied once per fixture via `DatabaseInitializer.ApplyMigrationsAsync`
- Do not share static DB state across test classes without collection fixture

---

## 9. Quick Reference — Current Schema

```text
Table: hotels
  id          uuid          PK, app-assigned
  name        varchar(200)  NOT NULL
  price       numeric(18,2) NOT NULL, CHECK > 0
  latitude    double        NOT NULL, CHECK [-90, 90]
  longitude   double        NOT NULL, CHECK [-180, 180]
```

Entity: `HotelSearch.Domain.Hotels.Hotel`  
DbContext: `HotelSearch.Infrastructure.Persistence.HotelSearchDbContext`  
Repository: `HotelSearch.Infrastructure.Persistence.Repositories.HotelRepository`
