import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './tests-e2e',
  reporter: 'list',
  use: {
    baseURL: process.env['KnowVault-Core_API_BASE_URL'] || 'http://localhost:8080',
  },
  projects: [
    {
      name: 'api',
      testMatch: '**/*.spec.ts',
    },
  ],
});

