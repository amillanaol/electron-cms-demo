# Diseño Técnico: Base de conocimiento versionada

**Autor:** amillanaol  
**Versión:** 1.0  
**Fecha:** 2026-06-09  
**Destinatario:** Equipo técnico

## Introducción

Este documento define el diseño técnico para evolucionar el proyecto desde un CMS ligero hacia una base de conocimiento empresarial con control de versiones, restauración de documentos y eliminación lógica segura. El objetivo es convertir el sistema en una solución robusta donde ningún documento crítico se pierda por un borrado accidental.[cite:162][cite:155][cite:167]

La estrategia recomendada es no depender del borrado físico como operación normal. En su lugar, el sistema debe conservar el contenido fuente, registrar cada cambio como una versión inmutable y mantener una trazabilidad completa de quién hizo qué y cuándo.[cite:162][cite:166][cite:160]

## Objetivo

- Adaptar el modelo de datos para soportar un versionado completo.
- Implementar borrado lógico (Soft Delete) y mecanismos de recuperación.
- Establecer un sistema de auditoría de cambios.
- Mantener la compatibilidad con la arquitectura por capas existente.

## Alcance

### Incluye

- Nuevas entidades para versionado y auditoría.
- Borrado lógico con filtros globales.
- Restauración de documentos eliminados.
- Historial de versiones por documento.
- Reglas de negocio para publicación y archivado.

### No incluye

- Editor visual avanzado.
- Flujos de aprobación complejos.
- Integraciones con Git reales.
- Búsqueda semántica avanzada.
- Permisos detallados por rol.

## Propuesta de evolución

La base de conocimiento debe modelarse con tres conceptos principales: el documento lógico, sus versiones y su auditoría. El documento lógico representa el artículo visible; las versiones almacenan cada cambio relevante; y la auditoría registra la actividad operativa del sistema.[cite:167][cite:169][cite:160]

### Entidades recomendadas

| Entidad | Propósito |
|---|---|
| `KnowledgeDocument` | Identidad lógica del artículo o guía. |
| `KnowledgeDocumentVersion` | Historial inmutable de cambios. |
| `KnowledgeDocumentAudit` | Registro de creación, edición, publicación, borrado lógico y restauración. |
| `KnowledgeDocumentTag` | Taxonomía de categorías y etiquetas. |

## Soft delete

La eliminación lógica debe ser el comportamiento por defecto. En lugar de borrar registros, se debe marcar el documento como eliminado y ocultarlo de las consultas normales mediante filtros globales de EF Core.[cite:162][cite:155][cite:170]

### Campos sugeridos

| Campo | Propósito |
|---|---|
| `IsDeleted` | Indica que el documento fue eliminado lógicamente. |
| `DeletedAt` | Fecha de eliminación lógica. |
| `DeletedBy` | Usuario que realizó la acción. |
| `IsArchived` | Marca contenido obsoleto pero recuperable. |

### Comportamiento esperado

- Las consultas normales no muestran documentos eliminados.
- Los administradores pueden listar documentos eliminados.
- Un documento eliminado puede restaurarse.
- El borrado físico solo se reserva para mantenimiento o purga excepcional.

## Filtros globales

EF Core permite aplicar filtros globales para excluir automáticamente registros eliminados. Este enfoque reduce errores y evita que cada consulta tenga que repetir condiciones de filtrado.[cite:162][cite:155][cite:170]

### Ejemplo conceptual

```csharp
modelBuilder.Entity<KnowledgeDocument>()
    .HasQueryFilter(x => !x.IsDeleted);
```

### Acceso administrativo

Para recuperación o administración, el filtro puede deshabilitarse en consultas concretas cuando sea necesario, permitiendo ver documentos ocultos sin romper la regla general.[cite:162][cite:165]

## Versionado de documentos

Cada actualización relevante debe generar una nueva versión. La versión anterior no se modifica; se conserva como referencia histórica para recuperación o comparación.[cite:167][cite:169][cite:172]

### Campos sugeridos para la versión

| Campo | Propósito |
|---|---|
| `DocumentId` | Documento lógico al que pertenece. |
| `VersionNumber` | Número secuencial de versión. |
| `Title` | Título en ese momento. |
| `MarkdownBody` | Contenido fuente de la versión. |
| `RenderedHtml` | HTML derivado de esa versión. |
| `ChangeSummary` | Resumen de lo modificado. |
| `CreatedAt` | Marca temporal. |
| `CreatedBy` | Autor del cambio. |

### Reglas de versionado

- La versión actual debe quedar marcada explícitamente.
- Toda modificación relevante crea una nueva versión.
- La versión histórica no se edita.
- La restauración debe poder volver a una versión anterior.

## Auditoría

La auditoría debe registrar acciones críticas como creación, edición, publicación, archivo, borrado lógico y restauración. Un audit trail es útil para rastrear actividad, responder incidentes y recuperar contexto operacional.[cite:166][cite:160][cite:158]

### Campos mínimos de auditoría

| Campo | Propósito |
|---|---|
| `EntityName` | Entidad afectada. |
| `EntityId` | Identificador del registro. |
| `Action` | Create, Update, Publish, Archive, Delete, Restore. |
| `PerformedBy` | Usuario responsable. |
| `Timestamp` | Fecha y hora. |
| `ChangesJson` | Resumen estructurado de cambios. |

## Restauración

La restauración debe ser una operación de primer nivel. Si un usuario elimina un documento por error, debe poder recuperarlo sin pérdida de historial. Si además se elimina una versión reciente, el sistema debe permitir volver a una versión previa.[cite:162][cite:167][cite:172]

### Flujo de restauración

1. El administrador localiza el documento eliminado.
2. Elige si desea restaurar el documento o una versión específica.
3. El sistema reactiva el documento y quita el flag de borrado.
4. Se crea un evento de auditoría de restauración.
5. La operación queda visible en el historial.

## Reglas de negocio propuestas

- Nunca borrar físicamente un documento operativo por defecto.
- Toda edición relevante debe crear una versión nueva.
- Todo borrado debe ser lógico.
- Toda restauración debe quedar auditada.
- El documento visible siempre debe apuntar a la versión actual aprobada.

## Impacto sobre la arquitectura

La solución actual puede evolucionar sin rehacer todo el proyecto. La API seguirá siendo el punto de acceso, pero la capa de dominio necesitará nuevas entidades, la infraestructura deberá soportar filtros, y los endpoints deberán agregar consultas de historial y restauración.[cite:75][cite:84][cite:162]

### Áreas afectadas

- **Domain**: nuevas entidades y reglas.
- **Application**: casos de uso de versionado, restore y auditoría.
- **Infrastructure**: filtros globales, tablas nuevas y persistencia.
- **Api**: endpoints de restauración e historial.
- **Electron**: nuevas vistas de historial y documentos eliminados.

## Beneficios para una empresa de ventas

Esta evolución es especialmente útil si los artículos representan procedimientos comerciales, scripts de ventas, documentación de producto o contenido operativo. En ese contexto, perder un documento equivale a perder conocimiento interno, por lo que el versionado y la restauración son más importantes que la simple edición rápida.[cite:167][cite:138][cite:157]

## Entregables sugeridos para implementar esta mejora

- Nueva entidad de documento lógico.
- Tabla de versiones.
- Tabla de auditoría.
- Filtros globales de soft delete.
- Endpoints de restauración e historial.
- Vista en Electron para ver versiones y recuperar documentos.

## Riesgos y mitigaciones

| Riesgo | Impacto | Mitigación |
|---|---|---|
| Borrado irreversible accidental | Alto | Usar soft delete y deshabilitar borrado físico por defecto.[cite:162][cite:155] |
| Pérdida de contexto histórico | Alto | Guardar versiones inmutables y auditoría.[cite:166][cite:160] |
| Complejidad excesiva temprana | Medio | Implementar primero soft delete y versión simple. |
| Consultas inconsistentes | Medio | Usar filtros globales y casos de uso claros.[cite:162][cite:170] |

## Bloque de demora posible

Esta mejora puede tardar más de lo previsto si se intenta resolver al mismo tiempo versionado, auditoría, restauración, permisos y una interfaz de comparación de versiones. También puede alargarse si se decide migrar datos existentes o ajustar el modelo actual sin un plan de transición.

Para evitar retrasos, conviene implementar en este orden: soft delete, versionado, auditoría y luego restauración avanzada.

## Cierre

La recomendación es clara: el proyecto sí puede adaptarse para convertirse en una base de conocimiento robusta y recuperable. La clave está en tratar el contenido como un activo empresarial que nunca debe desaparecer sin rastro, sino evolucionar mediante versiones y trazabilidad completa.[cite:162][cite:167][cite:158]
