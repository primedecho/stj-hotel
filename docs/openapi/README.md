# OpenAPI specification

Machine-readable API contract generated from the **HotelSearch.Api** project using [Microsoft.Extensions.ApiDescription.Server](https://www.nuget.org/packages/Microsoft.Extensions.ApiDescription.Server).

| File | Format |
|------|--------|
| [openapi.json](openapi.json) | OpenAPI 3.1 (JSON) |
| [openapi.yaml](openapi.yaml) | OpenAPI 3.1 (YAML) |

## Regenerate

From the repository root:

```powershell
./scripts/export-openapi.ps1
```

Or manually:

```bash
ASPNETCORE_ENVIRONMENT=Testing \
ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=hotels;Username=postgres;Password=postgres" \
dotnet build src/HotelSearch.Api/HotelSearch.Api.csproj -p:OpenApiGenerateDocumentsOnBuild=true

python scripts/openapi-json-to-yaml.py   # requires: pip install pyyaml
```

Generation bootstraps the app assembly; PostgreSQL does **not** need to be running (Testing environment, no migrations).

## Runtime vs committed spec

| Source | When available | Notes |
|--------|----------------|-------|
| **Committed files** (`docs/openapi/`) | Always | For reviewers, client generators, CI |
| **Swagger UI** (`/swagger`) | Development only | Interactive docs via Swashbuckle |
| **Live OpenAPI** (`/openapi/v1.json`) | Development only | Same document model as build output |

The committed spec includes request body examples and optional `X-Api-Key` security on write operations.

## Health endpoint

`/health` is excluded from the OpenAPI document (operational probe, not part of the business API surface).
