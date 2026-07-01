# Postman

Import the collection and environment into [Postman](https://www.postman.com/) or [Postman for VS Code](https://marketplace.visualstudio.com/items?itemName=Postman.postman-for-vscode) to exercise the API without writing curl commands.

| File | Purpose |
|------|---------|
| [HotelSearch.postman_collection.json](HotelSearch.postman_collection.json) | Requests grouped by **Health**, **Hotel CRUD**, and **Search** |
| [HotelSearch.local.postman_environment.json](HotelSearch.local.postman_environment.json) | Local `baseUrl` and `apiKey` variables |

## Import

### Postman desktop / web

1. Open Postman → **Import** (top-left).
2. Drag both JSON files into the import dialog, or choose **Upload Files**.
3. Confirm **Hotel Search API** collection and **Hotel Search — Local** environment are selected → **Import**.

### Select the environment

1. Top-right environment dropdown → **Hotel Search — Local**.
2. Click the eye icon to edit variables if needed.

### Postman for VS Code

1. Install the **Postman** extension.
2. Sign in (or use lightweight mode).
3. **Import** → select both JSON files from this folder.

## Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `baseUrl` | `http://localhost:5103` | API root — use `http://localhost:8080` when running via Docker Compose |
| `apiKey` | *(empty)* | Sent as `X-Api-Key` on POST/PUT/DELETE when `ApiKey__WriteKey` is configured on the server |

The collection also sets `hotelId` automatically after **Create hotel** succeeds (workflow helper for Get/Update/Delete by id).

## Suggested run order

1. **Health → Health check** — confirm API and database are up
2. **Hotel CRUD → Create hotel** — stores `hotelId` from response
3. **Hotel CRUD → Get hotels**
4. **Hotel CRUD → Get hotel by id**
5. **Search → Search hotels** — create hotels first for non-empty results
6. **Hotel CRUD → Update hotel**
7. **Hotel CRUD → Delete hotel**

## Docker Compose

If the API runs in Docker on port **8080**, update the environment:

```
baseUrl = http://localhost:8080
```

Or duplicate the environment in Postman as **Hotel Search — Docker**.

## Prerequisites

Start the API before sending requests:

```bash
docker compose up --build
# or
docker compose up postgres -d
dotnet run --project src/HotelSearch.Api/HotelSearch.Api.csproj
```
