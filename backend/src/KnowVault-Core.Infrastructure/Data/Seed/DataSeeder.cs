using KnowVaultCore.Application.Interfaces;
using KnowVaultCore.Domain.Entities;
using KnowVaultCore.Domain.Enums;

namespace KnowVaultCore.Infrastructure.Data.Seed;

public static class DataSeeder
{
    public static (List<ContentDocument> docs, List<ContentDocumentVersion> versions, List<ContentDocumentAudit> audits) GetSeedData(IMarkdownRenderer renderer)
    {
        var now = DateTime.UtcNow;

        var docs = new List<ContentDocument>
        {
            new()
            {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000001"),
                Slug = "bienvenida",
                Title = "Bienvenido a KnowVaultCore",
                Summary = "Introduccion al sistema de gestion de contenidos con Markdown y Electron.",
                MarkdownBody = @"# Bienvenido a KnowVaultCore

Bienvenido a **KnowVaultCore**, un sistema de gestion de contenidos ligero diseñado para equipos que necesitan publicar documentacion tecnica de forma rapida y segura.

## Que es KnowVaultCore

KnowVaultCore es una plataforma editorial que combina la potencia de Markdown como lenguaje de escritura con un pipeline de renderizado seguro que protege contra ataques XSS e inyeccion de HTML. Todo el contenido se almacena en PostgreSQL y se sirve a traves de una API REST construida con ASP.NET Core.

Caracteristicas principales:

- Editor basado en Markdown con previsualizacion en vivo
- Pipeline de sanitizacion que neutraliza `javascript:`, `vbscript:` y event handlers HTML
- Busqueda full-text sobre titulos, resumenes y slugs
- Cliente desktop Electron con aislamiento de contexto
- Despliegue via Docker Compose con API + PostgreSQL + Frontend web

## Como usar este sistema

Para empezar a trabajar con KnowVaultCore, solo necesitas escribir tus documentos en Markdown y publicarlos a traves de la API. El sistema se encarga del resto:

1. **Escribe** tu contenido usando sintaxis Markdown estandar
2. **Renderiza** el HTML seguro automaticamente al guardar
3. **Publica** cuando estes listo para compartir
4. **Busca** entre todos los documentos publicados

Cada documento pasa por un flujo editorial completo: *Borrador*, *Publicado* y *Archivado*. Esto permite mantener un control de versiones sobre el estado de cada publicacion.

## Ejemplo de formato

Puedes usar toda la potencia de Markdown en tus documentos:

```markdown
# Titulo principal
## Subtitulo
### Encabezado nivel 3

Texto con **negrita**, *cursiva* y `codigo inline`.

- Lista
- De
- Elementos

1. Lista
2. Numerada

[Enlaces](https://ejemplo.com)
```

> **Nota:** Todo el HTML generado pasa por un proceso de sanitizacion que elimina etiquetas peligrosas y esquemas de URL maliciosos. Puedes escribir con tranquilidad sabiendo que tu contenido se mostrara de forma segura en cualquier cliente.

El cliente desktop Electron incluye un puente de comunicacion seguro mediante `contextBridge` que aísla el proceso de renderizado del proceso principal, siguiendo las mejores practicas de seguridad recomendadas por el equipo de Electron.
",
                Status = DocumentStatus.Published,
                CreatedAt = now,
                UpdatedAt = now,
                PublishedAt = now,
                CurrentVersion = 1
            },
            new()
            {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000002"),
                Slug = "guia-instalacion-local",
                Title = "Guia de instalacion local",
                Summary = "Pasos detallados para levantar el entorno de desarrollo completo de KnowVaultCore.",
                MarkdownBody = @"# Guia de instalacion local

Esta guia describe los pasos necesarios para configurar y ejecutar KnowVaultCore en tu maquina local utilizando Docker Compose.

## Requisitos del sistema

Antes de comenzar, asegurate de tener instalado lo siguiente:

- **Docker Desktop** version 24.0 o superior
- **Node.js** version 20 LTS o superior (para pruebas E2E)
- **.NET SDK** version 10.0 (opcional, solo para desarrollo del backend)
- **PowerShell** 7 o superior (para scripts auxiliares)
- **Git** para clonar el repositorio

## Paso 1: Clonar el repositorio

Abre una terminal y ejecuta:

```bash
git clone https://github.com/amillanaol/electron-cms-demo.git
cd electron-cms-demo
```

## Paso 2: Configurar variables de entorno

Copia el archivo de ejemplo y ajusta los valores si es necesario:

```bash
cp .env.example .env
```

Las variables principales incluyen:

| Variable | Descripcion | Valor por defecto |
| :--- | :--- | :--- |
| `POSTGRES_DB` | Nombre de la base de datos | KnowVaultCore |
| `POSTGRES_USER` | Usuario de PostgreSQL | KnowVaultCore |
| `POSTGRES_PASSWORD` | Contrasena del usuario | changeme |
| `ConnectionStrings__Default` | Cadena de conexion a la BD | Host=postgres;Database=KnowVaultCore... |

## Paso 3: Iniciar los servicios

Levanta todos los contenedores con un solo comando:

```bash
docker compose up -d
```

Esto iniciara tres servicios:

1. **postgres** — Base de datos PostgreSQL 16 con healthcheck automatico
2. **api** — Backend ASP.NET Core en .NET 10 con OpenAPI y migraciones automaticas
3. **frontend** — Servidor Nginx con proxy reverso y UI web

Puedes verificar el estado de cada contenedor con `docker compose ps`.

## Paso 4: Acceder a la aplicacion

Una vez que todos los servicios esten saludables, puedes acceder:

- **Frontend web:** [http://localhost:3000](http://localhost:3000)
- **API directa:** [http://localhost:8080](http://localhost:8080)
- **Documentacion OpenAPI:** [http://localhost:8080/openapi/v1.json](http://localhost:8080/openapi/v1.json)

## Verificacion de la instalacion

Para confirmar que todo funciona correctamente, ejecuta estas pruebas:

```bash
# Test de health
curl http://localhost:8080/health

# Test de ping
curl http://localhost:8080/api/ping

# Listar documentos
curl http://localhost:8080/api/content
```

Si recibes respuestas JSON sin errores, la instalacion se ha completado exitosamente.
",
                Status = DocumentStatus.Published,
                CreatedAt = now,
                UpdatedAt = now,
                PublishedAt = now,
                CurrentVersion = 1
            },
            new()
            {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000003"),
                Slug = "guia-markdown",
                Title = "Guia de referencia rapida de Markdown",
                Summary = "Referencia completa de la sintaxis Markdown soportada por KnowVaultCore.",
                MarkdownBody = @"# Guia de referencia rapida de Markdown

Markdown es un lenguaje de marcado ligero que permite escribir contenido formateado usando texto plano. KnowVaultCore utiliza **Markdig** como motor de renderizado, que soporta la sintaxis estandar de Markdown con algunas extensiones adicionales.

## Encabezados

Los encabezados se crean con el simbolo `#`. Se soportan hasta seis niveles:

```markdown
# Encabezado nivel 1
## Encabezado nivel 2
### Encabezado nivel 3
#### Encabezado nivel 4
##### Encabezado nivel 5
###### Encabezado nivel 6
```

## Formato de texto

Puedes aplicar diferentes estilos al texto:

- **Negrita:** `**texto**` o `__texto__` → **texto en negrita**
- *Cursiva:* `*texto*` o `_texto_` → *texto en cursiva*
- ~~Tachado~~: `~~texto~~` → ~~texto tachado~~
- `Codigo inline`: `` `codigo` `` → `codigo inline`
- `Combinado`: **texto con *cursiva* y `codigo`**

## Listas

### Listas no ordenadas

- Elemento uno
- Elemento dos
  - Subelemento anidado
  - Otro subelemento
- Elemento tres

### Listas ordenadas

1. Primer paso
2. Segundo paso
3. Tercer paso
   1. Subpaso indentado
   2. Otro subpaso

## Bloques de codigo

Para bloques de codigo multilinea, usa triple comilla invertida con el lenguaje:

```javascript
function saludo(nombre) {
    return `Hola, ${nombre}!`;
}

const mensaje = saludo('KnowVaultCore');
console.log(mensaje);
```

```python
def fibonacci(n):
    a, b = 0, 1
    for _ in range(n):
        yield a
        a, b = b, a + b

for num in fibonacci(10):
    print(num)
```

## Enlaces e imagenes

### Enlaces

Los enlaces se crean con la sintaxis `[texto](url)`:

- [Pagina principal de KnowVaultCore](http://localhost:3000)
- [Repositorio en GitHub](https://github.com/amillanaol/electron-cms-demo)
- [Documentacion de Markdig](https://github.com/xoofx/markdig)

### Imagenes

Para incluir imagenes, usa `![alt](url)`:

![Placeholder](https://placehold.co/800x200/2563eb/ffffff?text=KnowVaultCore+Banner)

Las imagenes se renderizan directamente en el contenido y pueden incluirse dentro de enlaces si es necesario.

## Citas y bloques

Las citas se crean con el simbolo `>`:

> KnowVaultCore transforma Markdown en HTML seguro.
>
> Puedes incluir multiples parrafos dentro de una cita.
> 
> > Incluso citas anidadas dentro de otras citas.

## Tablas

KnowVaultCore soporta tablas con sintaxis GFM (GitHub Flavored Markdown):

| Comando | Descripcion | Puerto |
| :--- | :--- | :--- |
| `docker compose up` | Inicia todos los servicios | 3000, 8080 |
| `docker compose down` | Detiene los servicios | — |
| `dotnet test` | Ejecuta tests unitarios | — |
| `npm test` | Ejecuta tests E2E | — |

## Lineas horizontales

Para separar secciones, usa tres o mas guiones:

---

## Consideraciones de seguridad

KnowVaultCore aplica automaticamente las siguientes reglas de sanitizacion al renderizar Markdown:

1. **Desactivacion de HTML**: Las etiquetas HTML como `<script>`, `<div>` o `onclick` se escapan y muestran como texto literal.
2. **Neutralizacion de esquemas peligrosos**: Los enlaces con `javascript:` o `vbscript:` se reemplazan por `about:blank`.
3. **Eliminacion de event handlers**: Atributos como `onclick`, `onerror` o `onload` se eliminan del HTML generado.

Estas reglas garantizan que el contenido renderizado sea seguro para visualizar en cualquier navegador o cliente desktop.
",
                Status = DocumentStatus.Published,
                CreatedAt = now,
                UpdatedAt = now,
                PublishedAt = now,
                CurrentVersion = 1
            },
            new()
            {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000004"),
                Slug = "documento-en-borrador",
                Title = "Documento en borrador",
                Summary = "Ejemplo de documento en estado Draft.",
                MarkdownBody = @"# Documento en borrador

Este documento se encuentra actualmente en estado **Borrador** y no es visible en las consultas de documentos publicados.

## Contenido pendiente

El siguiente contenido esta siendo revisado y se publicara proximamente:

- [ ] Seccion de introduccion
- [ ] Ejemplos de configuracion
- [ ] Capturas de pantalla
- [ ] Enlaces a recursos externos
- [ ] Notas de la version

## Cambios planificados

Estamos trabajando en las siguientes mejoras para la proxima version:

> 1. Integracion con editor visual
> 2. Soporte para arrastrar y soltar imagenes
> 3. Exportacion a PDF
> 4. Historial de revisiones
> 5. Colaboracion en tiempo real

Una vez que todo el contenido este listo, este documento se publicara y estara disponible para todos los usuarios del sistema.

```json
{
    ""status"": ""draft"",
    ""version"": ""0.2.0"",
    ""lastReview"": ""2026-06-04""
}
```
",
                Status = DocumentStatus.Draft,
                CreatedAt = now,
                UpdatedAt = now,
                PublishedAt = null,
                CurrentVersion = 1
            },
            new()
            {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000005"),
                Slug = "documento-archivado",
                Title = "Documento archivado",
                Summary = "Ejemplo de documento en estado Archived con contenido legado.",
                MarkdownBody = @"# Documento archivado

Este documento ha sido **archivado** y ya no aparece en los resultados de busqueda ni en la lista de documentos publicados. Se conserva unicamente con fines historicos.

## Historial del documento

Este documento fue creado durante la fase inicial del proyecto y contenia las primeras notas sobre la arquitectura del sistema. Con el tiempo, su contenido fue reemplazado por documentacion mas actualizada.

### Motivo del archivado

- El contenido fue migrado a la guia oficial de instalacion
- Las instrucciones ya no reflejan la version actual del sistema
- Se recomienda consultar los documentos activos para informacion vigente

## Nota para administradores

Los documentos archivados pueden restaurarse a traves de la API si es necesario:

```http
POST /api/content/{id}/publish
```

Esta accion cambia el estado de Archived a Published y el documento vuelve a estar disponible para los usuarios. Se recomienda revisar el contenido antes de republicarlo para asegurar que la informacion sigue siendo correcta.

---

*Documento archivado el 25 de mayo de 2026.*
*Ultima actualizacion: 30 de mayo de 2026.*
",
                Status = DocumentStatus.Archived,
                CreatedAt = now.AddDays(-10),
                UpdatedAt = now.AddDays(-5),
                PublishedAt = now.AddDays(-8),
                IsArchived = true,
                CurrentVersion = 1
            },
        };

        var versions = new List<ContentDocumentVersion>();
        var audits = new List<ContentDocumentAudit>();
        const string systemUser = "system";

        foreach (var doc in docs)
        {
            var rendered = renderer.Render(doc.MarkdownBody);
            doc.RenderedHtml = rendered;

            versions.Add(new ContentDocumentVersion
            {
                Id = Guid.NewGuid(),
                DocumentId = doc.Id,
                VersionNumber = 1,
                Title = doc.Title,
                MarkdownBody = doc.MarkdownBody,
                RenderedHtml = rendered,
                ChangeSummary = "Creación inicial",
                CreatedAt = doc.CreatedAt,
                CreatedBy = systemUser,
                IsCurrent = true
            });

            audits.Add(new ContentDocumentAudit
            {
                Id = Guid.NewGuid(),
                DocumentId = doc.Id,
                Action = AuditAction.Create,
                PerformedBy = systemUser,
                Timestamp = doc.CreatedAt
            });

            if (doc.Status == DocumentStatus.Published)
            {
                audits.Add(new ContentDocumentAudit
                {
                    Id = Guid.NewGuid(),
                    DocumentId = doc.Id,
                    Action = AuditAction.Publish,
                    PerformedBy = systemUser,
                    Timestamp = doc.PublishedAt ?? doc.CreatedAt
                });
            }

            if (doc.IsArchived)
            {
                audits.Add(new ContentDocumentAudit
                {
                    Id = Guid.NewGuid(),
                    DocumentId = doc.Id,
                    Action = AuditAction.Archive,
                    PerformedBy = systemUser,
                    Timestamp = doc.UpdatedAt
                });
            }
        }

        return (docs, versions, audits);
    }
}

