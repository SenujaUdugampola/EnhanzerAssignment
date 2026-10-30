IF DB_ID(N'EnhanzerDB') IS NULL
BEGIN
    CREATE DATABASE EnhanzerDB;
END
GO

USE EnhanzerDB;
GO

IF OBJECT_ID(N'dbo.Location_Details', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Location_Details
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Location_Details PRIMARY KEY,
        Location_Code VARCHAR(100) NOT NULL,
        Location_Name VARCHAR(255) NOT NULL
    );
END
GO
