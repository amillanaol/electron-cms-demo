const { contextBridge } = require('electron');

const API_BASE = process.env['KnowVault-Core_API_BASE_URL'] || 'http://localhost:8080';
const TIMEOUT_MS = 10000;

let authToken = '';

async function apiFetch(url, options = {}) {
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), TIMEOUT_MS);

  const headers = { ...options.headers };
  if (authToken) {
    headers['Authorization'] = 'Bearer ' + authToken;
  }

  try {
    const res = await fetch(url, { ...options, headers, signal: controller.signal });
    if (!res.ok) {
      if (res.status === 404) return null;
      throw new Error(`API error: ${res.status}`);
    }
    return res.json();
  } catch (err) {
    if (err.name === 'AbortError') {
      throw new Error('La solicitud excedió el tiempo de espera. Verifica la conexión con el servidor.');
    }
    if (err instanceof TypeError && err.message.includes('fetch')) {
      throw new Error('No se pudo conectar con el servidor. Verifica que la API esté disponible.');
    }
    throw err;
  } finally {
    clearTimeout(timer);
  }
}

contextBridge.exposeInMainWorld('KnowVaultCore', {
  apiBaseUrl: API_BASE,

  async login(username, password) {
    const formData = { username, password };
    const res = await fetch(`${API_BASE}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(formData),
    });
    if (!res.ok) throw new Error('Credenciales inválidas');
    const data = await res.json();
    authToken = data.token;
    return data;
  },

  getToken() {
    return authToken;
  },

  setToken(token) {
    authToken = token;
  },

  async fetchPublished() {
    return apiFetch(`${API_BASE}/api/content`);
  },

  async fetchBySlug(slug) {
    return apiFetch(`${API_BASE}/api/content/${encodeURIComponent(slug)}`);
  },

  async search(text) {
    return apiFetch(`${API_BASE}/api/content/search?text=${encodeURIComponent(text)}`);
  },

  async fetchVersions(id) {
    return apiFetch(`${API_BASE}/api/content/${id}/versions`);
  },

  async fetchAudit(id) {
    return apiFetch(`${API_BASE}/api/content/${id}/audit`);
  },

  async fetchDeleted() {
    return apiFetch(`${API_BASE}/api/content/deleted`);
  },

  async restoreDocument(id, versionNumber) {
    const body = versionNumber ? { versionNumber } : {};
    return apiFetch(`${API_BASE}/api/content/${id}/restore`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body)
    });
  },

  async deleteDocument(id) {
    return apiFetch(`${API_BASE}/api/content/${id}`, { method: 'DELETE' });
  }
});
