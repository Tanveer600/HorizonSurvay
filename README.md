# Horizon Survey layered API

The repository contains a .NET 8 equipment-management API organized into Domain, Application, Infrastructure, and API projects. It supports individually tracked rovers, total stations, and drones; pooled tripods and field tablets; checkout, return, drone approval, damage notes, and bounded movement history.

## Requirements

- .NET 8 SDK
- SQL Server or SQL Server LocalDB

## Build, test, and run

From the repository root:

```powershell
dotnet build src\HorizonSurvey.sln
dotnet test src\HorizonSurvey.sln
$env:ConnectionStrings__HorizonSurvey = "Server=(localdb)\MSSQLLocalDB;Database=HorizonSurvey;Trusted_Connection=True;TrustServerCertificate=True"
dotnet run --project src\HorizonSurvey.Api
```

The checked-in `appsettings.json` contains a local SQL Server connection string; it may need to be overridden for your machine. Set `ConnectionStrings__HorizonSurvey` to a SQL Server or LocalDB instance you can access. `TrustServerCertificate=True` is only for local development; remote connections should validate a trusted SQL Server certificate. No username or password is committed. In Development, Swagger UI is available at `/swagger`.

## Database migrations

Schema changes are applied only when you explicitly run an EF Core migration command; API startup does not create or migrate the database. In Visual Studio, restore the solution, set `HorizonSurvey.Api` as the startup project, then open **Tools > NuGet Package Manager > Package Manager Console** and select `HorizonSurvey.Infrastructure` as the console's **Default project**. Run:

```powershell
# This repository already contains InitialCreate; run this only before it exists.
Add-Migration InitialCreate
Update-Database
```

The migration and model snapshot are stored in `src\HorizonSurvey.Infrastructure\Migrations`. `Add-Migration` generates source files but does not connect to or change the database. `Update-Database` connects to the SQL Server configured by `ConnectionStrings:HorizonSurvey` (overridable with `ConnectionStrings__HorizonSurvey`) and creates or updates the schema and records the migration in `__EFMigrationsHistory`. Migrations do not create default categories or pooled stock.

For a new database, run `Update-Database` to apply all migrations. For an existing database, the `ManageEquipmentCategories` migration converts each distinct category value already stored on equipment into a persisted category and links existing assets without changing checkout or movement records. For later model changes, use `Add-Migration <MigrationName>` with a new name, then apply it with `Update-Database`.

Check the connection string and target database before running `Update-Database`. Database connectivity errors and schema conflicts are reported; startup never falls back to `EnsureCreated`. Back up existing data and review generated migrations before applying schema changes. Do not use `EnsureCreated` on a database managed by migrations.

## API

| Method and path | Purpose |
|---|---|
| `GET /api/inventory` | Current individual asset holders/availability and overdue state, pooled counts/overdue quantities, and latest damage note |
| `GET /api/categories` | List persisted equipment categories |
| `POST /api/categories` | Create a category (`name`) |
| `PUT /api/categories/{id}` | Rename a category (`name`) |
| `DELETE /api/categories/{id}` | Delete a category; reject while assigned equipment has an open checkout, otherwise keep assets/history and clear their category |
| `POST /api/equipment` | Register an individual asset (`name`, `categoryId`) |
| `POST /api/approvals` | Record a drone approval (`equipmentAssetId`, `holder`, `approvedBy`) |
| `POST /api/checkouts` | Check out exactly one asset or pooled stock (`holder`, `expectedReturnAt`, one target ID, `quantity`, optional `approvalId`) |
| `POST /api/checkouts/{id}/returns` | Return some or all outstanding quantity (`returner`, `quantity`; damage reports include `damageDescription` and `damageQuantity`) |
| `GET /api/checkouts` | Read open checkouts with checkout ID, item identity, holder, due time, outstanding quantity, and overdue state |
| `GET /api/history?from={ISO-8601}&to={ISO-8601}` | Movements in the inclusive, finite date-time range |
| `DELETE /api/history/{id}` | Delete one movement record without changing its checkout or inventory |
| `DELETE /api/history?from={ISO-8601}&to={ISO-8601}` | Delete movement records in the inclusive, finite date-time range only |

### Example: approve, check out, and return a drone

Use the equipment ID returned by `POST /api/equipment`. Record Hamza's approval first:

```json
{
  "equipmentAssetId": "DRONE_ASSET_ID",
  "holder": "Amina",
  "approvedBy": "Hamza"
}
```

Use the returned approval ID for checkout:

```json
{
  "holder": "Amina",
  "expectedReturnAt": "2026-10-22T17:00:00Z",
  "equipmentAssetId": "DRONE_ASSET_ID",
  "quantity": 1,
  "approvalId": "APPROVAL_ID"
}
```

For returns, send `returner` and `quantity`. When reporting damage, also provide `damageDescription` and `damageQuantity` (the number of returned units affected).

A drone approval must be recorded before checkout, must name Hamza as approver, and is tied to the specified drone and holder. It can be used once. Without authentication, this records the submitted approval name but does not verify the real-world identity of the approver.

Damage notes are retained in movement history and shown in inventory. They do not automatically block future checkouts because that policy is not specified. History requests require both bounds, with `from <= to`. Invalid input returns a client error; unavailable or conflicting inventory returns a conflict.

## Project boundaries

- `HorizonSurvey.Domain`: entities, domain rules, repository contracts, and inventory result contracts.
- `HorizonSurvey.Application`: use cases, validation, API-independent commands, and repository coordination.
- `HorizonSurvey.Infrastructure`: SQL Server EF Core persistence, serializable checkout/return transactions, and EF Core migrations applied explicitly by the developer.
- `HorizonSurvey.Api`: controllers, dependency injection, HTTP responses, and configuration.
- `tests/HorizonSurvey.Tests`: workflow and persistence tests against an in-memory SQLite database.

The API does not include authentication/roles, reminders, WhatsApp integration, a maintenance workflow, or unsupported equipment approval policies. The separate frontend is described below. This application is not production-ready; identity, access control, privacy, retention, deployment, backup/recovery, and operational requirements remain unresolved.

## React frontend

The `frontend` directory is a separate React 18 + TypeScript single-page application. It uses Vite, React Router, Redux Toolkit, Redux Saga, and a typed Axios API layer. No sample inventory or mock API fallback is included.

Start the API in one terminal:

```powershell
dotnet run --project src\HorizonSurvey.Api --launch-profile http
```

Then start the UI in another:

```powershell
cd frontend
npm ci
npm run dev
```

The Vite development server proxies `/api` requests to `http://localhost:5097`, matching the API's HTTP launch profile. Override the proxy target with `VITE_PROXY_TARGET` if the API listens elsewhere. For a deployed UI, set `VITE_API_BASE_URL` to the API base URL (for example, `https://api.example.com/api`) and configure the same UI origin in the API's `Cors:AllowedOrigins` list or environment variable `Cors__AllowedOrigins__0`. The default local origin is `http://localhost:5173`.

The UI provides an overview and inventory screen, checkout and partial-return forms, drone approval recording, persisted category management, asset registration using API-loaded categories, and bounded movement history with individual and confirmed date-range deletion. The approvals view records Hamza's submitted name but does not authenticate him; the API has no authentication. API or validation failures remain visible and do not produce success receipts.

Frontend verification commands:

```powershell
npm run lint
npm test
npm run build
```
