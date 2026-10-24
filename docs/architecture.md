# Horizon Survey API Architecture

## Runtime shape

The .NET 8 solution is split into four projects. The API owns HTTP controllers and composition; Application owns use cases and validation; Domain owns persistence-independent entities and repository contracts; Infrastructure implements those contracts with EF Core and SQL Server.

```text
HorizonSurvey.Api -> HorizonSurvey.Application -> HorizonSurvey.Domain
HorizonSurvey.Api -> HorizonSurvey.Infrastructure -> HorizonSurvey.Domain
```

Controllers call `InventoryService`; that service validates commands and delegates persistence through the Domain `IInventoryRepository` contract. Infrastructure implements the contract and applies state changes in serializable SQL Server transactions. Database constraints and a unique filtered index supplement the transaction checks to prevent duplicate active checkouts and invalid pooled counts.

## Data and workflow

- Equipment categories are persisted `EquipmentCategory` records referenced by individually tracked `EquipmentAsset` records. The category migration converts distinct existing values and creates no default categories in a new database.
- No pooled stock is seeded by the application; staff-managed inventory is returned from database records. Databases created with an earlier version may retain the original Tripods and field tablets rows because no cleanup migration is included.
- Each checkout has one holder, one asset or stock target, a required expected-return timestamp, and a quantity. Asset checkouts have quantity one; stock checkouts may be partial.
- Open checkouts become overdue only after their expected-return instant. Inventory exposes this state for individual assets and overdue outstanding quantities for pooled stock.
- Drone approval is recorded separately before checkout and is tied to one drone and holder. A successful checkout consumes the approval.
- Partial returns are supported. Return and damage movements are recorded atomically with count changes; damage reports identify how many returned pooled units were affected. Damage stays visible but does not impose a blocking policy.
- History requires both inclusive date-time bounds; no open-ended query is accepted.
- Deleting a category is blocked when any assigned asset has an open checkout; otherwise assets remain with a null category reference. Checkout, movement, and inventory records are not cascaded.
- Individual movement deletion and inclusive date-range deletion remove movement rows only; they do not change checkout or inventory state.

## Persistence safety

SQL Server is configured through `ConnectionStrings:HorizonSurvey`, with the environment override `ConnectionStrings__HorizonSurvey`. The default is configured in `appsettings.json`; no credentials are committed. Its `TrustServerCertificate` setting is for local development only; remote SQL Server connections should use a trusted certificate. EF Core migrations live in Infrastructure, selected as the migrations assembly by the context configuration. Visual Studio's Package Manager Console uses Infrastructure as its default project and API as its startup project; `Add-Migration InitialCreate` generates the initial migration, and the developer explicitly runs `Update-Database` to apply it. API startup does not create or migrate the schema. Existing databases are changed only through reviewed, explicitly applied migrations; connectivity or schema conflicts remain visible errors.

Date-time values are stored normalized to UTC ticks to support ordered range queries consistently across SQL Server and the SQLite test provider.

## API surface

- `GET /api/inventory`
- `GET /api/checkouts` (read-only open checkout list, including outstanding quantity and overdue status)
- `POST /api/equipment`
- `GET /api/categories`, `POST /api/categories`, `PUT /api/categories/{id}`, `DELETE /api/categories/{id}`
- `POST /api/approvals`
- `POST /api/checkouts`
- `POST /api/checkouts/{id}/returns`
- `GET /api/history?from=...&to=...`
- `DELETE /api/history/{id}` and `DELETE /api/history?from=...&to=...`

See the root README for request fields, startup instructions, and the approval limitation.

## Browser application

`frontend/` is an independent React 18 + TypeScript/Vite SPA. React Router owns screen navigation; typed Redux Toolkit slices and Redux Saga workers manage inventory, checkouts/returns, drone approvals, equipment registration, and history. Axios calls are isolated in `frontend/src/api/client.ts`. During development Vite proxies `/api` to the API server; deployments can set `VITE_API_BASE_URL`. ASP.NET Core CORS allows only configured `Cors:AllowedOrigins` values, defaulting to the local Vite origin. Successful mutations refresh inventory and open checkouts; the UI surfaces API problem details and does not substitute mock data.

## Exclusions and unresolved production concerns

There is no authentication/authorization, external notification, maintenance workflow, or generalized approval policy. A submitted `"Hamza"` approver value is not an identity assertion. Production identity, access, privacy, retention, deployment, backup/recovery, observability, and support decisions remain outside this implementation; it must not be considered production-ready.
