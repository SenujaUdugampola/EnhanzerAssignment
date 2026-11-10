USE db77278;
GO

IF OBJECT_ID(N'dbo.Purchase_Bill_Number', N'SO') IS NULL
BEGIN
    EXEC(N'CREATE SEQUENCE dbo.Purchase_Bill_Number
        AS INT
        START WITH 1
        INCREMENT BY 1;');
END
GO

IF OBJECT_ID(N'dbo.Purchase_Bills', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Purchase_Bills
    (
        Id BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Purchase_Bills PRIMARY KEY,
        Bill_No VARCHAR(30) NOT NULL
            CONSTRAINT UQ_Purchase_Bills_Bill_No UNIQUE,
        Purchase_Date DATETIME2(0) NOT NULL
            CONSTRAINT DF_Purchase_Bills_Purchase_Date DEFAULT SYSUTCDATETIME(),
        Created_By VARCHAR(255) NULL,
        Created_At DATETIME2(0) NOT NULL
            CONSTRAINT DF_Purchase_Bills_Created_At DEFAULT SYSUTCDATETIME()
    );
END
GO

IF OBJECT_ID(N'dbo.Purchase_Bill_Items', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Purchase_Bill_Items
    (
        Id BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Purchase_Bill_Items PRIMARY KEY,
        Purchase_Bill_Id BIGINT NOT NULL,
        Item_Name VARCHAR(100) NOT NULL,
        Batch_Code VARCHAR(100) NOT NULL,
        Standard_Cost DECIMAL(18,2) NOT NULL,
        Standard_Price DECIMAL(18,2) NOT NULL,
        Margin_Percent DECIMAL(5,2) NOT NULL,
        Quantity DECIMAL(18,3) NOT NULL,
        Free_Quantity DECIMAL(18,3) NOT NULL,
        Discount_Percent DECIMAL(5,2) NOT NULL,
        Total_Cost AS
        (
            (Standard_Cost * Quantity) *
            (1 - (Discount_Percent / 100))
        ) PERSISTED,
        Total_Selling AS
        (
            Standard_Price * Quantity
        ) PERSISTED,
        CONSTRAINT FK_Purchase_Bill_Items_Purchase_Bills
            FOREIGN KEY (Purchase_Bill_Id)
            REFERENCES dbo.Purchase_Bills(Id),
        CONSTRAINT CK_Purchase_Bill_Items_Standard_Cost
            CHECK (Standard_Cost >= 0),
        CONSTRAINT CK_Purchase_Bill_Items_Standard_Price
            CHECK (Standard_Price >= 0),
        CONSTRAINT CK_Purchase_Bill_Items_Margin
            CHECK (Margin_Percent >= 0),
        CONSTRAINT CK_Purchase_Bill_Items_Quantity
            CHECK (Quantity > 0),
        CONSTRAINT CK_Purchase_Bill_Items_Free_Quantity
            CHECK (Free_Quantity >= 0),
        CONSTRAINT CK_Purchase_Bill_Items_Discount
            CHECK (Discount_Percent BETWEEN 0 AND 100)
    );
END
GO

IF TYPE_ID(N'dbo.Purchase_Item_Type') IS NULL
BEGIN
    EXEC(N'CREATE TYPE dbo.Purchase_Item_Type AS TABLE
    (
        Item_Name VARCHAR(100) NOT NULL,
        Batch_Code VARCHAR(100) NOT NULL,
        Standard_Cost DECIMAL(18,2) NOT NULL,
        Standard_Price DECIMAL(18,2) NOT NULL,
        Margin_Percent DECIMAL(5,2) NOT NULL,
        Quantity DECIMAL(18,3) NOT NULL,
        Free_Quantity DECIMAL(18,3) NOT NULL,
        Discount_Percent DECIMAL(5,2) NOT NULL
    );');
END
GO

CREATE OR ALTER PROCEDURE dbo.Create_Purchase_Bill
    @Created_By VARCHAR(255),
    @Items dbo.Purchase_Item_Type READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM @Items)
        THROW 50001, 'At least one purchase item is required.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @Items
        WHERE Quantity <= 0
           OR Free_Quantity < 0
           OR Standard_Cost < 0
           OR Standard_Price < 0
           OR Margin_Percent < 0
           OR Discount_Percent NOT BETWEEN 0 AND 100
    )
        THROW 50002, 'One or more purchase items contain invalid values.', 1;

    DECLARE @BillId BIGINT;
    DECLARE @BillNo VARCHAR(30);

    SET @BillNo =
        'PB-' + CONVERT(VARCHAR(8), GETDATE(), 112) + '-' +
        RIGHT('000000' + CONVERT(VARCHAR(6), NEXT VALUE FOR dbo.Purchase_Bill_Number), 6);

    BEGIN TRANSACTION;

    INSERT INTO dbo.Purchase_Bills
    (
        Bill_No,
        Created_By
    )
    VALUES
    (
        @BillNo,
        @Created_By
    );

    SET @BillId = CONVERT(BIGINT, SCOPE_IDENTITY());

    INSERT INTO dbo.Purchase_Bill_Items
    (
        Purchase_Bill_Id,
        Item_Name,
        Batch_Code,
        Standard_Cost,
        Standard_Price,
        Margin_Percent,
        Quantity,
        Free_Quantity,
        Discount_Percent
    )
    SELECT
        @BillId,
        Item_Name,
        Batch_Code,
        Standard_Cost,
        Standard_Price,
        Margin_Percent,
        Quantity,
        Free_Quantity,
        Discount_Percent
    FROM @Items;

    COMMIT TRANSACTION;

    SELECT
        b.Id,
        b.Bill_No,
        b.Purchase_Date,
        b.Created_By,
        SUM(i.Quantity) AS Total_Quantity,
        SUM(i.Free_Quantity) AS Total_Free_Quantity,
        SUM(i.Total_Cost) AS Total_Cost,
        SUM(i.Total_Selling) AS Total_Selling
    FROM dbo.Purchase_Bills AS b
    INNER JOIN dbo.Purchase_Bill_Items AS i
        ON i.Purchase_Bill_Id = b.Id
    WHERE b.Id = @BillId
    GROUP BY
        b.Id,
        b.Bill_No,
        b.Purchase_Date,
        b.Created_By;
END
GO
