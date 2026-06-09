import { test, expect } from '@playwright/test';

const API_BASE = process.env['KnowVault-Core_API_BASE_URL'] || 'http://localhost:8080';

test.describe('Fase 2 — Persistencia y modelo editorial', () => {

  test('GET /api/db/status confirma conexion a PostgreSQL', async ({ request }) => {
    const res = await request.get(`${API_BASE}/api/db/status`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body).toHaveProperty('database', 'connected');
    expect(body).toHaveProperty('documentCount');
    expect(body.documentCount).toBeGreaterThanOrEqual(1);
  });

  test('GET /health sigue funcionando tras agregar DB', async ({ request }) => {
    const res = await request.get(`${API_BASE}/health`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body).toHaveProperty('status', 'ok');
  });

  test('GET /openapi/v1.json incluye nuevo endpoint /api/db/status', async ({ request }) => {
    const res = await request.get(`${API_BASE}/openapi/v1.json`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    const paths = Object.keys(body.paths || {});
    expect(paths).toContain('/api/db/status');
  });

});

