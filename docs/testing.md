# Testing

How the Hotel Search API is tested, what each layer covers, and how to run the suite.

**Quick run (from repository root):**

```bash
dotnet test HotelSearch.sln -c Release
```

Integration tests require **Docker** (Testcontainers starts PostgreSQL automatically).

---

## Test summary

| Metric | Value |
|--------|-------|
| Framework | xUnit |
| Total tests | 112 (run `dotnet test` for current count) |
| CI | GitHub Actions — restore, build, test, package audit, Docker build |
| Parallelization | Disabled at assembly level (Testcontainers stability) |

---

## Test matrix

### Interface 1 — Hotel CRUD

| Scenario | Unit | API (mocked) | Integration (PostgreSQL) |
|----------|------|--------------|---------------------------|
| Create hotel — success (`201`, body, `Location`) | `HotelServiceTests` | — | `HotelApiIntegrationTests` |
| Create — empty name / invalid price / bad coordinates | `HotelTests`, domain | `HotelApiValidationTests` | `HotelApiIntegrationTests` |
| Get by id — success | `HotelServiceTests` | — | `HotelApiIntegrationTests` |
| Get by id — `404` | `HotelServiceTests` | — | `HotelApiIntegrationTests` |
| List all hotels | `HotelServiceTests` | — | `HotelApiIntegrationTests` |
| Update — success (`204`) | `HotelServiceTests` | — | `HotelApiIntegrationTests` |
| Update — `404` | `HotelServiceTests` | `HotelApiValidationTests` | — |
| Delete — success (`204`) | — | — | `HotelApiIntegrationTests` |
| Delete — `404` | `HotelServiceTests` | `HotelApiValidationTests` | — |

### Interface 2 — Search

| Scenario | Unit | API (mocked) | Integration (PostgreSQL) |
|----------|------|--------------|---------------------------|
| Returns name, price, distance, id | `HotelSearchServiceTests` | — | `HotelApiIntegrationTests` |
| Cheaper + closer ranked first | `HotelSearchRankerTests`, `HotelSearchServiceTests` | — | `HotelApiIntegrationTests` |
| Budget penalty (over budget ranked lower) | `HotelSearchRankerTests`, `HotelSearchServiceTests` | — | — |
| Paging (`page`, `pageSize`, `totalCount`, `totalPages`) | `HotelSearchServiceTests` | — | `HotelApiIntegrationTests` |
| Only CRUD-created hotels in results | — | — | `HotelApiIntegrationTests` |
| Empty prompt / invalid page / page size | `HotelSearchServiceTests` | `HotelApiValidationTests` | — |
| Prompt too long | — | `HotelApiValidationTests` | — |
| Invalid prompt (no location) | `RegexPromptParserTests` | `HotelApiValidationTests` | `HotelApiIntegrationTests` |
| Negative budget in prompt | `RegexPromptParserTests` | `HotelApiValidationTests` | — |
| Empty catalogue — `200` with empty items | — | `HotelApiValidationTests` | `HotelApiIntegrationTests` |

### Cross-cutting

| Scenario | Unit | API (mocked) | Integration |
|----------|------|--------------|-------------|
| `/health` + database status | — | — | `HotelApiIntegrationTests` |
| Optional `X-Api-Key` on writes — `401` / success | `ApiKeyAuthorizationFilterTests`, `ApiKeyStartupValidatorTests` | — | `HotelApiSecurityTests` |
| Production requires write key at startup | `ApiKeyStartupValidatorTests` | — | — |
| FluentValidation error shape (`400`) | — | `HotelApiValidationTests` | `HotelApiIntegrationTests` |

### Domain & infrastructure

| Area | Test class | Notes |
|------|------------|-------|
| `GeoLocation` — validation, Haversine | `GeoLocationTests` | Distance used in search ranking |
| `Money` — non-negative | `MoneyTests` | |
| `Hotel` — create invariants, `UpdateDetails` | `HotelTests` | |
| Prompt parser formats | `RegexPromptParserTests` | Theory tests per supported pattern |

---

## Project layout

```
tests/HotelSearch.Tests/
  Domain/           # Value objects and entity rules
  Application/      # Services and ranker (Moq for repositories)
  Infrastructure/   # RegexPromptParser
  Api/              # Validation and filters (WebApplicationFactory + mocks)
  Api/Integration/  # Testcontainers + real HTTP
```

---

## Running subsets

```bash
# All tests
dotnet test HotelSearch.sln -c Release

# Domain only
dotnet test tests/HotelSearch.Tests --filter "FullyQualifiedName~HotelSearch.Tests.Domain"

# Integration only (Docker required)
dotnet test tests/HotelSearch.Tests --filter "FullyQualifiedName~Integration"
```

---

## What is not covered

| Area | Reason |
|------|--------|
| Load / performance tests | Out of scope for PoC |
| `HotelRepository` in isolation | Exercised via integration tests |
| Production `500` body masking | Manual / future integration test |
| OpenAPI contract tests | Committed spec; regenerate with `scripts/export-openapi.ps1` |

---

## Negative and corner cases (assignment checklist)

- Invalid coordinates (domain, API validation, DB constraints)
- Empty / whitespace hotel name
- Zero or negative price
- Search with no hotels in database
- Search page beyond last page
- Search prompt without extractable location
- Optional API key missing or wrong on protected writes
- Delete/update non-existent hotel id
