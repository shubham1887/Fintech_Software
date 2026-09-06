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
    return (rows as Record<string, unknown>[]).map(mapRow).sort((a, b) => a.menuOrder - b.menuOrder);
  } catch {
    return [];
  }
}
