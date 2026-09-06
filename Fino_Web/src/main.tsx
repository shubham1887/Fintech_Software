import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import { loadRuntimeConfig } from '@/lib/runtimeConfig';

// Load the deployment config (API URL + AES key/IV) BEFORE importing App — App transitively
// pulls in apiClient/crypto, which read those values at module-eval time. Dynamically importing
// App only after the config resolves guarantees the runtime values are in place first.
loadRuntimeConfig().then(async () => {
  const { default: App } = await import('@/App');
  createRoot(document.getElementById('root')!).render(
    <StrictMode>
      <App />
    </StrictMode>,
  );
});
