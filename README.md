# Enhanzer Assignment

From `C:\11AD\Enhanzer-Demo`, enter the assignment folder first:

```powershell
cd EnhanzerAssignment
```

## Prerequisites

- .NET 8 SDK
- Node.js and Angular CLI
- SQL Server
- SQL Server LocalDB (the default development connection uses `MSSQLLocalDB`)

Run [Database/Database_Setup.sql](./Database/Database_Setup.sql) against SQL Server
before starting the API. For the default LocalDB setup, start the instance first:

```powershell
sqllocaldb start MSSQLLocalDB
```

## Run the backend

```powershell
cd Enhanzer.API
dotnet restore
dotnet dev-certs https --trust
dotnet run --launch-profile https
```

The HTTPS API is available at `https://localhost:7051`. If you run plain
`dotnet run`, .NET selects the HTTP profile and uses `http://localhost:5106`
instead.

Open `https://localhost:7051/` or `https://localhost:7051/swagger` to view the
API. If Windows shows a certificate prompt, accept it after running the
`dotnet dev-certs` command.

## Test the protected locations endpoint

`GET /api/locations` requires the JWT returned by the login endpoint. First
call `POST /api/auth/login` with valid external-service credentials, copy the
returned `token`, and include it as a bearer token:

```powershell
$login = Invoke-RestMethod `
  -Uri "https://localhost:7051/api/auth/login" `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"email":"your-email","password":"your-password"}'

Invoke-RestMethod `
  -Uri "https://localhost:7051/api/locations" `
  -Headers @{ Authorization = "Bearer $($login.token)" }
```

In Swagger, call the login endpoint first, click **Authorize**, enter the
returned token, and then call **GET /api/locations**. A request without an
`Authorization: Bearer <token>` header correctly returns `401 Unauthorized`.

## Run the frontend

```powershell
cd enhanzer-frontend
npm install
ng serve
```

Open `http://localhost:4200`.
