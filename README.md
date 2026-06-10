# KnowVault-Core

CMS ligero con ASP.NET Core 10, PostgreSQL, Electron y Docker Compose.

## Descripcion

Sistema de gestion de contenidos (CMS) con editor Markdown, pipeline de renderizado seguro, busqueda full-text y cliente desktop Electron. Implementado con ASP.NET Core Web API en .NET 10 siguiendo una arquitectura por capas (Domain, Application, Infrastructure, Api) con Entity Framework Core para persistencia en PostgreSQL y Markdig para procesamiento Markdown. El cliente desktop Electron se comunica con la API mediante un puente seguro preload con contextIsolation. La infraestructura se despliega con Docker Compose orquestando API + PostgreSQL.

## Indice de la documentacion

| Necesidad | Ubicacion |
| :--- | :--- |
| Instalar y ejecutar localmente | [docs/desarrollo/instalacion_local.md](docs/desarrollo/instalacion_local.md) |
| Configurar variables de entorno | [docs/configuracion/env_puerto.md](docs/configuracion/env_puerto.md) |
| Entender la arquitectura del proyecto | [docs/arquitectura/capas_dotnet.md](docs/arquitectura/capas_dotnet.md) |
| Consultar endpoints de la API REST | [docs/api/endpoints_rest.md](docs/api/endpoints_rest.md) |
| Desplegar con Docker Compose | [docs/configuracion/docker_despliegue.md](docs/configuracion/docker_despliegue.md) |
| Ejecutar tests | [docs/desarrollo/tests_ejecucion.md](docs/desarrollo/tests_ejecucion.md) |
| Resolver errores comunes | [docs/errores/general_resolucion.md](docs/errores/general_resolucion.md) |
| Guia maestra del proyecto | [docs/guia%20maestra%20electron%20cms/guia-maestra.md](docs/guia%20maestra%20electron%20cms/guia-maestra.md) |
| Componentes del cliente Electron | [docs/arquitectura/electron_puente_seguro.md](docs/arquitectura/electron_puente_seguro.md) |
| Pipeline Markdown y sanitizacion | [docs/seguridad/pipeline_markdown.md](docs/seguridad/pipeline_markdown.md) |
| Flujo de trabajo editorial | [docs/arquitectura/workflow_editorial.md](docs/arquitectura/workflow_editorial.md) |
| Empaquetado Windows (.exe) | [docs/desarrollo/empaquetado_windows.md](docs/desarrollo/empaquetado_windows.md) |

## Stack Tecnico del proyecto

| Componente | Tecnologia | Version |
| :--- | :--- | :--- |
| Lenguaje (backend) | C# | 13 (.NET 10) |
| Framework Web | ASP.NET Core Web API | 10.0 |
| Procesamiento Markdown | Markdig | 1.2.0 |
| ORM | Entity Framework Core | 10.0.8 |
| Base de Datos (prod) | PostgreSQL | 16 |
| Base de Datos (dev) | EF Core InMemory | 10.0.8 |
| Cliente Desktop | Electron | 34.x |
| Empaquetado Windows | electron-builder + NSIS | 25.x |
| Contenedores | Docker + Compose | multi-stage |
| Testing Unitario | xUnit + Moq | 2.9.3 / 4.20.72 |
| Testing Integracion | xUnit + Microsoft.AspNetCore.Mvc.Testing | 10.0.8 |
| Testing E2E | Playwright | 1.52.x |
| SDK Backend | .NET SDK | 10.0 |
| SDK Frontend | Node.js | 20+ |

## Estructura del Proyecto

```
backend/                        # Solucion .NET por capas
  KnowVault-Core.slnx                  # Archivo de solucion (formato .slnx)
  src/
    KnowVault-Core.Domain/             # Capa de dominio: entidades y enums puras
    KnowVault-Core.Application/        # Capa de aplicacion: casos de uso, DTOs, interfaces
    KnowVault-Core.Infrastructure/     # Capa de infraestructura: EF Core, PostgreSQL, Markdown, repositorios
    KnowVault-Core.Api/                # Capa de presentacion: controladores, middleware, OpenAPI, Dockerfile
  tests/
    KnowVault-Core.UnitTests/          # Tests unitarios (MarkdownRenderer + ContentService)
    KnowVault-Core.IntegrationTests/   # Tests de integracion (API endpoints)

docker/                         # Archivos de infraestructura
  docker-compose.yml            # Orquestacion: api + postgres + healthcheck

electron-app/                   # Cliente desktop Electron
  src/
    main/                       # Proceso principal (BrowserWindow, app lifecycle)
    preload/                    # Bridge seguro (contextBridge, AbortController, timeout)
    renderer/                   # UI: HTML semantico, CSS moderno, JS vanilla
      css/app.css               # Estilos con variables CSS, layout flexbox
      js/
        api-client.js           # Fachada para window.KnowVault-Core
        app.js                  # Orquestador: carga inicial, busqueda, estados
        ui/
          document-list.js      # Lista de documentos con loading/error/empty states
          document-viewer.js    # Visor de documento con HTML renderizado seguro
    shared/                     # Configuracion compartida
  electron-builder.yml          # Configuracion de empaquetado NSIS
  package.json

tests-e2e/                      # Tests de validacion Playwright por fase
  fase0.spec.ts a fase7-resilience.spec.ts
  smoke.spec.ts

playwright.config.ts            # Configuracion Playwright (baseURL, reporter)
.env.example                    # Template de variables de entorno
```

## Inicio Rapido

```bash
# 1. Copiar variables de entorno
cp .env.example .env

# 2. Iniciar API + base de datos
cd docker
docker compose up -d
cd ..

# 3. Ejecutar tests del backend
cd backend
dotnet test KnowVault-Core.slnx

# 4. Instalar dependencias y ejecutar tests E2E
cd ..
npm install
npx playwright install chromium
npx playwright test --reporter=list
```

Endpoints disponibles en http://localhost:8080

## Ejecucion de Tests

Detalle completo de comandos, alcance y requisitos en [docs/desarrollo/tests_ejecucion.md](docs/desarrollo/tests_ejecucion.md).

| Comando | Alcance |
| :--- | :--- |
| `dotnet test KnowVault-Core.slnx` | Unitarios (34) + Integracion (19) |
| `npx playwright test` | E2E (22+ escenarios) |

## Resolucion de Errores

Casos diagnosticados y soluciones en [docs/errores/general_resolucion.md](docs/errores/general_resolucion.md).

Problemas frecuentes: conflictos de slug en tests, puerto 8080 ocupado, migraciones pendientes, dependencias Node no instaladas, y errores de conexion Docker.

## Control de versiones

| Campo | Valor |
| :--- | :--- |
| **Mantenedor** | [amillanaol](https://github.com/amillanaol) |
| **Estado** | En desarrollo |
| **Ultima Actualizacion** | 2026-06-04 |

