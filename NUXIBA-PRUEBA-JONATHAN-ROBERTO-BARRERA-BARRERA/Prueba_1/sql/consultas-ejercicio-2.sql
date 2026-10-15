/*
NUXIBA - Ejercicio 2. Ejecutar en la base NuxibaEvaluation después de importar datos.
Criterio: cada evento se asocia al siguiente evento del mismo User_id, ordenado por fecha e Id.
Solo una secuencia TipoMov=1 seguida inmediatamente por TipoMov=0 forma sesión completa.
Extension no se usa como llave de emparejamiento porque en el Excel real cambia entre login/logout.
Las sesiones que cruzan meses se asignan al mes del login (inicio); así no se duplica duración.
*/
USE NuxibaEvaluation;
GO
SET NOCOUNT ON;

IF OBJECT_ID('tempdb..#Sessions') IS NOT NULL DROP TABLE #Sessions;

;WITH EventsWithNext AS
(
    SELECT
        User_id,
        Id,
        Extension,
        TipoMov,
        fecha AS StartOrEnd,
        LEAD(TipoMov) OVER (PARTITION BY User_id ORDER BY fecha, Id) AS NextTipoMov,
        LEAD(fecha) OVER (PARTITION BY User_id ORDER BY fecha, Id) AS NextFecha,
        LEAD(Id) OVER (PARTITION BY User_id ORDER BY fecha, Id) AS NextId
    FROM dbo.ccloglogin
)
SELECT
    User_id,
    Id AS LoginId,
    NextId AS LogoutId,
    StartOrEnd AS LoginFecha,
    NextFecha AS LogoutFecha,
    DATEDIFF_BIG(SECOND, StartOrEnd, NextFecha) AS TotalSegundos
INTO #Sessions
FROM EventsWithNext
WHERE TipoMov = 1
  AND NextTipoMov = 0
  AND NextFecha >= StartOrEnd;

CREATE CLUSTERED INDEX IX_Sessions_User_LoginFecha ON #Sessions(User_id, LoginFecha);

PRINT 'A. Usuario con mayor tiempo acumulado de sesiones completas (todos los empates):';
;WITH Totals AS
(
    SELECT User_id, SUM(TotalSegundos) AS TotalSegundos
    FROM #Sessions
    GROUP BY User_id
),
Ranked AS
(
    SELECT *, DENSE_RANK() OVER (ORDER BY TotalSegundos DESC) AS rn
    FROM Totals
)
SELECT User_id, TotalSegundos,
       TotalSegundos / 86400 AS Dias,
       (TotalSegundos % 86400) / 3600 AS Horas,
       (TotalSegundos % 3600) / 60 AS Minutos,
       TotalSegundos % 60 AS Segundos
FROM Ranked
WHERE rn = 1;

PRINT 'B. Usuario con menor tiempo acumulado entre quienes tienen al menos una sesión completa (todos los empates):';
;WITH Totals AS
(
    SELECT User_id, SUM(TotalSegundos) AS TotalSegundos
    FROM #Sessions
    GROUP BY User_id
),
Ranked AS
(
    SELECT *, DENSE_RANK() OVER (ORDER BY TotalSegundos ASC) AS rn
    FROM Totals
)
SELECT User_id, TotalSegundos,
       TotalSegundos / 86400 AS Dias,
       (TotalSegundos % 86400) / 3600 AS Horas,
       (TotalSegundos % 3600) / 60 AS Minutos,
       TotalSegundos % 60 AS Segundos
FROM Ranked
WHERE rn = 1;

PRINT 'C. Promedio mensual por usuario, mes de inicio de sesión:';
SELECT
    User_id,
    DATEFROMPARTS(YEAR(LoginFecha), MONTH(LoginFecha), 1) AS Mes,
    COUNT_BIG(*) AS CantidadSesionesCompletas,
    SUM(TotalSegundos) AS TiempoTotalSegundos,
    CAST(AVG(CAST(TotalSegundos AS decimal(19,2))) AS decimal(19,2)) AS PromedioSegundosPorSesion,
    CAST(AVG(CAST(TotalSegundos AS decimal(19,2))) / 3600.0 AS decimal(19,4)) AS PromedioHorasPorSesion
FROM #Sessions
GROUP BY User_id, DATEFROMPARTS(YEAR(LoginFecha), MONTH(LoginFecha), 1)
ORDER BY User_id, Mes;

-- Criterios:
-- * Usuarios sin sesiones completas no participan en max/min porque no existe un total medible.
-- * DENSE_RANK devuelve todos los empates.
-- * Para bases grandes se recomienda índice (User_id, fecha, Id) INCLUDE (TipoMov, Extension).
-- * El Id es necesario para ordenar determinísticamente eventos con la misma fecha.
-- * Si el negocio permite sesiones simultáneas, los datos actuales no contienen un session-id fiable:
--   emparejar por Extension tampoco sería correcto para el Excel proporcionado.
DROP TABLE #Sessions;
GO
