function getToken() {
  return localStorage.getItem('knowvault-token');
}

function setToken(token) {
  if (token) localStorage.setItem('knowvault-token', token);
  else localStorage.removeItem('knowvault-token');
}

function getRole() {
  return localStorage.getItem('knowvault-role') || '';
}

function setRole(role) {
  if (role) localStorage.setItem('knowvault-role', role);
  else localStorage.removeItem('knowvault-role');
}

function getUserName() {
  return localStorage.getItem('knowvault-username') || '';
}

function setUserName(name) {
  if (name) localStorage.setItem('knowvault-username', name);
  else localStorage.removeItem('knowvault-username');
}

function authHeaders() {
  const token = getToken();
  return token ? { 'Authorization': 'Bearer ' + token } : {};
}

const api = {
  async login(username, password) {
    const res = await fetch('/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password }),
    });
    if (!res.ok) throw new Error('Credenciales inválidas');
    const data = await res.json();
    setToken(data.token);
    setUserName(data.username);
    setRole(data.role);
    return data;
  },

  logout() {
    setToken(null);
    setUserName(null);
    setRole(null);
  },

  async fetchJson(url, options = {}) {
    const res = await fetch(url, {
      ...options,
      headers: { ...authHeaders(), ...options.headers },
    });
    if (!res.ok) {
      if (res.status === 404) return null;
      if (res.status === 403) throw new Error('No tienes permisos para esta operación');
      throw new Error(`API error: ${res.status}`);
    }
    return res.json();
  },

  async getPublished() {
    return this.fetchJson('/api/content');
  },

  async getBySlug(slug) {
    return this.fetchJson(`/api/content/${encodeURIComponent(slug)}`);
  },

  async search(text) {
    return this.fetchJson(`/api/content/search?text=${encodeURIComponent(text)}`);
  },

  async updateContent(id, data) {
    return this.fetchJson(`/api/content/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
  },
};
