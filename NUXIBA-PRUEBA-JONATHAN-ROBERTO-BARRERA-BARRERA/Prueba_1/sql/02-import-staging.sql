USE NuxibaEvaluation;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- No se omiten duplicados de manera silenciosa: los casos conflictivos deben revisarse en 03-validate-import.sql.
-- User_id es clave real en ccUsers; una segunda fila con el mismo ID queda fuera de la importación final
-- y debe resolverse manualmente tras revisar sus valores.
;WITH U AS (
    SELECT *, ROW_NUMBER() OVER (PARTITION BY TRY_CONVERT(int, User_id) ORDER BY SourceRowNumber) AS rn
    FROM dbo.stg_ccUsers
    WHERE TRY_CONVERT(int, User_id) IS NOT NULL AND NULLIF(Login, '') IS NOT NULL
)
INSERT dbo.ccUsers (User_id, Login, Nombres, ApellidoPaterno, ApellidoMaterno, IDArea)
SELECT TRY_CONVERT(int, u.User_id), LEFT(u.Login,100), NULLIF(u.Nombres,''), NULLIF(u.ApellidoPaterno,''),
       NULLIF(u.ApellidoMaterno,''), TRY_CONVERT(int, u.IDArea)
FROM U u
WHERE u.rn = 1
  AND NOT EXISTS (SELECT 1 FROM dbo.ccUsers d WHERE d.User_id = TRY_CONVERT(int, u.User_id))
  AND NOT EXISTS (SELECT 1 FROM dbo.ImportLedger l WHERE l.SourceName='ccUsers' AND l.SourceRowNumber=u.SourceRowNumber);

INSERT dbo.ImportLedger(SourceName, SourceRowNumber)
SELECT 'ccUsers', u.SourceRowNumber
FROM dbo.stg_ccUsers u
WHERE TRY_CONVERT(int, u.User_id) IS NOT NULL
  AND EXISTS (SELECT 1 FROM dbo.ccUsers d WHERE d.User_id=TRY_CONVERT(int,u.User_id))
  AND NOT EXISTS (SELECT 1 FROM dbo.ImportLedger l WHERE l.SourceName='ccUsers' AND l.SourceRowNumber=u.SourceRowNumber);

-- Catálogo con IDArea repetido: se conserva por clave compuesta IDArea + AreaName.
INSERT dbo.ccRIACat_Areas(IDArea, AreaName, StatusArea, CreateDate)
SELECT TRY_CONVERT(int, a.IDArea), LEFT(a.AreaName,150), TRY_CONVERT(int,a.StatusArea), TRY_CONVERT(datetime2,a.CreateDate)
FROM dbo.stg_ccRIACat_Areas a
WHERE TRY_CONVERT(int,a.IDArea) IS NOT NULL AND NULLIF(a.AreaName,'') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.ccRIACat_Areas d
                  WHERE d.IDArea=TRY_CONVERT(int,a.IDArea) AND d.AreaName=LEFT(a.AreaName,150));

-- Cada fila de log se importa una sola vez según ordinal de origen; incluso eventos idénticos se preservan.
INSERT dbo.ccloglogin(User_id, Extension, TipoMov, fecha)
SELECT TRY_CONVERT(int,l.User_id), TRY_CONVERT(int,l.Extension), TRY_CONVERT(int,l.TipoMov), TRY_CONVERT(datetime2,l.fecha)
FROM dbo.stg_ccloglogin l
WHERE TRY_CONVERT(int,l.User_id) IS NOT NULL
  AND TRY_CONVERT(int,l.Extension) IS NOT NULL
  AND TRY_CONVERT(int,l.TipoMov) IN (0,1)
  AND TRY_CONVERT(datetime2,l.fecha) IS NOT NULL
  AND EXISTS (SELECT 1 FROM dbo.ccUsers u WHERE u.User_id=TRY_CONVERT(int,l.User_id))
  AND NOT EXISTS (SELECT 1 FROM dbo.ImportLedger il WHERE il.SourceName='ccloglogin' AND il.SourceRowNumber=l.SourceRowNumber);

INSERT dbo.ImportLedger(SourceName, SourceRowNumber)
SELECT 'ccloglogin', l.SourceRowNumber
FROM dbo.stg_ccloglogin l
WHERE TRY_CONVERT(int,l.User_id) IS NOT NULL
  AND TRY_CONVERT(int,l.Extension) IS NOT NULL
  AND TRY_CONVERT(int,l.TipoMov) IN (0,1)
  AND TRY_CONVERT(datetime2,l.fecha) IS NOT NULL
  AND EXISTS (SELECT 1 FROM dbo.ccUsers u WHERE u.User_id=TRY_CONVERT(int,l.User_id))
  AND NOT EXISTS (SELECT 1 FROM dbo.ImportLedger il WHERE il.SourceName='ccloglogin' AND il.SourceRowNumber=l.SourceRowNumber);

COMMIT TRANSACTION;
GO
