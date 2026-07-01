# AI Usage

This document discloses how AI tools were used while building the Lemax Hotel Search API take-home assignment. The primary tool was **Cursor** (agent-assisted editing in the IDE).

AI was used as a **development accelerator**. All output was reviewed, tested, and adjusted before acceptance. Final technical decisions and ownership of the submission remain with the author.

---

## How AI Was Used

| Activity | Role of AI |
|----------|------------|
| **Break down requirements** | Parsed the assignment brief into layers (Domain, Application, Infrastructure, Api), endpoints, and test categories |
| **Plan architecture** | Proposed Clean Architecture project layout, dependency direction, and separation of HTTP vs persistence concerns |
| **Generate implementation prompts** | Iterative prompts scoped each phase (scaffold → features → security → production → docs) to keep changes reviewable |
| **Review edge cases** | Surfaced validation gaps, ranking tie-breaks, paging limits, budget penalty behaviour, and error-response consistency |
| **Improve tests** | Generated unit/integration test scaffolding; identified Testcontainers fixture isolation issues and API-key coverage |
| **Review documentation** | Drafted and refined README and `/docs` content to match the implemented behaviour |

---

## Full Prompt Log

Prompts below are listed in development order.

- **Prompt 1** — verbatim from the archived Cursor session export.
- **Prompt 2** — consolidated from iterative follow-up messages after scaffolding (exact wording varied; substance is faithful to what was requested).
- **Prompts 3–6** — verbatim from continued development in the same assignment workflow.

Minor formatting normalisation (line breaks, list markers) was applied for readability; meaning is unchanged.

---

### Prompt 1 — Solution scaffold and Docker setup

```
You are helping me build a take-home assignment for Lemax: a .NET JSON REST API for hotel search.

Build this as a professional proof-of-concept using C#, ASP.NET Core, PostgreSQL, EF Core, and Docker Compose.

Core requirements:
CRUD API for hotel data
Hotel has: name, price, geo location
Search API returns hotels created through the CRUD API only
Search accepts a user prompt from which location and budget can be extracted
Search output includes hotel name, price, and distance from the user's current location
Results are ordered so cheaper and closer hotels rank higher
Search supports paging

Architecture requirements:
Use Clean Architecture
Use DDD-inspired domain modelling where appropriate
Keep Domain and Application independent from ASP.NET Core, EF Core, and PostgreSQL
Infrastructure should contain EF Core persistence
API should only handle HTTP concerns

Create the solution structure:

/src
  HotelSearch.Api
  HotelSearch.Application
  HotelSearch.Domain
  HotelSearch.Infrastructure

/tests
  HotelSearch.Tests

/docs
  ai-usage.md
  architecture.md
  api-examples.md

Also add:
docker-compose.yml
Dockerfile for the API
.env.example if useful
README.md
.github/workflows/ci.yml placeholder

Use PostgreSQL from the start via Docker Compose.

Do not implement all business logic yet. First create the project structure, references, Docker setup, and a short README section explaining how to run the empty API.
```

---

### Prompt 2 — Full implementation (domain through integration tests)

*Consolidated from iterative follow-ups after Prompt 1.*

```
Implement the full assignment on top of the scaffold:

- Domain: Hotel aggregate, GeoLocation, Money value objects, Haversine distance, domain validation
- Application: CRUD and search services, ranking (normalized price + distance, optional budget penalty), paging
- Infrastructure: EF Core DbContext, repository, migrations, regex-based prompt parser for documented formats
- API: Minimal API endpoints, FluentValidation, RFC 7807 ProblemDetails, Swagger in Development
- Tests: domain/application unit tests, API validation tests, Testcontainers integration tests for CRUD and search

Keep Clean Architecture boundaries. Search ranks cheaper and closer hotels higher with deterministic tie-breaks. Update README and docs as features land.
```

---

### Prompt 3 — Security (PoC)

```
Add reasonable security practices for a PoC.

Implement:
Input validation
Defensive programming
No leaking stack traces in production responses
Secure configuration via environment variables
Optional simple API key authentication for write operations if this does not overcomplicate the solution

If API key auth is added:
Protect POST/PUT/DELETE endpoints
Leave search/read endpoints public
Document how to set the API key
Add tests for unauthorized write requests

Keep this practical. Do not turn the assignment into an enterprise auth project.
```

---

### Prompt 4 — Production readiness

```
Add production-readiness features.

Implement:
Structured logging
Health check endpoint at /health
Database health check
Request logging if appropriate
Clear startup logs
CI workflow that builds and runs tests

Update README with:
Health check endpoint
Logging notes
CI notes
Production-readiness trade-offs
```

---

### Prompt 5 — Documentation

```
Create strong documentation.

Update README.md with:
Project overview
Assignment requirements covered
Architecture overview
Technology choices
How to run with Docker Compose
How to run tests
How to apply migrations if needed
API endpoint list
Example curl requests
Search prompt examples
Ranking algorithm explanation
Known trade-offs
Future improvements

Update /docs:
architecture.md
api-examples.md
ai-usage.md

Keep documentation professional and concise.
```

---

### Prompt 6 — AI usage disclosure (this document)

```
Create /docs/ai-usage.md.

Explain that AI tools were used to:
Break down requirements
Plan architecture
Generate implementation prompts
Review edge cases
Improve tests
Review documentation

Include the full prompt log used during development.

Also include a section called "Manual Review and Ownership" explaining:
I reviewed and adjusted generated code
I made final technical decisions
I validated behavior with tests
I used AI as an accelerator, not as a replacement for understanding

Keep the tone professional and honest.
```

---

### Implicit follow-ups (not full prompts)

During implementation, shorter follow-ups were used when builds or tests failed—for example, fixing EF Core package alignment, registering `IPromptParser` in DI, correcting integration test assertions, and resolving Testcontainers parallel-test flakiness. These were diagnostic iterations rather than new feature specifications.

---

## Manual Review and Ownership

I reviewed and adjusted all generated code before treating it as part of this submission. Concretely:

- **I reviewed and adjusted generated code** — Every layer was read for correctness, naming consistency, and fit with Clean Architecture. Examples of manual changes: rejecting OAuth/JWT in favour of a simple optional API key; using built-in JSON console logging instead of a heavier logging stack; fixing separate PostgreSQL database names per test fixture to stop parallel test interference.
- **I made final technical decisions** — Ranking weights (0.5 price / 0.5 distance), over-budget penalty (+1.0), tie-break order, regex prompt formats, auto-migrations in Development only, and PoC scope boundaries were confirmed or overridden by me—not delegated blindly to the agent.
- **I validated behaviour with tests** — `dotnet build` and `dotnet test` were run repeatedly during development. The suite currently includes 96 tests covering domain rules, application ranking, prompt parsing, API validation, security (401 on writes), health checks, and full CRUD/search integration against real PostgreSQL via Testcontainers.
- **I used AI as an accelerator, not as a replacement for understanding** — I can explain the architecture, ranking algorithm, request flow, security model, and test strategy in an interview without AI assistance. AI reduced time on boilerplate and documentation drafts; engineering judgement, review, and verification remained human-driven.

---

## Tooling

| Tool | Usage |
|------|-------|
| **Cursor** | Primary IDE agent for code generation, refactors, test fixes, and documentation drafts |
| **dotnet CLI** | Build, test, and EF migrations (used by both author and agent) |
| **Docker / Docker Compose** | Local PostgreSQL and API; Testcontainers in CI and integration tests |

No proprietary, confidential, or employer-sensitive data was included in prompts.

---

## Candidate statement

This submission is my work. AI tools helped me move faster on scaffolding, implementation, tests, and documentation, but I remain responsible for the design, the code that was kept, and the behaviour demonstrated by the test suite.
