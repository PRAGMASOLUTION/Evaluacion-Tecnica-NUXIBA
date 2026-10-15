IF DB_ID(N'NuxibaEvaluation') IS NULL
    CREATE DATABASE NuxibaEvaluation;
GO
USE NuxibaEvaluation;
GO

-- Las tablas destino ccUsers, ccRIACat_Areas y ccloglogin las crea la migración EF Core.
-- Ejecuta primero: dotnet ef database update --project src/Nuxiba.Backend --startup-project src/Nuxiba.Backend.
-- Este script crea solo staging y el ledger para importar los datos del Excel sin perder filas repetidas.

-- Staging con ordinal de origen: conserva filas repetidas del Excel y permite importación idempotente.
IF OBJECT_ID('dbo.stg_ccUsers', 'U') IS NULL
CREATE TABLE dbo.stg_ccUsers (
    SourceRowNumber int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    User_id nvarchar(50) NULL, Login nvarchar(200) NULL, Nombres nvarchar(200) NULL,
    ApellidoPaterno nvarchar(200) NULL, ApellidoMaterno nvarchar(200) NULL,
    TipoUser_id nvarchar(50) NULL, Status nvarchar(50) NULL, fCreate nvarchar(50) NULL,
    IDArea nvarchar(50) NULL, LastLoginAttempt nvarchar(50) NULL
);
IF OBJECT_ID('dbo.stg_ccRIACat_Areas', 'U') IS NULL
CREATE TABLE dbo.stg_ccRIACat_Areas (
    SourceRowNumber int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    IDArea nvarchar(50) NULL, AreaName nvarchar(200) NULL, StatusArea nvarchar(50) NULL, CreateDate nvarchar(50) NULL
);
IF OBJECT_ID('dbo.stg_ccloglogin', 'U') IS NULL
CREATE TABLE dbo.stg_ccloglogin (
    SourceRowNumber int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    User_id nvarchar(50) NULL, Extension nvarchar(50) NULL, TipoMov nvarchar(50) NULL, fecha nvarchar(50) NULL
);
IF OBJECT_ID('dbo.ImportLedger', 'U') IS NULL
CREATE TABLE dbo.ImportLedger (
    SourceName sysname NOT NULL,
    SourceRowNumber int NOT NULL,
    ImportedAt datetime2 NOT NULL CONSTRAINT DF_ImportLedger_ImportedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ImportLedger PRIMARY KEY (SourceName, SourceRowNumber)
);
GO

/*
Cargar los CSV desde SSMS (Import Flat File) en las tablas stg_*:
- data/import/ccUsers.csv -> stg_ccUsers (no importar SourceRowNumber)
- data/import/ccRIACat_Areas.csv -> stg_ccRIACat_Areas
- data/import/ccloglogin.csv -> stg_ccloglogin
Los CSV tienen BOM UTF-8 y encabezado. Si se usa BULK INSERT, primero cópialos a una ruta
visible desde el proceso SQL Server y especifica FIRSTROW=2, CODEPAGE='65001', FIELDQUOTE='"'.
No truncar staging antes de revisar el reporte de validación.
*/
