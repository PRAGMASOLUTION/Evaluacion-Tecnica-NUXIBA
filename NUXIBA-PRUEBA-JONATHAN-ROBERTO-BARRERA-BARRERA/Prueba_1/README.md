# Evaluación técnica NUXIBA — Desarrollador JR (Backend)

**Autor:** Jonathan Roberto Barrera Barrera  
**Stack:** .NET 8, ASP.NET Core Web API, Entity Framework Core 8, SQL Server 2019 y Docker.

Este repositorio contiene una implementación para los tres ejercicios backend: CRUD de registros de acceso, consultas SQL y descarga CSV. El Excel original se conserva en `data/original/CCenterRIA.xlsx`; los scripts de importación generan staging y reportan anomalías sin descartar filas silenciosamente.

> **Estado de validación:** el entorno en el que se preparó este paquete no tiene instalado `dotnet` ni `docker`. Por ello no se afirma que `dotnet restore`, `dotnet build`, `dotnet test`, migraciones o consultas hayan sido ejecutadas contra SQL Server. El código y los scripts se prepararon estáticamente y deben verificarse en un entorno con .NET 8 y Docker.

## 1. Estructura

```text
NUXIBA-Backend-Jonathan-Barrera/
├── README.md
├── .gitignore
├── .env.example
├── docker-compose.yml
├── Nuxiba.Backend.sln
├── data/original/CCenterRIA.xlsx
├── docs/original-evaluation/    # README original en MD y PDF
├── data/import/                 # CSVs exportados del Excel, sin Password
├── src/Nuxiba.Backend/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Models/
│   ├── Services/
│   ├── Migrations/
│   ├── Program.cs
│   └── appsettings.json
├── tests/Nuxiba.Backend.Tests/
└── sql/
    ├── consultas-ejercicio-2.sql
    ├── 01-create-source-tables.sql
    ├── 02-import-staging.sql
    └── 03-validate-import.sql
```

## 2. Requisitos previos

- Docker Desktop o Docker Engine con Compose.
- .NET SDK 8.0.
- (Opcional) SQL Server Management Studio, Azure Data Studio, `sqlcmd`, Postman o `curl`.

## 3. Levantar SQL Server

Copia `.env.example` a `.env`. La contraseña incluida es únicamente para desarrollo local.

```bash
docker compose up -d
docker compose ps
```

El contenedor publica SQL Server en `localhost:1433`. Usuario: `sa`; contraseña: la variable `MSSQL_SA_PASSWORD` definida en `.env`.

Para detenerlo: `docker compose down`. Para eliminar también los datos persistidos: `docker compose down -v`.

## 4. Configurar y ejecutar la API

La configuración local predeterminada está en `src/Nuxiba.Backend/appsettings.json`. Ajusta `ConnectionStrings:DefaultConnection` si cambias usuario, contraseña, puerto o nombre de base de datos. No subas `.env` ni contraseñas reales.

```bash
dotnet restore Nuxiba.Backend.sln
dotnet build Nuxiba.Backend.sln
dotnet tool install --global dotnet-ef --version 8.*
dotnet ef database update --project src/Nuxiba.Backend --startup-project src/Nuxiba.Backend
dotnet run --project src/Nuxiba.Backend
```

La API usa `http://localhost:5080` y `https://localhost:7080` según `launchSettings.json`. Swagger está habilitado en desarrollo.

### Migraciones

El proyecto incluye una migración inicial para las tablas de la evaluación y `Login`. La tabla `ccloglogin` tiene una columna técnica `Id` como clave primaria porque la hoja Excel solo contiene `User_id`, `Extension`, `TipoMov` y `fecha`; esas cuatro columnas no garantizan unicidad. Se conservan todos los campos originales solicitados.

`ccRIACat_Areas` se mapea con clave compuesta (`IDArea`, `AreaName`) porque el Excel contiene `IDArea=2` dos veces (`BBVA` y `Banamex`). Esto preserva ambas filas. La tabla de usuarios usa `User_id` como clave.

## 5. Importar el Excel

El archivo original se conserva en `data/original/CCenterRIA.xlsx`. Los CSV de `data/import/` fueron generados desde sus hojas reales. En `ccUsers` se excluyó la columna `Password` por no ser necesaria y por evitar exportar hashes.

1. Aplica primero las migraciones EF Core (sección anterior) y después ejecuta `sql/01-create-source-tables.sql`; este script crea staging y el ledger, no recrea las tablas finales.
2. Importa los CSV a las tablas `stg_ccUsers`, `stg_ccRIACat_Areas` y `stg_ccloglogin`, usando el asistente de importación de SSMS o `BULK INSERT` ajustando la ruta a una ruta visible desde SQL Server. Los CSV usan UTF-8 y encabezado.
3. Ejecuta `sql/03-validate-import.sql` para ver usuarios duplicados, áreas ambiguas y referencias inválidas.
4. Después de revisar el reporte, ejecuta el bloque de inserción documentado en `sql/02-import-staging.sql`. No se borran duplicados automáticamente. Los registros de login se insertan con `Id` identity para preservar cada evento, incluso si hay eventos con campos repetidos.

No se incluyen contraseñas ni hashes en los CSV. La hoja de áreas tiene un `IDArea` repetido; no se ha corregido ni eliminado esa fila.

## 6. Endpoints

Base URL local: `http://localhost:5080`.

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/logins` | Lista todos los eventos, ordenados por fecha e Id |
| GET | `/logins/{id}` | Obtiene un evento |
| POST | `/logins` | Crea un login (`TipoMov=1`) o logout (`TipoMov=0`) |
| PUT | `/logins/{id}` | Actualiza un evento y vuelve a validar la secuencia |
| DELETE | `/logins/{id}` | Elimina un evento existente |
| GET | `/reports/hours.csv` | Descarga el reporte de horas por usuario |

### Ejemplo POST login

```json
{
  "user_id": 70,
  "extension": -8,
  "tipoMov": 1,
  "fecha": "2023-01-05T18:02:44"
}
```

### Ejemplo POST logout

```json
{
  "user_id": 70,
  "extension": 9,
  "tipoMov": 0,
  "fecha": "2023-01-06T16:32:30"
}
```

La validación de secuencia se realiza por usuario y fecha, no por extensión: los datos reales muestran que la extensión puede diferir entre login y logout (por ejemplo, usuario 70). Las sesiones se emparejan con el siguiente evento del mismo usuario en orden cronológico y solo se contabiliza una pareja login→logout completa. Una actualización que invalide la secuencia se rechaza.

Códigos previstos: `200 OK`, `201 Created`, `204 No Content`, `400 Bad Request` para validaciones y `404 Not Found`. `POST` responde con `409 Conflict` si la secuencia de sesión es inválida.

## 7. Consultas SQL del ejercicio 2

Abre y ejecuta `sql/consultas-ejercicio-2.sql` después de importar los datos en `dbo.ccloglogin`. La consulta utiliza `LEAD` sobre los eventos ordenados por usuario, fecha e Id. Las sesiones que cruzan meses se asignan al mes de inicio (fecha del login), para que una sesión no se duplique. Las consultas ignoran eventos sin pareja completa; se documentan empates y usuarios sin sesiones completas en el propio script.

La duración se calcula en segundos con `DATEDIFF_BIG(SECOND, ...)`. No se incluyen resultados numéricos en este README porque no se ejecutó el script contra SQL Server en este entorno.

## 8. Descargar CSV

Con la API en ejecución:

```bash
curl -f -o horas-trabajadas.csv http://localhost:5080/reports/hours.csv
```

En Postman, usa `GET http://localhost:5080/reports/hours.csv` y selecciona **Send and Download**. El archivo se codifica UTF-8 con BOM y escapa comillas, comas y saltos de línea según las reglas CSV. Los usuarios sin sesiones completas se incluyen con `TotalHoras=0`.

Ejemplo representativo de encabezado (no son resultados del Excel):

```csv
Usuario,NombreCompleto,Area,TotalHoras
```

## 9. Pruebas

```bash
dotnet test Nuxiba.Backend.sln
```

Las pruebas xUnit incluidas cubren el emparejamiento de sesiones y validaciones de movimientos. No se afirma que pasen: no se pudo ejecutar `dotnet test` en el entorno de generación. Se recomienda completar pruebas de integración contra SQL Server para concurrencia, migraciones y endpoints HTTP.

## 10. Decisiones y limitaciones conocidas

- Se agregó `Id` identity a `ccloglogin` porque el Excel no aporta clave primaria.
- El catálogo de áreas se representa con clave compuesta para no perder el `IDArea=2` duplicado. Para `ccUsers`, `IDArea=1` coincide con `Default`; el catálogo real presenta una anomalía que debe confirmarse con el responsable de datos.
- Los eventos de sesión se emparejan por usuario, ignorando `Extension`, porque en los datos observados el valor cambia entre login y logout. Si el sistema de negocio admite sesiones simultáneas por usuario, el origen debe aportar un identificador de sesión para resolver la asociación sin ambigüedad.
- La integridad de la secuencia se valida en una transacción `Serializable`. Para cargas concurrentes de alta escala se recomienda además una estrategia de bloqueo/serialización por usuario o un identificador de sesión en el modelo de origen.
- La API crea su tabla `ccloglogin` mediante migración; los scripts de importación permiten cargar los eventos reales. No ejecutar la importación en una base con datos existentes sin revisar primero el informe de validación.
- El examen React/Redux que también aparece al final del texto pegado es una prueba distinta. Este entregable se centra en la evaluación backend .NET 8/SQL Server detallada en la primera sección.

## 11. Subir a GitHub

Crea un repositorio vacío en tu cuenta y desde la carpeta descomprimida ejecuta:

```bash
git init
git add .
git commit -m "Completa evaluación técnica NUXIBA backend"
git branch -M main
git remote add origin https://github.com/TU-USUARIO/NUXIBA-Backend-Jonathan-Barrera.git
git push -u origin main
```

Reemplaza la URL por la de tu repositorio. Antes de publicar, revisa que no hayas agregado `.env` ni secretos.
