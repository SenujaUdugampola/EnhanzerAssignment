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
dotnet run
```

The API uses `https://localhost:7051` by default. Update
`enhanzer-frontend/src/environments/environment.ts` if the API uses a different port.

## Run the frontend

```powershell
cd enhanzer-frontend
npm install
ng serve
```

Open `http://localhost:4200`.
