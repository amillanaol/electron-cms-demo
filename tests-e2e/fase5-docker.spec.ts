import { test, expect } from '@playwright/test';
import { execSync } from 'child_process';

const API_BASE = process.env['KnowVault-Core_API_BASE_URL'] || 'http://localhost:8080';

test.describe('Fase 5 — Docker Compose y entorno local', () => {

  test('GET /health responde correctamente', async ({ request }) => {
    const res = await request.get(`${API_BASE}/health`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body).toHaveProperty('status', 'ok');
  });

  test('GET /api/db/status confirma conexion a PostgreSQL', async ({ request }) => {
    const res = await request.get(`${API_BASE}/api/db/status`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body).toHaveProperty('database', 'connected');
  });

  test('docker-compose.yml define servicios api y postgres', () => {
    const content = execSync('type docker\\docker-compose.yml', { encoding: 'utf-8' });
    expect(content).toContain('api:');
    expect(content).toContain('postgres:');
    expect(content).toContain('volumes:');
    expect(content).toContain('pgdata:');
    expect(content).toContain('networks:');
    expect(content).toContain('KnowVault-Core-net:');
  });

  test('Dockerfile existe y tiene multi-stage build', () => {
    const content = execSync('type backend\\src\\KnowVault-Core.Api\\Dockerfile', { encoding: 'utf-8' });
    expect(content).toContain('FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build');
    expect(content).toContain('FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime');
    expect(content).toContain('ENTRYPOINT [');
    expect(content).toContain('KnowVault-Core.Api.dll');
  });

  test('.env.example tiene las variables requeridas', () => {
    const content = execSync('type .env.example', { encoding: 'utf-8' });
    expect(content).toContain('ASPNETCORE_ENVIRONMENT');
    expect(content).toContain('POSTGRES_DB');
    expect(content).toContain('POSTGRES_USER');
    expect(content).toContain('POSTGRES_PASSWORD');
    expect(content).toContain('ConnectionStrings__Default');
    expect(content).toContain('KnowVault-Core_API_BASE_URL');
  });

});

