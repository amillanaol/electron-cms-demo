import { test, expect } from '@playwright/test';

test.describe('Fase 0 — Validacion del entorno', () => {

  test('Node.js runtime disponible', () => {
    expect(process.version).toBeDefined();
    expect(process.version.startsWith('v')).toBeTruthy();
  });

  test('Variables de proyecto definidas', () => {
    const pkg = require('../package.json');
    expect(pkg.name).toBe('KnowVault-Core');
    expect(pkg.scripts).toHaveProperty('test');
  });

  test('Playwright puede lanzar navegador', async ({ browser }) => {
    const context = await browser.newContext();
    const page = await context.newPage();
    await page.setContent('<html><body><h1>KnowVault-Core</h1></body></html>');
    const title = await page.locator('h1').textContent();
    expect(title).toBe('KnowVault-Core');
    await context.close();
  });

});

