# API Examples

Base URL: `http://localhost:8080` (Docker Compose) or `http://localhost:5103` (local `dotnet run`).

All error responses use **RFC 7807 ProblemDetails** with a `traceId` extension.

---

## Health

### `GET /health`

```bash
curl http://localhost:8080/health
```

**200 OK**

```json
{
  "status": "healthy",
  "database": "healthy"
}
```

**503 Service Unavailable** (PostgreSQL unreachable)

```json
{
  "status": "unhealthy",
  "database": "unhealthy"
}
```

---

## Hotels (CRUD)

When `ApiKey__WriteKey` is configured, include `X-Api-Key: <key>` on POST, PUT, and DELETE.

### `POST /api/hotels` — Create

```bash
curl -X POST http://localhost:8080/api/hotels \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Grand Hotel Zagreb",
    "price": 120,
    "latitude": 45.8150,
    "longitude": 15.9819
  }'
```

**201 Created**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Grand Hotel Zagreb",
  "price": 120,
  "latitude": 45.8150,
  "longitude": 15.9819
}
```

Response header: `Location: /api/hotels/{id}`

### `GET /api/hotels` — List

```bash
curl http://localhost:8080/api/hotels
```

**200 OK** — JSON array of hotel objects.

### `GET /api/hotels/{id}` — Get by ID

```bash
curl http://localhost:8080/api/hotels/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**200 OK** — hotel object.

**404 Not Found** — hotel does not exist.

### `PUT /api/hotels/{id}` — Update

```bash
curl -X PUT http://localhost:8080/api/hotels/3fa85f64-5717-4562-b3fc-2c963f66afa6 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Grand Hotel Zagreb",
    "price": 135,
    "latitude": 45.8150,
    "longitude": 15.9819
  }'
```

**204 No Content** — success.

**404 Not Found** — hotel does not exist.

### `DELETE /api/hotels/{id}` — Delete

```bash
curl -X DELETE http://localhost:8080/api/hotels/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**204 No Content** — success.

**404 Not Found** — hotel does not exist.

---

## Search

### `POST /api/hotels/search`

```bash
curl -X POST http://localhost:8080/api/hotels/search \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "near 45.8150, 15.9819 under 200",
    "page": 1,
    "pageSize": 10
  }'
```

**200 OK**

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Grand Hotel Zagreb",
      "price": 120,
      "distanceKm": 0.42,
      "rankingScore": 0.15
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1
}
```

Empty catalogue: **200 OK** with `"items": []`, `"totalCount": 0` (not an error).

### Search prompt formats

| Format | Example |
|--------|---------|
| Near + coordinates | `near 45.8150, 15.9819` |
| Near + budget | `near 45.8150, 15.9819 under 200` |
| Location + max price | `location 45.8150, 15.9819 max price 150` |
| From + budget | `from 45.8150, 15.9819 budget 300` |
| Hotels near | `hotels near 45.8150, 15.9819` |

**Rules**

- Location (latitude, longitude) is **required**
- Budget is **optional** (`under`, `max price`, `budget`)
- Latitude ∈ [-90, 90], longitude ∈ [-180, 180], budget ≥ 0

**Additional curl examples**

```bash
# Location only
curl -X POST http://localhost:8080/api/hotels/search \
  -H "Content-Type: application/json" \
  -d '{"prompt":"near 45.8150, 15.9819","page":1,"pageSize":10}'

# With budget keyword variant
curl -X POST http://localhost:8080/api/hotels/search \
  -H "Content-Type: application/json" \
  -d '{"prompt":"location 45.8150, 15.9819 max price 150","page":1,"pageSize":10}'
```

---

## Error responses

### Validation — `400 Bad Request`

FluentValidation failures:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/hotels",
  "traceId": "00-...",
  "errors": {
    "name": ["Hotel name cannot be empty."],
    "price": ["Price must be greater than zero."]
  }
}
```

Invalid search prompt or domain rule:

```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "Could not extract a location from the search prompt.",
  "traceId": "00-..."
}
```

### Not found — `404 Not Found`

```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "Hotel '3fa85f64-5717-4562-b3fc-2c963f66afa6' was not found.",
  "instance": "/api/hotels/3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "traceId": "00-..."
}
```

### Unauthorized — `401 Unauthorized`

Returned on write operations when API key auth is enabled and the key is missing or invalid:

```json
{
  "title": "Unauthorized",
  "status": 401,
  "detail": "A valid API key is required for this operation.",
  "traceId": "00-..."
}
```

### Server error — `500 Internal Server Error`

Production responses omit exception details:

```json
{
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred.",
  "traceId": "00-..."
}
```

---

## Swagger UI

Available in **Development** only:

- Docker: http://localhost:8080/swagger
- Local: http://localhost:5103/swagger

OpenAPI JSON: `/openapi/v1.json`
