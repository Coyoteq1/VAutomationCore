# HTTP Server API

`HttpServer` provides REST endpoints for V Rising Admin GUI integration.

## Base URL

```
http://localhost:8080/
```

## Authentication

All endpoints support optional API key authentication via the `X-API-Key` header.

## Endpoints

### Status

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/status` | Get server status |
| GET | `/api/v1/stats` | Get quick statistics |

### Zones

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/zones` | Get all zones |
| GET | `/api/v1/zones/paginated` | Get zones with pagination |
| POST | `/api/v1/zones/glow/spawn` | Spawn glows in zone |
| POST | `/api/v1/zones/glow/clear` | Clear glows from zone |
| PUT | `/api/v1/zones/borders` | Toggle zone borders |
| PUT | `/api/v1/zones/config` | Update zone configuration |

### Traps

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/traps` | Get all traps |
| GET | `/api/v1/traps/paginated` | Get traps with pagination |
| POST | `/api/v1/traps/set` | Set a trap |
| POST | `/api/v1/traps/remove` | Remove a trap |
| POST | `/api/v1/traps/arm` | Arm a trap |
| POST | `/api/v1/traps/trigger` | Trigger a trap |
| POST | `/api/v1/traps/clear` | Clear all traps |
| GET | `/api/v1/traps/zones` | Get trap zones |
| POST | `/api/v1/traps/zones/create` | Create trap zone |
| POST | `/api/v1/traps/zones/delete` | Delete trap zone |
| POST | `/api/v1/traps/zones/arm` | Arm trap zone |

### Chests

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/chests` | Get all chests |
| GET | `/api/v1/chests/paginated` | Get chests with pagination |
| POST | `/api/v1/chests/spawn` | Spawn a chest |
| POST | `/api/v1/chests/remove` | Remove a chest |
| POST | `/api/v1/chests/clear` | Clear all chests |

### Streaks

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/streaks` | Get streak data |
| POST | `/api/v1/streaks/reset` | Reset streak |

### Configuration

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/config` | Get configuration |
| PUT | `/api/v1/config` | Update configuration |
| POST | `/api/v1/config/reload` | Reload configuration |

### Logs

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/logs` | Get event logs |

### Players

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/players` | Get all players |
| GET | `/api/v1/players/paginated` | Get players with pagination |
| GET | `/api/v1/players/update` | Get player update events |

### Legacy Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/status` | Redirects to v1 |
| GET | `/api/stats` | Redirects to v1 |
| GET | `/api/zones` | Redirects to v1 |
| GET | `/api/traps` | Redirects to v1 |
| GET | `/api/chests` | Redirects to v1 |
| GET | `/api/streaks` | Redirects to v1 |
| GET | `/api/config` | Redirects to v1 |
| GET | `/api/logs` | Redirects to v1 |
| GET | `/api/players` | Redirects to v1 |
| GET | `/api/players/update` | Redirects to v1 |

## Response Format

All responses follow this structure:

```json
{
  "success": true,
  "data": { ... },
  "error": null
}
```

### Error Response

```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "ERROR_CODE",
    "message": "Error message"
  }
}
```

### Pagination Response

Paginated endpoints return:

```json
{
  "items": [...],
  "offset": 0,
  "limit": 50,
  "total": 100
}
```

## CORS

All endpoints support CORS with the following headers:
- `Access-Control-Allow-Origin: *`
- `Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS`
- `Access-Control-Allow-Headers: Content-Type, X-API-Key`

## Example Usage

### Get Server Status

```bash
curl http://localhost:8080/api/v1/status
```

### Get All Players

```bash
curl http://localhost:8080/api/v1/players
```

### Get Players with Authentication

```bash
curl -H "X-API-Key: your-api-key" http://localhost:8080/api/v1/players
```

### Paginated Request

```bash
curl "http://localhost:8080/api/v1/players/paginated?offset=0&limit=10"
```

