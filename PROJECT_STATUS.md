# Enhanzer Assignment - Project Status

**Status:** Implemented and build-verified  
**Last updated:** 2026-10-31

## Overview

This project contains a two-page purchase-bill application:

- Angular 21 standalone frontend
- ASP.NET Core 8 Web API backend
- SQL Server database accessed through Entity Framework Core
- JWT authentication and protected location data

## Repository Structure

```text
EnhanzerAssignment/
├── Database/
│   └── Database_Setup.sql
├── Enhanzer.API/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   └── LocationController.cs
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Models/
│   │   ├── ExternalApiPayload.cs
│   │   ├── LocationDetail.cs
│   │   └── LoginRequest.cs
│   ├── Program.cs
│   └── appsettings.json
├── enhanzer-frontend/
│   └── src/app/
│       ├── core/
│       │   ├── guards/auth.guard.ts
│       │   ├── interceptors/jwt.interceptor.ts
│       │   └── services/
│       │       ├── auth.service.ts
│       │       └── location.service.ts
│       └── features/
│           ├── login/
│           └── purchase-bill/
├── README.md
└── PROJECT_STATUS.md
```

## Backend Status

### Implemented

- `POST /api/auth/login`
  - Validates the submitted email and password.
  - Sends the required payload to the external POS login API.
  - Imports new user locations into SQL Server.
  - Returns a signed JWT.
- `GET /api/locations`
  - Requires a valid bearer token.
  - Returns stored locations ordered by name.
- JWT bearer authentication.
- CORS for `http://localhost:4200`.
- Swagger/OpenAPI in development.
- Root URL redirects to `/swagger`.

### Backend Configuration

- HTTPS profile: `https://localhost:7051`
- HTTP profile: `http://localhost:5106`
- SQL Server database: `EnhanzerDB`
- Entity Framework table: `dbo.Location_Details`
- Connection string and JWT settings are in `Enhanzer.API/appsettings.json`.

The development JWT secret must be replaced with a secure secret for any
non-development deployment.

## Database Status

[Database/Database_Setup.sql](./Database/Database_Setup.sql) creates:

- Database: `EnhanzerDB`
- Table: `Location_Details`
- Columns:
  - `Id INT IDENTITY PRIMARY KEY`
  - `Location_Code VARCHAR(100) NOT NULL`
  - `Location_Name VARCHAR(255) NOT NULL`

The script is idempotent and does not recreate an existing database or table.

## Frontend Status

### Login Page

- Reactive email and password validation.
- Loading spinner during authentication.
- Error message handling.
- JWT stored in `localStorage`.
- Redirects to the purchase-bill page after successful login.

### Purchase-Bill Page

- Protected by an authentication guard.
- Loads batches/locations through the authenticated locations API.
- Item autocomplete list:
  - Mango
  - Apple
  - Banana
  - Orange
  - Grapes
  - Kiwi
  - Strawberry
- Reactive calculations:
  - `Total Cost = (Standard Cost × Quantity) - (Standard Cost × Quantity × Margin / 100)`
  - `Total Selling = Standard Price × Quantity`
- Adds validated items to the grid.
- Displays total item count and total quantity.
- Includes sign-out functionality.

## Run Instructions

From `C:\11AD\Enhanzer-Demo`:

```powershell
cd EnhanzerAssignment
```

Run the database script in SQL Server, then start the backend:

```powershell
cd Enhanzer.API
dotnet restore
dotnet dev-certs https --trust
dotnet run --launch-profile https
```

Open `https://localhost:7051/swagger`.

In a second terminal, start the frontend:

```powershell
cd C:\11AD\Enhanzer-Demo\EnhanzerAssignment\enhanzer-frontend
npm install
ng serve
```

Open `http://localhost:4200`.

## Validation

The following checks passed during implementation:

```powershell
dotnet build --no-restore
npm run build
```

## Development Notes

- The external login endpoint requires network access:
  `https://ez-staging-api.azurewebsites.net/api/External_Api/POS_Api/Invoke`.
- SQL Server must be running and reachable using the configured connection string.
- The HTTPS developer certificate must be trusted locally.
- Generated `bin/`, `obj/`, and Angular build output should not be treated as source changes.
