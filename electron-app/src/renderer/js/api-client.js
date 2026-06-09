const bridge = window.KnowVaultCore;

const api = {
  async login(username, password) {
    return bridge.login(username, password);
  },

  getToken() {
    return bridge.getToken();
  },

  async getPublished() {
    return bridge.fetchPublished();
  },

  async getBySlug(slug) {
    return bridge.fetchBySlug(slug);
  },

  async search(text) {
    return bridge.search(text);
  },

  async getVersions(id) {
    return bridge.fetchVersions(id);
  },

  async getAudit(id) {
    return bridge.fetchAudit(id);
  },

  async getDeleted() {
    return bridge.fetchDeleted();
  },

  async restoreDocument(id, versionNumber) {
    return bridge.restoreDocument(id, versionNumber);
  },

  async deleteDocument(id) {
    return bridge.deleteDocument(id);
  }
};
