import axios, { type AxiosError } from 'axios';
import { encrypt, decrypt } from './crypto';
import { clearSession, loadSession, patchTokens } from './storage';
import { getConfig, cfgValue } from './runtimeConfig';
import { ApiError, STATUS, type ApiResult } from '@/types/api';
import type { Tokens } from '@/types/auth';

/**
 * API base URL. Resolved at run time so a deployed build can be retargeted without a rebuild:
 *   config.json "apiUrl"  →  build-time VITE_API_URL  →  same-origin '/api/'.
 * (loadRuntimeConfig() runs in main.tsx before this module is imported.)
 */
const BASE_URL = (cfgValue(getConfig().apiUrl) ?? import.meta.env.VITE_API_URL ?? '/api/').replace(/\/?$/, '/');

export const SESSION_EXPIRED_EVENT = 'fino:session-expired';

const http = axios.create({ baseURL: BASE_URL, timeout: 120_000 });

/**
 * Case-insensitive field read. The API envelope is serialised camelCase
 * (status/data/message) while the encrypted Data payload is PascalCase — this reads
 * the envelope correctly regardless of casing.
 */
function field(obj: unknown, name: string): unknown {
  if (!obj || typeof obj !== 'object') return undefined;
  const rec = obj as Record<string, unknown>;
  if (name in rec) return rec[name];
  const lower = name.toLowerCase();
  const key = Object.keys(rec).find((k) => k.toLowerCase() === lower);
  return key === undefined ? undefined : rec[key];
}

function authHeader(): Record<string, string> {
  const token = loadSession()?.tokens?.AccessToken;
  return token ? { Authorization: `Bearer ${token}` } : {};
}

/** KeyData echoes the signed identity; the backend cross-checks it against the JWT. */
function buildKeyData(): string {
  const s = loadSession();
  if (!s) return '';
  return encrypt(
    JSON.stringify({
      UserID: s.user.UserID,
      UserTypeID: s.user.UserTypeID,
      SessionToken: s.tokens.AccessToken,
      DeviceType: 'Web',
      IPAddress: '',
    }),
  );
}

/** The Data string is AES-encrypted on most endpoints; fall back to plain JSON otherwise. */
function unwrap<T>(dataStr: string): T | null {
  if (!dataStr) return null;
  const text = decrypt(dataStr) || dataStr;
  try {
    return JSON.parse(text) as T;
  } catch {
    return text as unknown as T;
  }
}

interface PostOptions {
  /** Attach KeyData + Bearer token (default true). Use false for login/refresh. */
  auth?: boolean;
  _retried?: boolean;
}

export async function apiPost<T = unknown>(
  endpoint: string,
  payload?: unknown,
  options: PostOptions = {},
): Promise<ApiResult<T>> {
  const { auth = true } = options;

  const form = new FormData();
  form.append('Request', encrypt(JSON.stringify(payload ?? {})));
  // KeyData must be present + non-empty (the API marks it required). For anonymous
  // calls (login/refresh) the value is ignored server-side, so send an encrypted stub.
  form.append('KeyData', auth ? buildKeyData() : encrypt('{}'));

  try {
    const res = await http.post(endpoint.replace(/^\//, ''), form, {
      headers: auth ? authHeader() : undefined,
    });
    const env = res.data;
    const status = String(field(env, 'status') ?? '');
    const message = String(field(env, 'message') ?? '');
    const dataStr = (field(env, 'data') as string) ?? '';
    const ok = status.toUpperCase() === STATUS.SUCCESS;
    return { ok, status, message, data: ok ? unwrap<T>(dataStr) : null };
  } catch (err) {
    const ax = err as AxiosError;
    const httpStatus = ax.response?.status ?? 0;

    if (httpStatus === 401 && auth && !options._retried) {
      const refreshed = await tryRefresh();
      if (refreshed) return apiPost<T>(endpoint, payload, { ...options, _retried: true });
      forceLogout();
      throw new ApiError('Your session has expired. Please sign in again.', STATUS.UNAUTHORIZED, 401);
    }

    // A 404 means the endpoint isn't implemented in the backend yet. Treat it as a soft
    // business-level failure (like the "Feature under development" stubs) rather than a hard
    // throw, so list/report screens render a clean empty state and forms show a friendly note
    // instead of the page crashing.
    if (httpStatus === 404) {
      return { ok: false, status: STATUS.FAILED, message: 'This feature is not available yet.', data: null };
    }

    // A 403 is the server's access decision — the role may not call this endpoint, or the
    // request named a user outside the caller's own branch of the network. Surface it as a
    // business-level failure so the screen shows the reason instead of crashing.
    if (httpStatus === 403) {
      return {
        ok: false,
        status: STATUS.FAILED,
        message: String(field(ax.response?.data, 'message') ?? "You don't have permission to perform this action."),
        data: null,
      };
    }

    const serverMessage = field(ax.response?.data, 'message');
    if (serverMessage) {
      throw new ApiError(
        String(serverMessage),
        String(field(ax.response?.data, 'status') ?? STATUS.ERROR),
        httpStatus,
      );
    }
    if (ax.code === 'ERR_NETWORK') {
      throw new ApiError('Cannot reach the server. Please check your connection.', STATUS.ERROR, 0);
    }
    throw new ApiError(ax.message || 'Something went wrong.', STATUS.ERROR, httpStatus);
  }
}

// --- token refresh (deduped across concurrent 401s) -------------------------
let refreshPromise: Promise<boolean> | null = null;

function tryRefresh(): Promise<boolean> {
  const tokens = loadSession()?.tokens;
  if (!tokens?.RefreshToken) return Promise.resolve(false);
  if (!refreshPromise) {
    refreshPromise = doRefresh(tokens).finally(() => {
      refreshPromise = null;
    });
  }
  return refreshPromise;
}

async function doRefresh(tokens: Tokens): Promise<boolean> {
  try {
    const form = new FormData();
    form.append(
      'Request',
      encrypt(JSON.stringify({ AccessToken: tokens.AccessToken, RefreshToken: tokens.RefreshToken })),
    );
    form.append('KeyData', encrypt('{}'));
    const env = (await http.post('Auth/refresh', form)).data;
    if (String(field(env, 'status') ?? '').toUpperCase() !== STATUS.SUCCESS) return false;
    const dataStr = (field(env, 'data') as string) ?? '';
    const next = JSON.parse(decrypt(dataStr) || dataStr) as Tokens;
    if (!next?.AccessToken) return false;
    patchTokens(next);
    return true;
  } catch {
    return false;
  }
}

function forceLogout(): void {
  clearSession();
  window.dispatchEvent(new CustomEvent(SESSION_EXPIRED_EVENT));
}

export function apiBaseUrl(): string {
  return BASE_URL;
}
