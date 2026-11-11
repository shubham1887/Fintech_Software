import axios from 'axios';
import { apiBaseUrl, apiPost } from '@/lib/apiClient';
import { getField } from '@/lib/row';

export interface WebsitePageRow {
  id: number;
  slug: string;
  title: string;
  metaDescription: string;
  sectionsJson: string;
  menuOrder: number;
  showInMenu: boolean;
  isPublished: boolean;
}

export interface WebsitePagePayload {
  Slug: string;
  Title: string;
  MetaDescription: string;
  SectionsJson: string;
  MenuOrder: number;
  ShowInMenu: boolean;
  IsPublished: boolean;
}

function mapRow(r: Record<string, unknown>): WebsitePageRow {
  return {
    id: Number(getField(r, 'WebsitePageMasterID', 'ID') ?? 0),
    slug: String(getField(r, 'Slug') ?? ''),
    title: String(getField(r, 'Title') ?? ''),
    metaDescription: String(getField(r, 'MetaDescription') ?? ''),
    sectionsJson: String(getField(r, 'SectionsJson') ?? ''),
    menuOrder: Number(getField(r, 'MenuOrder') ?? 0),
    showInMenu: Boolean(getField(r, 'ShowInMenu') ?? true),
    isPublished: Boolean(getField(r, 'IsPublished') ?? false),
  };
}

/** Admin: all pages, drafts included (encrypted, admin-only). */
export async function getWebsitePages(): Promise<WebsitePageRow[]> {
  const res = await apiPost<unknown>('WebsitePage/GetAll', {});
  if (!res.ok) return [];
  const rows = Array.isArray(res.data) ? res.data : [];
  return (rows as Record<string, unknown>[]).map(mapRow).sort((a, b) => a.menuOrder - b.menuOrder);
}

export function createWebsitePage(p: WebsitePagePayload) {
  return apiPost('WebsitePage/Create', p);
}

export function updateWebsitePage(id: number, p: WebsitePagePayload) {
  return apiPost('WebsitePage/Update', { WebsitePageMasterID: id, ...p });
}

export function deleteWebsitePage(id: number) {
  return apiPost('WebsitePage/Delete', { WebsitePageMasterID: id });
}

/**
 * Public site: published pages only. Anonymous endpoint — plain JSON, no session,
 * no encryption (there is no logged-in visitor on the public website).
 */
export async function fetchPublicPages(): Promise<WebsitePageRow[]> {
  const res = await axios.get(`${apiBaseUrl()}WebsitePage/Public`, { timeout: 30_000 });
  const dataStr = String(res.data?.data ?? res.data?.Data ?? '');
  if (!dataStr) return [];
  try {
    const rows = JSON.parse(dataStr);
    if (!Array.isArray(rows)) return [];
    const mapped = (rows as Record<string, unknown>[]).map(mapRow).sort((a, b) => a.menuOrder - b.menuOrder);
    cachePublicPages(mapped);
    return mapped;
  } catch {
    return [];
  }
}

/**
 * Last-known public pages, kept in localStorage.
 *
 * The public site cannot render a single word until WebsitePage/Public answers, so until now
 * every visit to /, /services or /about showed a spinner for one whole round trip — and on a
 * cold app pool, a loaded SQL instance or a weak mobile connection that round trip is what
 * people describe as "the page sometimes doesn't load". Marketing copy that changes a few times
 * a year is the ideal thing to keep: a returning visitor gets the page painted immediately from
 * this and the network answer only corrects it a moment later, in the background.
 *
 * It is also the offline/degraded answer. If the API is unreachable the site shows the content
 * it had rather than a 404 that is really "the backend is down", which is a far worse thing for
 * a prospective customer to see.
 *
 * Deliberately NOT sessionStorage (it must survive a closed tab) and deliberately not versioned
 * by anything but shape: a row that fails to parse is simply ignored, so a future field rename
 * degrades to one spinner, never to a crash.
 */
const PUBLIC_PAGES_CACHE_KEY = 'fino.site.pages';

export function cachedPublicPages(): WebsitePageRow[] | null {
  try {
    const raw = localStorage.getItem(PUBLIC_PAGES_CACHE_KEY);
    if (!raw) return null;
    const rows = JSON.parse(raw);
    if (!Array.isArray(rows) || rows.length === 0) return null;
    // Shape check rather than trust: anything without a slug is unusable to the router below.
    const usable = (rows as WebsitePageRow[]).filter((r) => r && typeof r.slug === 'string' && r.slug !== '');
    return usable.length > 0 ? usable : null;
  } catch {
    return null;
  }
}

function cachePublicPages(rows: WebsitePageRow[]): void {
  try {
    if (rows.length === 0) return; // never cache "nothing" over a good copy
    localStorage.setItem(PUBLIC_PAGES_CACHE_KEY, JSON.stringify(rows));
  } catch {
    /* private mode / quota — the site works, it just pays the round trip */
  }
}
