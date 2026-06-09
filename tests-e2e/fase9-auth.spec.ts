import { test, expect } from '@playwright/test';

const API_BASE = process.env['KnowVault-Core_API_BASE_URL'] || 'http://localhost:8080';

test.describe('Fase 9 — Autenticacion JWT', () => {

  test('POST /api/auth/login con credenciales validas retorna token', async ({ request }) => {
    const res = await request.post(`${API_BASE}/api/auth/login`, {
      data: { username: 'admin', password: 'admin123' }
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body).toHaveProperty('token');
    expect(body).toHaveProperty('username', 'admin');
    expect(body).toHaveProperty('role', 'admin');
  });

  test('POST /api/auth/login con credenciales invalidas da 401', async ({ request }) => {
    const res = await request.post(`${API_BASE}/api/auth/login`, {
      data: { username: 'admin', password: 'wrong' }
    });
    expect(res.status()).toBe(401);
  });

  test('POST /api/content sin token da 403', async ({ request }) => {
    const res = await request.post(`${API_BASE}/api/content`, {
      data: { title: 'No Auth', slug: `no-auth-${Date.now()}`, summary: 'test', markdownBody: '# test' }
    });
    expect(res.status()).toBe(403);
  });

  test('Admin puede crear contenido', async ({ request }) => {
    const login = await request.post(`${API_BASE}/api/auth/login`, {
      data: { username: 'admin', password: 'admin123' }
    });
    const { token } = await login.json();
    const headers = { 'Authorization': `Bearer ${token}` };

    const res = await request.post(`${API_BASE}/api/content`, {
      data: { title: 'Admin Create', slug: `admin-${Date.now()}`, summary: 'test', markdownBody: '# admin' },
      headers
    });
    expect(res.status()).toBe(201);
  });

  test('Editor puede crear contenido', async ({ request }) => {
    const login = await request.post(`${API_BASE}/api/auth/login`, {
      data: { username: 'editor', password: 'editor123' }
    });
    const { token } = await login.json();
    const headers = { 'Authorization': `Bearer ${token}` };

    const res = await request.post(`${API_BASE}/api/content`, {
      data: { title: 'Editor Create', slug: `editor-${Date.now()}`, summary: 'test', markdownBody: '# editor' },
      headers
    });
    expect(res.status()).toBe(201);
  });

  test('Viewer no puede crear contenido (403)', async ({ request }) => {
    const login = await request.post(`${API_BASE}/api/auth/login`, {
      data: { username: 'viewer', password: 'viewer321' }
    });
    const { token } = await login.json();
    const headers = { 'Authorization': `Bearer ${token}` };

    const res = await request.post(`${API_BASE}/api/content`, {
      data: { title: 'Viewer Create', slug: `viewer-${Date.now()}`, summary: 'test', markdownBody: '# viewer' },
      headers
    });
    expect(res.status()).toBe(403);
  });

  test('Editor no puede eliminar contenido (403)', async ({ request }) => {
    // Admin creates
    const loginAdmin = await request.post(`${API_BASE}/api/auth/login`, {
      data: { username: 'admin', password: 'admin123' }
    });
    const { token: adminToken } = await loginAdmin.json();
    const adminHeaders = { 'Authorization': `Bearer ${adminToken}` };

    const create = await request.post(`${API_BASE}/api/content`, {
      data: { title: 'To Delete', slug: `to-del-${Date.now()}`, summary: 'test', markdownBody: '# delete' },
      headers: adminHeaders
    });
    const doc = await create.json();

    // Editor tries to delete
    const loginEditor = await request.post(`${API_BASE}/api/auth/login`, {
      data: { username: 'editor', password: 'editor123' }
    });
    const { token: editorToken } = await loginEditor.json();
    const delRes = await request.delete(`${API_BASE}/api/content/${doc.id}`, {
      headers: { 'Authorization': `Bearer ${editorToken}` }
    });
    expect(delRes.status()).toBe(403);
  });

});
