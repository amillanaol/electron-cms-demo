const config = {
  apiBaseUrl: process.env.KnowVault-Core_API_BASE_URL || 'http://localhost:8080',
  appName: 'KnowVault-Core Desktop',
};

if (typeof module !== 'undefined' && module.exports) {
  module.exports = { config };
}

