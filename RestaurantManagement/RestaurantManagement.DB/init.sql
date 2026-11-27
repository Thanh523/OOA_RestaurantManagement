IF DB_ID(N'RestaurantManagement') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE RestaurantManagement');
END;
GO

USE RestaurantManagement;
GO

IF OBJECT_ID(N'dbo.Managers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Managers
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Managers PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL,
        FullName NVARCHAR(100) NOT NULL,
        Role NVARCHAR(30) NOT NULL,
        PasswordHash VARBINARY(32) NOT NULL,
        PasswordSalt VARBINARY(16) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Managers_IsActive DEFAULT (1),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Managers_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_Managers_Username UNIQUE (Username)
    );
END;
GO

IF OBJECT_ID(N'dbo.BanAn', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BanAn
    (
        MaBan NVARCHAR(20) NOT NULL CONSTRAINT PK_BanAn PRIMARY KEY,
        KhuVuc NVARCHAR(50) NOT NULL,
        SucChua INT NOT NULL,
        TrangThaiBan NVARCHAR(30) NOT NULL,
        CONSTRAINT CK_BanAn_SucChua CHECK (SucChua > 0)
    );
END;
GO

DECLARE @StatusDefaultConstraint sysname;
SELECT @StatusDefaultConstraint = dc.name
FROM sys.default_constraints AS dc
JOIN sys.columns AS c
    ON c.object_id = dc.parent_object_id
   AND c.column_id = dc.parent_column_id
WHERE dc.parent_object_id = OBJECT_ID(N'dbo.BanAn')
  AND c.name = N'TrangThaiBan';

IF @StatusDefaultConstraint IS NOT NULL
BEGIN
    DECLARE @DropStatusDefaultSql NVARCHAR(MAX) =
        N'ALTER TABLE dbo.BanAn DROP CONSTRAINT ' + QUOTENAME(@StatusDefaultConstraint);
    EXEC sp_executesql @DropStatusDefaultSql;
END;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.BanAn')
      AND name = N'CK_BanAn_TrangThaiBan'
)
BEGIN
    ALTER TABLE dbo.BanAn DROP CONSTRAINT CK_BanAn_TrangThaiBan;
END;
GO
