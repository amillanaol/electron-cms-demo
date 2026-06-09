# Resolucion de Errores

Errores comunes durante el desarrollo, ejecucion y despliegue del proyecto.

## Errores de Desarrollo

| Sintoma | Causa Raiz | Solucion Tecnica |
| :--- | :--- | :--- |
| `error NETSDK1004: Assets file '.../project.assets.json' not found` | Proyecto .NET no restaurado | Ejecutar `dotnet restore KnowVault-Core.slnx` desde `backend/` |
| `The connection string 'Default' is not configured` | Base de datos no disponible o .env no configurado | Verificar `ConnectionStrings__Default` en entorno o levantar `docker compose up -d` |
| `A database operation failed while seeding the database` | Migraciones pendientes o PostgreSQL no responde | Ejecutar `dotnet ef database update` o verificar `docker compose ps` para estado de postgres |
| `Failed to bind to address http://localhost:8080` | Puerto 8080 ya en uso por otro proceso | Detener proceso con `Stop-Process -Id (Get-NetTCPConnection -LocalPort 8080).OwningProcess` |
| `Cannot find module '@playwright/test'` | Dependencias Node no instaladas | Ejecutar `npm install` desde la raiz del proyecto |
| `Playwright tests fail: net::ERR_CONNECTION_REFUSED` | API no esta corriendo | Iniciar API con `docker compose up -d` o `dotnet run` desde `backend/src/KnowVault-Core.Api/` |
| `error: 'dotnet-ef' is not recognized` | Herramienta EF Core no instalada globalmente | Ejecutar `dotnet tool install --global dotnet-ef` |
| `electron-builder: command not found` | Dependencias de electron-app no instaladas | Ejecutar `npm install` desde `electron-app/` |
| `Npgsql.PostgresException: 42P01: relation "content_documents" does not exist` | Migraciones no aplicadas | Ejecutar `dotnet ef database update` o reiniciar contenedor con `docker compose down -v && docker compose up -d` |

## Errores de Docker

| Sintoma | Causa Raiz | Solucion Tecnica |
| :--- | :--- | :--- |
| `Error response from daemon: driver failed programming external connectivity` | Puerto 5432 o 8080 ya en uso | Detener servicios locales: `Stop-Process` en PostgreSQL local o cambiar puertos en `docker-compose.yml` |
| `Service 'api' failed to build: no such file or directory` | Contexto de build incorrecto | Verificar que `docker compose build` se ejecuta desde la raiz del proyecto (donde esta `backend/`) |
| `healthcheck: exit code 1` | PostgreSQL no acepta conexiones | Esperar 10-15s, el primer healthcheck puede fallar mientras arranca |

## Errores de Tests

| Sintoma | Causa Raiz | Solucion Tecnica |
| :--- | :--- | :--- |
| `System.InvalidOperationException: Can't configure WebApplicationFactory` | `InternalsVisibleTo` no configurado en Api.csproj | Agregar `<InternalsVisibleTo Include="KnowVault-Core.IntegrationTests" />` al csproj de la API |
| `Failed to create unique constraint: duplicate key value violates unique constraint` | Slug duplicado en tests de integracion secuenciales | Usar `Guid.NewGuid()` para generar slugs unicos en cada ejecucion de test |
| `xUnit.net: Assembly integration failed: Unit test not found` | Tests no compilados o filtro incorrecto | Ejecutar `dotnet test --verbosity normal` para ver detalles de compilacion |

