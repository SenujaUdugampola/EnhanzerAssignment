# Enhanzer Assignment

Two-page purchase-bill application built with Angular 21, ASP.NET Core 8,
Entity Framework Core, and SQL Server LocalDB.

## Prerequisites

Install the following:

- .NET 8 SDK
- Node.js and npm
- Angular CLI
- SQL Server LocalDB
- SQL Server command-line tools (`sqlcmd`) or SQL Server Management Studio

The default development database connection is:

```text
(localdb)\MSSQLLocalDB
```

## 1. Open the project

From PowerShell:

```powershell
cd C:\11AD\Enhanzer-Demo\EnhanzerAssignment
```

## 2. Create the database

Start the LocalDB instance:

```powershell
sqllocaldb start MSSQLLocalDB
```

Run [Database/Database_Setup.sql](./Database/Database_Setup.sql) using
SQL Server Management Studio, Azure Data Studio, or `sqlcmd`.

With `sqlcmd`, run:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i ".\Database\Database_Setup.sql"
```

The script creates:

- Database: `EnhanzerDB`
- Table: `dbo.Location_Details`
- Columns: `Id`, `Location_Code`, and `Location_Name`

## 3. Start the backend

Open a terminal in the project root and run:

```powershell
cd .\Enhanzer.API
dotnet restore
dotnet dev-certs https --trust
dotnet run --launch-profile https
```

The backend listens on:

- HTTPS: `https://localhost:7051`
- HTTP: `http://localhost:5106`

Open Swagger at:

```text
https://localhost:7051/swagger
```

If the browser displays a certificate warning, run
`dotnet dev-certs https --trust` and restart the backend.

## 4. Start the frontend

Open a second terminal and run:

```powershell
cd C:\11AD\Enhanzer-Demo\EnhanzerAssignment\enhanzer-frontend
npm install
ng serve
```

Open the application at:

```text
http://localhost:4200
```

The frontend is configured to call `https://localhost:7051/api`. If the
backend port changes, update
[environment.ts](./enhanzer-frontend/src/environments/environment.ts).

## Authentication flow

1. Enter valid credentials from the external POS login service on the login
   page.
2. The backend sends the credentials to:

   ```text
   https://ez-staging-api.azurewebsites.net/api/External_Api/POS_Api/Invoke
   ```

3. On success, the backend saves any new locations and returns a JWT.
4. The frontend stores the JWT and attaches it to API requests.
5. The purchase-bill page loads locations from the protected endpoint.

The login endpoint is:

```text
POST https://localhost:7051/api/auth/login
```

Example request body:

```json
{
  "email": "your-email",
  "password": "your-password"
}
```

## API endpoints

| Method | Endpoint | Authentication | Purpose |
|---|---|---|---|
| POST | `/api/auth/login` | No | Authenticate and return a JWT |
| GET | `/api/locations` | Bearer JWT | Return saved locations |

`GET /api/locations` returns `401 Unauthorized` when called without a valid
`Authorization: Bearer <token>` header. In Swagger, call the login endpoint,
copy the returned token, select **Authorize**, enter the token, and then call
the locations endpoint.

## Frontend features

- Reactive login form with email and password validation
- JWT storage and HTTP authorization interceptor
- Protected purchase-bill route
- Item autocomplete for Mango, Apple, Banana, Orange, Grapes, Kiwi, and
  Strawberry
- Batch dropdown populated from `Location_Details`
- Reactive total-cost and total-selling calculations
- Purchase-bill item grid and summary metrics

## Purchase-bill calculations

```text
Total Cost = (Standard Cost × Quantity)
             - ((Standard Cost × Quantity) × Discount / 100)

Total Selling = Standard Price × Quantity
```

## Build and validation

Backend:

```powershell
cd .\Enhanzer.API
dotnet build
```

Frontend:

```powershell
cd .\enhanzer-frontend
npm run build
```

## Troubleshooting

### SQL Server connection error

If the API reports that SQL Server cannot be found:

```powershell
sqllocaldb start MSSQLLocalDB
```

Then confirm that [appsettings.json](./Enhanzer.API/appsettings.json) uses:

```text
Server=(localdb)\MSSQLLocalDB
```

### `401 Unauthorized` from `/api/locations`

Authenticate through `/api/auth/login` first. The locations request must
include the JWT as a bearer token.

### HTTPS certificate warning

Trust the local development certificate:

```powershell
dotnet dev-certs https --trust
```

### External login service unavailable

The login flow depends on network access to the external POS API. Confirm
internet access and use valid external-service credentials.
