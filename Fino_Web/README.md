# Fino Web

Fresh, original front-end for the **Fino_API** (.NET 9) fintech platform. Built from scratch
with React + TypeScript + Vite + Tailwind v4 — **not** derived from the legacy `UI/` (CRA + MUI)
app. The legacy app was used only to understand the API contract.

## Stack

- **Vite 8** + **React 19** + **TypeScript**
- **Tailwind CSS v4** (CSS-first theme tokens in `src/index.css`)
- **React Router 7**, **TanStack Query 5**, **react-hook-form + zod**
- **axios**, **crypto-js** (AES-256 matching the backend), **recharts**, **lucide-react**

## Run

```bash
npm install
npm run dev      # http://localhost:3000
npm run build    # type-check (tsc) + production build
```

The dev server runs on **:3000** (allow-listed by Fino_API CORS). It proxies `/api` to the
backend so there is no CORS or self-signed-cert friction — start Fino_API on
`https://localhost:7080` (or set `VITE_PROXY_TARGET`). See `.env.example`.

## API integration (the important part)

Every call mirrors the Fino_API envelope exactly — see `src/lib/`:

- **`crypto.ts`** — AES-256-CBC/PKCS7, key `Talk2m@FintechApp@SecretKey32Chr`, IV `FintechApp@16Byt`
  (UTF-8 → Base64), matching `Helpers/EncryptDecrypt.cs`.
- **`apiClient.ts`** — posts `multipart/form-data` with two encrypted fields:
  - `Request` = `enc(JSON.stringify(payload))`
  - `KeyData` = `enc({ UserID, UserTypeID, SessionToken, DeviceType:"Web", IPAddress })`
  - header `Authorization: Bearer <AccessToken>`.
  Reads the `{ StatusCode, Status, Message, Data }` envelope and **decrypts `Data`** (falls back
  to plain JSON for the few endpoints that don't encrypt). Handles `401 → refresh → retry`, and
  on hard expiry clears the session and signals `AuthProvider`.

The backend cross-checks that `KeyData.UserID/UserTypeID` match the signed JWT, so the client
echoes the real identity (never spoofable from the UI).

## What's wired

| Screen | Endpoint |
| --- | --- |
| Login | `POST api/Auth/login` |
| Sidebar (role-based) | `POST api/SidebarMenu/Select` |
| Dashboard | `POST api/Dashboard/{Select,SelectForRetailer,SelectForDistributor}` |
| Recharge | `POST api/TransactionProcessing/Recharge` |
| Transactions | `POST api/TransactionReport/SelectReport` |

The sidebar renders the **real role-filtered menu** from the backend. Menu items without a built
screen yet route to a placeholder that names the pending screen — so the full menu is navigable.

## White-label (per-client branding)

One codebase, many branded deployments. Branding is driven by `public/config.json`:

```json
{
  "appName": "Acme Pay",
  "tagline": "Portal",
  "logoUrl": "/brand/acme.png",
  "theme": "emerald",
  "loginHeadline": "...",
  "loginSubtext": "..."
}
```

- `theme` picks a preset from `src/theme/themes.ts` (azure, emerald, violet, sunset, rose, ocean). The
  ramp is injected as runtime CSS variables (`--color-brand-*`), so the whole app re-themes instantly.
- To add a client theme, add a preset to `themes.ts`. To rebrand a deployment, ship a different
  `config.json` (+ logo) — no code changes.
- Users can switch themes live from the Topbar palette or the **Appearance** page (saved per device;
  the `config.json` theme is the default).

## Adding a screen

1. Add an API module in `src/api/<feature>.api.ts` using `apiPost<T>(endpoint, payload)`.
2. Build the page in `src/pages/` with the `ui/` primitives (`Card`, `DataTable`, `Input`, …).
3. Add a route in `src/App.tsx`, and map the menu keyword in
   `src/components/layout/resolveRoute.ts`.

## Structure

```
src/
  api/         endpoint modules (auth, menu, dashboard, recharge, transactionReport)
  components/
    ui/        design-system primitives (Button, Card, Input, DataTable, Toast, …)
    layout/    AppShell, Sidebar (role menu), Topbar, icon mapping
  features/auth/  AuthContext (session, login/logout, expiry)
  lib/         crypto, apiClient, storage, format, roles, cn, queryClient
  pages/       screens
  types/       backend-mirroring DTOs
```
