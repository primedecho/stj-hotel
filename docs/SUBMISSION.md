# Submission — Lemax take-home (Hotel search)

## Repository

**GitHub:** [https://github.com/primedecho/stj-hotel](https://github.com/primedecho/stj-hotel)

*(Single commit on submit — see README quick start for reviewers.)*

---

## Assignment mapping

| Problem statement | Implementation |
|-------------------|----------------|
| **1. CRUD** — name, price, geo location | `POST/GET/PUT/DELETE /api/hotels` |
| **2. Search** — prompt → geo + budget | `POST /api/hotels/search` + `RegexPromptParser` |
| Search returns **only CRUD hotels** | No external catalogue |
| Output: **name, price, distance** | JSON fields: `name`, `price`, `distanceKm` (+ `id`, `rankingScore`) |
| **Ordered** — cheaper and closer first | `HotelSearchRanker` (min-max price + distance) |
| **Bonus: paging** | `page`, `pageSize`, `totalCount`, `totalPages` |
| Persistence optional; easy to add later | `IHotelRepository` + EF Core PostgreSQL |
| .NET / C# | .NET 10, ASP.NET Core Minimal APIs |
| Clean architecture / DDD (strong plus) | Domain → Application ← Infrastructure / Api |
| AI tools disclosure | [ai-usage.md](ai-usage.md) |

---

## Run in 5 minutes

```bash
docker compose up --build
```

```bash
curl http://localhost:8080/health

curl -X POST http://localhost:8080/api/hotels \
  -H "Content-Type: application/json" \
  -d '{"name":"Grand Hotel","price":120,"latitude":45.815,"longitude":15.982}'

curl -X POST http://localhost:8080/api/hotels/search \
  -H "Content-Type: application/json" \
  -d '{"prompt":"near 45.8150, 15.9819 under 200","page":1,"pageSize":10}'
```

```bash
dotnet test HotelSearch.sln -c Release
```

---

## Evaluation criteria — where to look

| Criterion | Document / location |
|-----------|---------------------|
| Functionality | Run commands above; [testing.md](testing.md) |
| Technical design | [architecture.md](architecture.md) |
| Technology | README — stack table |
| Standards (HTTP/REST) | [api-examples.md](api-examples.md), [openapi/](openapi/) |
| Coding style | `.editorconfig`, `Directory.Build.props` |
| Source organization | README — project structure |
| Performance | README — Performance; [checklist.md](checklist.md) |
| Security | README — Security; optional `X-Api-Key` on writes |
| Test coverage | [testing.md](testing.md) |
| Documentation | README + `docs/` |
| Processes | `.github/workflows/ci.yml` |
