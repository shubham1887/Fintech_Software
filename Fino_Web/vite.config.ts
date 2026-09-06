import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import { fileURLToPath, URL } from 'node:url';

// Backend (Fino_API) dev origin. Default matches the "http" launch profile
// (what `dotnet run` uses). For the "https" profile use https://localhost:7080.
const API_TARGET = process.env.VITE_PROXY_TARGET ?? 'http://localhost:5266/' //'https://api.ripay.co.in/'//'http://localhost:5266';

// Dev server runs on :3000 (allowed by Fino_API CORS). In dev we proxy /api to the
// backend with secure:false so the browser never sees the self-signed cert and there
// is no cross-origin preflight. In production, set VITE_API_URL to the real API base.
export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: 3000,
    host: true,
    proxy: {
      '/api': {
        target: API_TARGET,
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
