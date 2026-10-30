# Enhanzer Assignment

From `C:\11AD\Enhanzer-Demo`, enter the assignment folder first:

```powershell
cd EnhanzerAssignment
```

## Prerequisites

- .NET 8 SDK
- Node.js and Angular CLI
- SQL Server

Run [Database/Database_Setup.sql](./Database/Database_Setup.sql) against SQL Server before starting the API.

## Run the backend

```powershell
cd Enhanzer.API
dotnet restore
dotnet run --launch-profile https
```

The HTTPS API is available at `https://localhost:7051`. If you run plain
`dotnet run`, .NET selects the HTTP profile and uses `http://localhost:5106`
instead.

## Run the frontend

```powershell
cd enhanzer-frontend
npm install
ng serve
```

Open `http://localhost:4200`.
