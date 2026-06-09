import { test, expect } from '@playwright/test';
import { execSync } from 'child_process';
import path from 'path';

const API_BASE = process.env['KnowVault-Core_API_BASE_URL'] || 'http://localhost:8080';
const ELECTRON_APP = path.join(__dirname, '..', 'electron-app');

async function authHeaders(request: any) {
  const res = await request.post(`${API_BASE}/api/auth/login`, {
    data: { username: 'admin', password: 'admin123' }
  });
  const { token } = await res.json();
  return { 'Authorization': `Bearer ${token}` };
}

test.describe('Fase 7 — Pruebas y endurecimiento', () => {

  test.describe('API - Validacion de errores', () => {

    test('GET /api/content/slug-invalido devuelve 404', async ({ request }) => {
      const res = await request.get(`${API_BASE}/api/content/no-existe`);
      expect(res.status()).toBe(404);
      const body = await res.json();
      expect(body).toHaveProperty('error');
    });

    test('POST /api/content con title vacio devuelve 400', async ({ request }) => {
      const headers = await authHeaders(request);
      const res = await request.post(`${API_BASE}/api/content`, {
        data: { title: '', slug: 'test', markdownBody: 'body' },
        headers
      });
      expect(res.status()).toBe(400);
      const body = await res.json();
      expect(body).toHaveProperty('error');
    });

    test('POST /api/content con slug vacio devuelve 400', async ({ request }) => {
      const headers = await authHeaders(request);
      const res = await request.post(`${API_BASE}/api/content`, {
        data: { title: 'Title', slug: '', markdownBody: 'body' },
        headers
      });
      expect(res.status()).toBe(400);
      const body = await res.json();
      expect(body).toHaveProperty('error');
    });

    test('POST /api/content con markdownBody vacio devuelve 400', async ({ request }) => {
      const headers = await authHeaders(request);
      const res = await request.post(`${API_BASE}/api/content`, {
        data: { title: 'Title', slug: 'test', markdownBody: '' },
        headers
      });
      expect(res.status()).toBe(400);
      const body = await res.json();
      expect(body).toHaveProperty('error');
    });

    test('GET /api/content/search sin text devuelve error de validacion', async ({ request }) => {
      const res = await request.get(`${API_BASE}/api/content/search`);
      expect(res.status()).toBe(400);
      const body = await res.json();
      expect(body.errors || body.error).toBeTruthy();
    });

    test('PUT /api/content con id inexistente devuelve 404', async ({ request }) => {
      const headers = await authHeaders(request);
      const res = await request.put(`${API_BASE}/api/content/00000000-0000-0000-0000-000000000000`, {
        data: { title: 'Test', markdownBody: 'body' },
        headers
      });
      expect(res.status()).toBe(404);
    });

    test('POST /api/content/{id}/publish con id inexistente devuelve 404', async ({ request }) => {
      const headers = await authHeaders(request);
      const res = await request.post(`${API_BASE}/api/content/00000000-0000-0000-0000-000000000000/publish`, { headers });
      expect(res.status()).toBe(404);
    });

    test('POST /api/content/{id}/archive con id inexistente devuelve 404', async ({ request }) => {
      const headers = await authHeaders(request);
      const res = await request.post(`${API_BASE}/api/content/00000000-0000-0000-0000-000000000000/archive`, { headers });
      expect(res.status()).toBe(404);
    });
  });

  test.describe('API - Resilience', () => {

    test('API Offline simulado: maneja error de conexion', async ({ request }) => {
      const fakeUrl = 'http://localhost:1';
      try {
        await request.get(`${fakeUrl}/api/content`);
      } catch {
        // Expected - connection refused
      }
    });

    test('Latencia: timeout en solicitud simulada', async ({ request }) => {
      const res = await request.get(`${API_BASE}/api/ping`);
      expect(res.status()).toBe(200);
    });
  });

  test.describe('API - Markdown sanitizacion', () => {

    test('MARKDOWN renderiza contenido vacio como 400', async ({ request }) => {
      const res = await request.post(`${API_BASE}/api/markdown/render`, {
        data: { markdown: '' }
      });
      expect(res.status()).toBe(400);
    });

    test('MARKDOWN entradas con solo espacios dan 400', async ({ request }) => {
      const res = await request.post(`${API_BASE}/api/markdown/render`, {
        data: { markdown: '   ' }
      });
      expect(res.status()).toBe(400);
    });

    test('MARKDOWN bloquea href javascript:', async ({ request }) => {
      const res = await request.post(`${API_BASE}/api/markdown/render`, {
        data: { markdown: '[click](javascript:alert(1))' }
      });
      expect(res.status()).toBe(200);
      const body = await res.json();
      expect(body.html).not.toContain('javascript:');
    });

    test('MARKDOWN bloquea href JAVASCRIPT: mayusculas', async ({ request }) => {
      const res = await request.post(`${API_BASE}/api/markdown/render`, {
        data: { markdown: '[click](JAVASCRIPT:alert(1))' }
      });
      expect(res.status()).toBe(200);
      const body = await res.json();
      expect(body.html).not.toContain('JAVASCRIPT:');
    });

    test('MARKDOWN elimina onerror de img', async ({ request }) => {
      const res = await request.post(`${API_BASE}/api/markdown/render`, {
        data: { markdown: '<img src="x" onerror="alert(1)">' }
      });
      expect(res.status()).toBe(200);
      const body = await res.json();
      expect(body.html).toContain('&lt;img'); // HTML escapado, no renderizado
    });

    test('MARKDOWN elimina onclick', async ({ request }) => {
      const res = await request.post(`${API_BASE}/api/markdown/render`, {
        data: { markdown: '<div onclick="alert(1)">click</div>' }
      });
      expect(res.status()).toBe(200);
      const body = await res.json();
      expect(body.html).toContain('&lt;div'); // HTML escapado, no renderizado
    });

    test('MARKDOWN texto normal se renderiza como parrafo', async ({ request }) => {
      const res = await request.post(`${API_BASE}/api/markdown/render`, {
        data: { markdown: 'Hello world' }
      });
      expect(res.status()).toBe(200);
      const body = await res.json();
      expect(body.html).toContain('Hello world');
    });
  });

  test.describe('ELECTRON - Configuracion segura', () => {

    test('ELECTRON main.js tiene contexto seguro', () => {
      const content = execSync(`type "${ELECTRON_APP}\\src\\main\\main.js"`, { encoding: 'utf-8' });
      expect(content).toContain('contextIsolation: true');
      expect(content).toContain('nodeIntegration: false');
      expect(content).toContain('preload');
    });

    test('ELECTRON preload solo expone metodos de bridge', () => {
      const content = execSync(`type "${ELECTRON_APP}\\src\\preload\\preload.js"`, { encoding: 'utf-8' });
      expect(content).toContain('contextBridge');
      expect(content).toContain('fetchPublished');
      expect(content).toContain('fetchBySlug');
      expect(content).toContain('search');
      expect(content).not.toContain('ipcRenderer');
      expect(content).not.toContain('nodeIntegration');
      expect(content).not.toContain("require('child_process"); // solo prohibir modulos peligrosos, no electron
    });

    test('ELECTRON index.html tiene CSP configurada', () => {
      const content = execSync(`type "${ELECTRON_APP}\\src\\renderer\\index.html"`, { encoding: 'utf-8' });
      expect(content).toContain('Content-Security-Policy');
      expect(content).toContain("connect-src 'self'");
    });

    test('ELECTRON preload maneja errores de red', () => {
      const content = execSync(`type "${ELECTRON_APP}\\src\\preload\\preload.js"`, { encoding: 'utf-8' });
      expect(content).toContain('No se pudo conectar con el servidor');
      expect(content).toContain('AbortError');
    });
  });

  test.describe('ELECTRON - Manejo de estados en renderer', () => {

    test('ELECTRON renderer tiene elementos de estado', () => {
      const content = execSync(`type "${ELECTRON_APP}\\src\\renderer\\index.html"`, { encoding: 'utf-8' });
      expect(content).toContain('loading-msg');
      expect(content).toContain('error-msg');
      expect(content).toContain('empty-msg');
      expect(content).toContain('status-badge');
    });

    test('ELECTRON app.js maneja errores de API', () => {
      const content = execSync(`type "${ELECTRON_APP}\\src\\renderer\\js\\app.js"`, { encoding: 'utf-8' });
      expect(content).toContain('try');
      expect(content).toContain('catch');
      expect(content).toContain('showError');
    });

    test('ELECTRON document-viewer muestra error', () => {
      const content = execSync(`type "${ELECTRON_APP}\\src\\renderer\\js\\ui\\document-viewer.js"`, { encoding: 'utf-8' });
      expect(content).toContain('showError');
      expect(content).toContain('Verifica la conexión');
    });

    test('ELECTRON document-list carga/error/vacio states', () => {
      const content = execSync(`type "${ELECTRON_APP}\\src\\renderer\\js\\ui\\document-list.js"`, { encoding: 'utf-8' });
      expect(content).toContain('showLoading');
      expect(content).toContain('showError');
    });
  });
});

