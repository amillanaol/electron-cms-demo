import { test, expect } from '@playwright/test';

const API_BASE = process.env['KnowVault-Core_API_BASE_URL'] || 'http://localhost:8080';

test.describe('Fase 3 — Pipeline Markdown seguro', () => {

  test('POST /api/markdown/render convierte Markdown basico a HTML', async ({ request }) => {
    const res = await request.post(`${API_BASE}/api/markdown/render`, {
      data: { markdown: '# Hola\n\nEsto es **negrita**.' }
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body).toHaveProperty('html');
    expect(body.html).toContain('<h1>Hola</h1>');
    expect(body.html).toContain('<strong>negrita</strong>');
  });

  test('POST /api/markdown/render bloquea inyeccion XSS', async ({ request }) => {
    const res = await request.post(`${API_BASE}/api/markdown/render`, {
      data: { markdown: '<script>alert("xss")</script>' }
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.html).not.toContain('<script>');
    expect(body.html).not.toContain('<Script>');
  });

  test('POST /api/markdown/render neutraliza enlaces javascript:', async ({ request }) => {
    const res = await request.post(`${API_BASE}/api/markdown/render`, {
      data: { markdown: '[Click](javascript:void(0))' }
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.html).not.toContain('javascript:');
  });

});

