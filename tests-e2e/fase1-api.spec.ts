import { test, expect } from '@playwright/test';

const API_BASE = process.env['KnowVault-Core_API_BASE_URL'] || 'http://localhost:8080';

test.describe('Fase 1 — Backend base', () => {

  test('GET /health responde 200 OK con status ok', async ({ request }) => {
    const res = await request.get(`${API_BASE}/health`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body).toHaveProperty('status', 'ok');
  });

  test('GET /api/ping responde 200 OK con pong', async ({ request }) => {
    const res = await request.get(`${API_BASE}/api/ping`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body).toHaveProperty('message', 'pong');
  });

  test('GET /openapi/v1.json retorna JSON valido de OpenAPI', async ({ request }) => {
    const res = await request.get(`${API_BASE}/openapi/v1.json`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body).toHaveProperty('openapi');
    expect(body).toHaveProperty('info');
    expect(body).toHaveProperty('paths');
  });

});

