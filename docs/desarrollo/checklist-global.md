# Checklist Global del Proyecto: KnowVault-Core

Este documento resume las tareas necesarias para completar cada fase del desarrollo de KnowVault-Core.

## Fase 0: Definición y preparación
- [ ] Definir alcance inicial y requisitos.
- [ ] Configurar repositorio y estructura de directorios base.

## Fase 1: Backend base
- [ ] Configurar solución .NET, proyectos Domain, Application, Infrastructure, Api.
- [ ] Definir entidades base.

## Fase 2: Persistencia y modelo editorial
- [ ] Implementar `KnowVault-CoreDbContext`.
- [ ] Configurar Entity Framework Core con PostgreSQL.
- [ ] Definir modelo editorial básico.
- [ ] Definir entidades `Version`, `Audit`. Campos `SoftDelete` en `KnowledgeDocument`.

## Fase 3: Pipeline Markdown seguro
- [ ] Implementar servicio `IMarkdownRenderer` (Markdig).
- [ ] Configurar sanitización de seguridad para el renderizado Markdown.
- [ ] Implementar versionado inmutable al editar.

## Fase 4: Endpoints de consulta y publicación
- [ ] Implementar `ContentController`.
- [ ] Desarrollar lógica de negocio en `ContentService`.
- [ ] Exponer endpoints REST API.
- [ ] Implementar endpoints de restauración, historial y filtros globales (SoftDelete).

## Fase 5: Docker Compose y entorno local
- [ ] Crear `Dockerfile` para la API.
- [ ] Configurar `docker-compose.yml` (API + PostgreSQL).
- [ ] Configurar entorno local de desarrollo.

## Fase 6: Cliente Electron MVP
- [ ] Configurar proceso principal (`main.js`) y preload (`preload.js`).
- [ ] Desarrollar UI base en `renderer/` (HTML/CSS/JS).
- [ ] Implementar puente seguro entre UI y API.
- [ ] UI para historial, gestión de papelera y restauración.

## Fase 7: Pruebas y endurecimiento
- [ ] Escribir tests unitarios (`ContentService`, `MarkdownRenderer`).
- [ ] Escribir tests de integración API.
- [ ] Escribir tests E2E con Playwright (Smoke test).
- [ ] Tests flujo completo: Edición -> Versión -> Borrado -> Restauración.

## Fase 8: Empaquetado Windows
- [ ] Configurar `electron-builder.yml`.
- [ ] Configurar pipeline de build para Windows (.exe).

