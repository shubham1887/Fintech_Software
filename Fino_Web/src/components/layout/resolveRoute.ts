import type { MenuNode } from '@/api/menu.api';
import { findScreen } from '@/screens/registry';

/**
 * Maps a backend menu node to a route. Registry-backed screens route to /s/<backend-url>
 * (rendered dynamically by ScreenHost); the dashboard maps to the built dashboard; every
 * other (not-yet-built) item routes to a placeholder that names the pending screen.
 */
export function resolveHref(node: MenuNode): string {
  const url = node.url ?? '';
  if (/dashboard/i.test(`${url} ${node.name ?? ''}`)) return '/dashboard';
  // The recharge service grid (/Transaction) is the hand-built RechargePage.
  if (/^\/Transaction$/i.test(url)) return '/recharge';
  // Domestic Money Transfer (/DMTTransaction) is the hand-built DmtPage.
  if (/^\/DMTTransaction$/i.test(url)) return '/dmt';
  if (url && findScreen(url)) return `/s${url}`;
  return `/coming-soon?title=${encodeURIComponent(node.name ?? 'Screen')}`;
}
