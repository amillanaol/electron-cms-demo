# Tests

Documentacion detallada de ejecucion de tests del proyecto.

## Comandos de Ejecucion

| Comando | Alcance | Requisitos |
| :--- | :--- | :--- |
| `dotnet test tests/KnowVault-Core.UnitTests` | Unitarios (MarkdownRenderer + ContentService) | Ninguno (puro .NET, mocks) |
| `dotnet test tests/KnowVault-Core.IntegrationTests` | Integracion (API endpoints con WebApplicationFactory) | .NET SDK, configuracion InMemory |
| `dotnet test KnowVault-Core.slnx` | Todos (unitarios + integracion) | .NET SDK |
| `npx playwright test` | E2E (validacion de fases 0-8) | API corriendo (docker compose up), Node.js |
| `npx playwright test --reporter=list tests-e2e/fase7-resilience.spec.ts` | E2E resiliencia especifica | API corriendo, Node.js |

## Descripcion de Tests

### Tests Unitarios (`KnowVault-Core.UnitTests`)

Ubicacion: `backend/tests/KnowVault-Core.UnitTests/`

34 tests que validan la logica de negocio sin dependencias externas:

- **MarkdownRendererTests**: 21 tests de sanitizacion y renderizado Markdown. Verifican que `DisableHtml()` escapa HTML crudo (`<script>`, `onclick`, etc.), que la sanitizacion regex neutraliza esquemas peligrosos (`javascript:`, `vbscript:`) en enlaces Markdown, y que el pipeline completo produce HTML seguro.
- **ContentServiceTests**: 13 tests del caso de uso `ContentService`. Validan creacion, actualizacion, publicacion, archivado, busqueda y obtencion de documentos usando repositorio mockeado (Moq).

Framework: xUnit + Moq.

### Tests de Integracion (`KnowVault-Core.IntegrationTests`)

Ubicacion: `backend/tests/KnowVault-Core.IntegrationTests/`

19 tests que validan los endpoints HTTP contra una instancia real de la aplicacion usando `WebApplicationFactory<Program>` con configuracion InMemory (sin PostgreSQL):

- CRUD completo (crear, listar, obtener por slug, actualizar)
- Workflow de estados (crear -> publicar -> archivar)
- Validacion de entrada (campos requeridos, slugs vacios, busqueda sin texto)
- Renderizado Markdown via API
- Sanitizacion de HTML crudo y event handlers en endpoint `/api/markdown/render`
- Health check (`/health`) y ping (`/api/ping`)

Framework: xUnit + Microsoft.AspNetCore.Mvc.Testing.

### Tests E2E (Playwright)

Ubicacion: `tests-e2e/`

22+ tests que validan el sistema completo desde la perspectiva del cliente:

| Archivo | Cobertura |
| :--- | :--- |
| `fase0.spec.ts` | Validacion de entorno (Node.js, Playwright) |
| `fase1-api.spec.ts` | Health, ping, OpenAPI |
| `fase2-persistencia.spec.ts` | CRUD, slug unico, migraciones |
| `fase3-security.spec.ts` | Sanitizacion Markdown, XSS |
| `fase4-workflow.spec.ts` | Publicacion, archivado, estados |
| `fase5-docker.spec.ts` | Docker Compose, healthcheck PostgreSQL |
| `fase6-electron.spec.ts` | Cliente Electron, preload, renderer |
| `fase7-resilience.spec.ts` | Resiliencia: API offline, latencia, contenido corrupto, endurecimiento |
| `smoke.spec.ts` | Smoke tests de empaquetado Windows |

## Configuracion

Los tests de integracion usan `WebApplicationFactory` con base de datos InMemory, no requieren PostgreSQL en ejecucion.

Los tests E2E requieren la API corriendo (localmente con `dotnet run` o via `docker compose up -d`) y Playwright configurado en `playwright.config.ts` con `baseURL` apuntando a `http://localhost:8080`.

Para ejecutar tests E2E por primera vez, instalar navegadores:
```bash
npx playwright install chromium
```

