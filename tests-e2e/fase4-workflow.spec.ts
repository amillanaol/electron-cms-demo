import { test, expect } from '@playwright/test';

const API_BASE = process.env['KnowVault-Core_API_BASE_URL'] || 'http://localhost:8080';

async function authHeaders(request: any, role = 'admin') {
  const pw = { admin: 'admin123', editor: 'editor123', viewer: 'viewer321' }[role] || 'admin123';
  const res = await request.post(`${API_BASE}/api/auth/login`, {
    data: { username: role, password: pw }
  });
  const { token } = await res.json();
  return { 'Authorization': `Bearer ${token}` };
}

test.describe('Fase 4 — Endpoints de consulta y publicacion', () => {

  test('GET /api/content lista solo documentos publicados', async ({ request }) => {
    const res = await request.get(`${API_BASE}/api/content`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(Array.isArray(body)).toBe(true);
    for (const doc of body) {
      expect(doc.status).toBe('Published');
    }
  });

  test('GET /api/content/{slug} devuelve detalle de documento', async ({ request }) => {
    const res = await request.get(`${API_BASE}/api/content/bienvenida`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body).toHaveProperty('slug', 'bienvenida');
    expect(body).toHaveProperty('title');
    expect(body).toHaveProperty('renderedHtml');
    expect(body.renderedHtml).toContain('<h1>');
  });

  test('GET /api/content/search encuentra documentos por texto', async ({ request }) => {
    const res = await request.get(`${API_BASE}/api/content/search?text=instalacion`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(Array.isArray(body)).toBe(true);
    expect(body.length).toBeGreaterThanOrEqual(1);
    expect(body.some((d: any) => d.slug === 'guia-instalacion-local')).toBe(true);
  });

  test('POST /api/content crea documento en Draft', async ({ request }) => {
    const slug = `test-${Date.now()}`;
    const headers = await authHeaders(request);
    const res = await request.post(`${API_BASE}/api/content`, {
      data: {
        title: 'Test Document',
        slug,
        summary: 'Created by test',
        markdownBody: '# Test\n\nContent.'
      },
      headers
    });
    expect(res.status()).toBe(201);
    const body = await res.json();
    expect(body).toHaveProperty('slug', slug);
    expect(body).toHaveProperty('status', 'Draft');
  });

  test('POST /api/content/publish cambia estado a Published', async ({ request }) => {
    const slug = `pub-test-${Date.now()}`;
    const headers = await authHeaders(request);
    const create = await request.post(`${API_BASE}/api/content`, {
      data: {
        title: 'Publish Test',
        slug,
        summary: 'To be published',
        markdownBody: '# Publish\n\nTest.'
      },
      headers
    });
    expect(create.status()).toBe(201);
    const created = await create.json();

    const pubRes = await request.post(`${API_BASE}/api/content/${created.id}/publish`, { headers });
    expect(pubRes.status()).toBe(200);
    const published = await pubRes.json();
    expect(published).toHaveProperty('status', 'Published');

    const listRes = await request.get(`${API_BASE}/api/content`);
    const list = await listRes.json();
    expect(list.some((d: any) => d.slug === slug)).toBe(true);
  });

  test('POST /api/content/{id}/archive oculta del listado publico', async ({ request }) => {
    const slug = `arch-test-${Date.now()}`;
    const headers = await authHeaders(request);
    const create = await request.post(`${API_BASE}/api/content`, {
      data: {
        title: 'Archive Test',
        slug,
        summary: 'To be archived',
        markdownBody: '# Archive\n\nTest.'
      },
      headers
    });
    const created = await create.json();

    await request.post(`${API_BASE}/api/content/${created.id}/publish`, { headers });
    await request.post(`${API_BASE}/api/content/${created.id}/archive`, { headers });

    const listRes = await request.get(`${API_BASE}/api/content`);
    const list = await listRes.json();
    expect(list.some((d: any) => d.slug === slug)).toBe(false);
  });

});
