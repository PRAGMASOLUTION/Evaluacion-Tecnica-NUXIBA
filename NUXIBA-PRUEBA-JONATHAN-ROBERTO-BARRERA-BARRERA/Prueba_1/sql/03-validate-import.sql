USE NuxibaEvaluation;
GO
SET NOCOUNT ON;

PRINT 'Usuarios duplicados por User_id en staging:';
SELECT TRY_CONVERT(int, User_id) AS User_id, COUNT(*) AS Cantidad
FROM dbo.stg_ccUsers
GROUP BY TRY_CONVERT(int, User_id)
HAVING COUNT(*) > 1;

PRINT 'Áreas duplicadas por IDArea (el Excel contiene IDArea=2 con nombres distintos):';
SELECT TRY_CONVERT(int, IDArea) AS IDArea,
       COUNT(*) AS Filas,
       STRING_AGG(AreaName, ' | ') AS Nombres
FROM dbo.stg_ccRIACat_Areas
GROUP BY TRY_CONVERT(int, IDArea)
HAVING COUNT(*) > 1;

PRINT 'Eventos con campos no convertibles o TipoMov inválido:';
SELECT *
FROM dbo.stg_ccloglogin
WHERE TRY_CONVERT(int, User_id) IS NULL
   OR TRY_CONVERT(int, Extension) IS NULL
   OR TRY_CONVERT(int, TipoMov) NOT IN (0,1)
   OR TRY_CONVERT(datetime2, fecha) IS NULL;

PRINT 'Eventos que apuntan a usuarios inexistentes en staging:';
SELECT l.*
FROM dbo.stg_ccloglogin l
LEFT JOIN dbo.stg_ccUsers u ON TRY_CONVERT(int, u.User_id) = TRY_CONVERT(int, l.User_id)
WHERE u.User_id IS NULL;

PRINT 'Usuarios con área sin referencia en catálogo:';
SELECT u.User_id, u.Login, u.IDArea
FROM dbo.stg_ccUsers u
LEFT JOIN dbo.stg_ccRIACat_Areas a ON TRY_CONVERT(int, u.IDArea) = TRY_CONVERT(int, a.IDArea)
WHERE NULLIF(u.IDArea, '') IS NOT NULL AND a.IDArea IS NULL;
GO
