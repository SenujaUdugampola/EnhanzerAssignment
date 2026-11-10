# Enhanzer Assignment

Three-page purchase-bill application built with Angular 21, ASP.NET Core 8,
Entity Framework Core, and SQL Server.

## Prerequisites

- .NET 8 SDK
- Node.js and npm
- Angular CLI
- SQL Server LocalDB
- SQL Server command-line tools (`sqlcmd`) or SQL Server Management Studio

The local development database connection is:

```text
(localdb)\MSSQLLocalDB
```

## 1. Open the project

```powershell
cd C:\11AD\Enhanzer-Demo\EnhanzerAssignment
```

## 2. Create the database

Start LocalDB:

```powershell
sqllocaldb start MSSQLLocalDB
```

Run the database setup scripts:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i ".\Database\Database_Setup.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i ".\Database\Purchase_Setup.sql"
```

`Database_Setup.sql` creates `EnhanzerDB` and `dbo.Location_Details`.
`Purchase_Setup.sql` creates `Purchase_Bills`, `Purchase_Bill_Items`, the
bill-number sequence, and the `Create_Purchase_Bill` stored procedure.

## 3. Start the backend

Open a terminal in the project root:

```powershell
cd .\Enhanzer.API
dotnet restore
dotnet dev-certs https --trust
dotnet run --launch-profile https
```

The backend listens on:

- HTTPS: `https://localhost:7051`
- HTTP: `http://localhost:5106`

Swagger is available at `https://localhost:7051/swagger`.

## 4. Start the frontend

Open a second terminal:

```powershell
cd C:\11AD\Enhanzer-Demo\EnhanzerAssignment\enhanzer-frontend
npm install
ng serve
```

Open the application at `http://localhost:4200`.

Production uses the relative `/api` path. During local `ng serve`, the
development environment calls `http://localhost:5106/api`. If the backend
port changes, update the environment files in
[enhanzer-frontend/src/environments](./enhanzer-frontend/src/environments).

Production builds use the relative `/api` path for deployment behind the
ASP.NET application.

## Navigating the pages

The application has three pages:

1. Open `http://localhost:4200/login` (or the root URL `/`).
2. Enter valid credentials from the external POS login service and select
   **Sign in**.
3. After login, the dashboard opens at `http://localhost:4200/dashboard`.
4. Select **New purchase bill** or **Purchase bills** to open
   `http://localhost:4200/purchase-bill`.
5. Select an item, choose a location in **Batch**, enter the cost, price,
   quantities, margin, and discount, then select **Add to grid**.
6. Add any additional items and select **Save purchase**.
7. Select **Dashboard** to return to the dashboard or **Sign out** to return
   to the login page.

The `/dashboard` and `/purchase-bill` routes are protected. Without a valid
JWT, they redirect to `/login`.

## Authentication flow

1. The login page sends credentials to the external POS login service.
2. The backend saves any new locations and returns a JWT.
3. The frontend stores the JWT and attaches it to API requests.
4. The purchase-bill page loads locations from the protected endpoint.

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
| POST | `/api/auth/login` | None | Authenticate and return a JWT |
| GET | `/api/locations` | Bearer JWT | Return saved locations |
| GET | `/api/purchases/dashboard` | Bearer JWT | Return dashboard widget data |
| POST | `/api/purchases` | Bearer JWT | Create a purchase bill with its items |
| GET | `/api/purchases/{id}` | Bearer JWT | Return a saved purchase bill |

Protected endpoints require an `Authorization: Bearer <token>` header. In
Swagger, call the login endpoint, copy the returned token, select
**Authorize**, enter `Bearer <token>`, and then call the protected endpoint.

## Frontend features

- Reactive login form with email and password validation
- JWT storage and HTTP authorization interceptor
- Protected dashboard route with purchase-bill navigation
- Dashboard widgets for latest orders, oldest order items, and item quantities
- Protected purchase-bill route
- Item autocomplete for Mango, Apple, Banana, Orange, Grapes, Kiwi, and
  Strawberry
- Batch dropdown populated from `Location_Details`
- Reactive total-cost and total-selling calculations
- Purchase-bill item grid and summary metrics
- Purchase-bill submission with success and error messages

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

### Deploy the latest frontend and API

Publish the API project using the
[`site103935-WebDeploy.pubxml`](./Enhanzer.API/Properties/PublishProfiles/site103935-WebDeploy.pubxml)
profile from Visual Studio or MSBuild. The publish target automatically runs
the Angular production bundle, and copies the generated
dashboard and purchase pages into the ASP.NET publish output.
The bundle is copied into `wwwroot`, which is the directory served by
ASP.NET Core and IIS.

After deployment, force-refresh the browser with **Ctrl + F5**. If the old
page still appears, clear the site cache or open the site in a private window
to rule out a cached JavaScript bundle.

## Troubleshooting

### SQL Server connection error

```powershell
sqllocaldb start MSSQLLocalDB
```

Confirm that [appsettings.json](./Enhanzer.API/appsettings.json) uses:

```text
Server=(localdb)\MSSQLLocalDB
```

### `401 Unauthorized`

Authenticate through `/api/auth/login` first. Protected requests must include
the JWT as a bearer token.

### HTTPS certificate warning

```powershell
dotnet dev-certs https --trust
```

### External login service unavailable

The login flow depends on network access to the external POS API. Confirm
internet access and use valid external-service credentials.

### Web Deploy returns `401 Unauthorized`

The Web Deploy profile requires valid credentials for the hosting account.
Update the publish profile credentials in Visual Studio or the hosting
provider's publish settings, then publish the API project again. Do not commit
the deployment password to the repository.
