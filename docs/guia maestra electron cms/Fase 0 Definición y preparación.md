# Guía maestra - Fase 0: Definición y preparación

**Autor:** amillanaol  
**Versión:** 1.0  
**Fecha:** 2026-06-04  
**Destinatario:** Agente IA de implementación / equipo técnico

## Introducción

Esta fase establece las bases del proyecto antes de escribir la primera línea de código funcional. El objetivo es cerrar decisiones de arquitectura, delimitar el alcance del MVP y dejar definidos los artefactos iniciales para que el resto de la implementación avance con menos retrabajo y menos ambigüedad. La fase 0 es especialmente importante porque evita que backend, base de datos y Electron crezcan de forma desordenada.[cite:75][cite:84]

El proyecto se concibe como un CMS ligero orientado a documentos Markdown, con API en ASP.NET Core, persistencia relacional, consumo desde Electron y despliegue local mediante Docker Compose. .NET 9 aporta soporte nativo para OpenAPI, lo que permite documentar la API desde el inicio sin depender exclusivamente de plantillas antiguas de Swagger.[cite:89][cite:90][cite:102]

## Objetivos de la fase

- Definir la estructura raíz del repositorio.
- Acordar la arquitectura por capas del backend.
- Elegir base de datos y cliente desktop.
- Establecer convenciones de nombres, ramas y variables de entorno.
- Realizar el setup inicial de Playwright para validación continua.
- Preparar documentación mínima para continuar con la fase 1.

## Alcance

### Incluye

- Estructura de carpetas del repositorio.
- Decisiones tecnológicas principales.
- Definición de convenciones de trabajo.
- Lista inicial de entregables.
- Riesgos y mitigaciones de arranque.

### No incluye

- Código funcional de la API.
- Persistencia con EF Core.
- Renderizado Markdown.
- Interfaz Electron operativa.
- Migraciones de base de datos.

## Decisiones técnicas

### Backend

Se recomienda usar **ASP.NET Core Web API en .NET 9**. Esta elección permite aprovechar OpenAPI nativo para documentar endpoints desde el principio, reducir dependencias externas y mantener una base moderna para el futuro crecimiento del proyecto.[cite:89][cite:90][cite:111]

### Persistencia

Se recomienda **PostgreSQL** como base relacional inicial. Es una opción estable, bien soportada por Docker Compose y adecuada para una PoC o un MVP de contenido documental.[cite:80][cite:86]

### Cliente desktop

Se recomienda **Electron** como frontend ejecutable para Windows. Esta decisión permite reutilizar tecnologías web y controlar de forma clara la separación entre procesos `main`, `preload` y `renderer`, algo relevante para la seguridad de la app.[cite:56][cite:61][cite:95]

### Documentación API

La API debe exponerse con **OpenAPI nativo** en .NET 9. Esto simplifica la exploración de contratos desde el inicio y hace más fácil que el resto del proyecto consuma un contrato bien definido.[cite:89][cite:90][cite:102]

## Estrategia de Testing Inicial (Playwright)

En esta fase se debe inicializar Playwright en la raíz del proyecto para permitir que cada fase posterior sea validada de forma automatizada mediante el patrón **Setup de Test -> Implementación -> Validación**.

1. **Inicialización**: Ejecutar `npm init playwright@latest` en la raíz.
2. **Configuración**: Ajustar `playwright.config.ts` para que apunte a la carpeta `tests-e2e/`.
3. **Reporter**: Configurar el reporter `list` por defecto para obtener feedback inmediato en la shell.
4. **Primer Test**: Crear `tests-e2e/fase0.spec.ts` que valide que el entorno de Node y Playwright es operativo.

## Estructura del repositorio

Se propone la siguiente estructura raíz:

```text
KnowVault-Core-sample/
├─ backend/
├─ electron-app/
├─ tests-e2e/
├─ docker/
├─ docs/
├─ .env.example
├─ docker-compose.yml
├─ playwright.config.ts
└─ README.md
```

### Backend

```text
backend/
├─ src/
│  ├─ KnowVault-Core.Api/
│  ├─ KnowVault-Core.Application/
│  ├─ KnowVault-Core.Domain/
│  └─ KnowVault-Core.Infrastructure/
├─ tests/
│  ├─ KnowVault-Core.UnitTests/
│  └─ KnowVault-Core.IntegrationTests/
└─ KnowVault-Core.sln
```

### Electron

```text
electron-app/
├─ src/
│  ├─ main/
│  ├─ preload/
│  ├─ renderer/
│  └─ shared/
├─ assets/
├─ package.json
└─ electron-builder.yml
```

Esta estructura reduce acoplamiento y deja claro qué parte del sistema se encarga de cada responsabilidad. También deja espacio para pruebas y para el empaquetado Windows posterior.[cite:75][cite:76][cite:82][cite:84]

## Convenciones de trabajo

### Nombres

- Clases y proyectos en `PascalCase`.
- Variables y propiedades JSON en `camelCase`.
- Slugs y rutas amigables en `kebab-case`.

### Ramas

- `main` para estable.
- `develop` para integración.
- `feature/*` para trabajo incremental.

### Variables de entorno

| Variable | Uso |
|---------|-----|
| `ASPNETCORE_ENVIRONMENT` | Ambiente de ejecución de la API. |
| `ConnectionStrings__Default` | Cadena de conexión a PostgreSQL. |
| `KnowVault-Core_API_BASE_URL` | URL base consumida por Electron. |
| `POSTGRES_DB` | Nombre de la base de datos. |
| `POSTGRES_USER` | Usuario de la base de datos. |
| `POSTGRES_PASSWORD` | Clave local de desarrollo. |

## Entregables esperados

- Estructura inicial del repositorio creada.
- Decisiones tecnológicas registradas en documentación.
- Convenciones de trabajo definidas.
- Checklist de riesgos iniciales preparado.
- Base lista para iniciar la fase 1.

## Riesgos y mitigaciones

| Riesgo | Impacto | Mitigación |
|-------|---------|------------|
| Alcance demasiado amplio desde el inicio | Alto | Mantener el MVP enfocado en lectura y visualización de documentos.[cite:75][cite:84] |
| Mezcla prematura de capas | Alto | Definir responsabilidades claras para Domain, Application, Infrastructure y Api.[cite:75][cite:76] |
| Falta de contrato API temprano | Medio | Activar OpenAPI desde la primera fase de backend.[cite:89][cite:90] |
| Incertidumbre con el cliente desktop | Medio | Elegir Electron y mantenerlo como consumidor separado de la API.[cite:56][cite:95] |

## Bloque de demora posible

La fase 0 puede retrasarse si no se cierra pronto la arquitectura o si se agregan decisiones fuera del MVP, como autenticación avanzada, edición colaborativa, versionado complejo o sincronización offline. También puede haber demoras si se redefine la estrategia de frontend o si se decide cambiar la base de datos después de haber empezado la estructuración inicial.

Para minimizar ese riesgo, conviene aprobar la estructura del repositorio, las tecnologías base y el alcance del MVP antes de pasar a la fase 1.

## Siguiente paso

La salida natural de esta fase es iniciar la **Fase 1: Backend base**, donde se crea la solución .NET, se habilita OpenAPI y se prepara el esqueleto del entorno local con Docker Compose.[cite:89][cite:90][cite:102]

