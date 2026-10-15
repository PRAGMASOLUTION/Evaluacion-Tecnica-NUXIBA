# Evaluación técnica NUXIBA — Desarrollador JR

**Autor:** Jonathan Roberto Barrera Barrera

Este repositorio respeta la estructura de entrega con dos ejercicios independientes y sus instrucciones específicas.

## Estructura

```text
.
├── Prueba_1/   # Backend .NET 8, ASP.NET Core, EF Core y SQL Server
├── Prueba_2/   # Frontend ReactJS, Hooks, Redux Toolkit y JSONPlaceholder
└── README.md   # Índice general
```

## Prueba 1 — Backend

La carpeta `Prueba_1/` contiene la solución de API REST con SQL Server, migraciones, consultas SQL, scripts de importación/validación, reporte CSV, Docker Compose y pruebas xUnit.

Consulta [Prueba_1/README.md](Prueba_1/README.md) para los requisitos, configuración, comandos, endpoints y decisiones técnicas.

## Prueba 2 — Frontend

La carpeta `Prueba_2/` contiene la aplicación React con Hooks y Redux Toolkit para listar usuarios, consultar publicaciones con comentarios, listar tareas en orden descendente y crear una tarea mediante POST.

Consulta [Prueba_2/README.md](Prueba_2/README.md) para instalar dependencias y ejecutar la aplicación.

## Estado de validación

La estructura del repositorio se organizó en las dos carpetas solicitadas. La ejecución de `dotnet build`, `dotnet test`, `npm install` y `npm run build` debe realizarse en un equipo con los SDK y acceso a red disponibles; no se deben considerar aprobadas hasta ejecutarlas localmente.

## Subir a GitHub

Desde la raíz de este repositorio:

```bash
git init
git add .
git commit -m "Entrega evaluación técnica NUXIBA"
git branch -M main
git remote add origin https://github.com/TU-USUARIO/TU-REPOSITORIO.git
git push -u origin main
```

Reemplaza la URL por la de tu repositorio público. No subas contraseñas reales ni secretos.
