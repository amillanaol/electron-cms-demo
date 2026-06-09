import { test, expect, _electron as electron } from '@playwright/test';
import { execSync } from 'child_process';
import path from 'path';

const API_BASE = process.env['KnowVault-Core_API_BASE_URL'] || 'http://localhost:8080';
const ELECTRON_APP = path.join(__dirname, '..', 'electron-app');

test.describe('Fase 6 — Cliente Electron MVP', () => {

  test('estructura de Electron tiene archivos requeridos', () => {
    const main = execSync(`type "${ELECTRON_APP}\\src\\main\\main.js"`, { encoding: 'utf-8' });
    expect(main).toContain('BrowserWindow');
    expect(main).toContain('contextIsolation: true');
    expect(main).toContain('nodeIntegration: false');

    const preload = execSync(`type "${ELECTRON_APP}\\src\\preload\\preload.js"`, { encoding: 'utf-8' });
    expect(preload).toContain('contextBridge.exposeInMainWorld');

    const html = execSync(`type "${ELECTRON_APP}\\src\\renderer\\index.html"`, { encoding: 'utf-8' });
    expect(html).toContain('KnowVault-Core Desktop');
  });

  test('config.js define apiBaseUrl', () => {
    const content = execSync(`type "${ELECTRON_APP}\\src\\shared\\config.js"`, { encoding: 'utf-8' });
    expect(content).toContain('apiBaseUrl');
    expect(content).toContain('localhost');
  });

  test('package.json define electron como dependencia', () => {
    const content = execSync(`type "${ELECTRON_APP}\\package.json"`, { encoding: 'utf-8' });
    expect(content).toContain('electron');
    expect(content).toContain('KnowVault-Core-desktop');
  });

  test('preload expone fetchPublished y fetchBySlug', () => {
    const content = execSync(`type "${ELECTRON_APP}\\src\\preload\\preload.js"`, { encoding: 'utf-8' });
    expect(content).toContain('fetchPublished');
    expect(content).toContain('fetchBySlug');
    expect(content).toContain('search');
  });

  test('preload bridge usa contextBridge', () => {
    const content = execSync(`type "${ELECTRON_APP}\\src\\preload\\preload.js"`, { encoding: 'utf-8' });
    expect(content).toContain('contextBridge');
    expect(content).toContain('exposeInMainWorld');
    expect(content).toContain('KnowVaultCore');
  });

});

