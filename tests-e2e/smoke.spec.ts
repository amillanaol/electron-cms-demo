import { test, expect } from '@playwright/test';
import path from 'path';
import fs from 'fs';

const ELECTRON_APP = path.join(__dirname, '..', 'electron-app');
const DIST_DIR = path.join(ELECTRON_APP, 'dist');

test.describe('Fase 8 — Empaquetado Windows', () => {

  test('BUILD electron-builder.yml existe con config valida', () => {
    const content = fs.readFileSync(path.join(ELECTRON_APP, 'electron-builder.yml'), 'utf-8');
    expect(content).toContain('appId: com.KnowVault-Core.desktop');
    expect(content).toContain('productName: KnowVault-Core Desktop');
    expect(content).toContain('nsis');
    expect(content).toContain('x64');
  });

  test('BUILD package.json tiene scripts de empaquetado', () => {
    const content = fs.readFileSync(path.join(ELECTRON_APP, 'package.json'), 'utf-8');
    expect(content).toContain('"pack"');
    expect(content).toContain('"dist"');
    expect(content).toContain('electron-builder');
  });

  test('BUILD dist/win-unpacked contiene el ejecutable', () => {
    const exePath = path.join(DIST_DIR, 'win-unpacked', 'KnowVault-Core Desktop.exe');
    expect(fs.existsSync(exePath)).toBe(true);
    const stats = fs.statSync(exePath);
    expect(stats.size).toBeGreaterThan(1024 * 1024); // at least 1MB
  });

  test('BUILD dist/win-unpacked tiene recursos empaquetados', () => {
    const resourcesPath = path.join(DIST_DIR, 'win-unpacked', 'resources');
    expect(fs.existsSync(resourcesPath)).toBe(true);
    const items = fs.readdirSync(resourcesPath);
    expect(items.length).toBeGreaterThan(0);
  });

  test('BUILD dist/win-unpacked tiene app.asar', () => {
    const asarPath = path.join(DIST_DIR, 'win-unpacked', 'resources', 'app.asar');
    expect(fs.existsSync(asarPath)).toBe(true);
  });

});

