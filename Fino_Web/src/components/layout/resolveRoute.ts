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

/**
 * Is this menu href the route currently open?
 *
 * Why this exists rather than NavLink's own `isActive`: that flag matches on the PATHNAME
 * alone. Every not-yet-built menu item resolves to `/coming-soon?title=<name>` — the same
 * pathname for all of them — so opening any one placeholder highlighted EVERY placeholder in
 * the dropdown at once, which reads as "the menu is highlighting the wrong item". Comparing
 * the query string as well leaves exactly one row highlighted: the one that was clicked.
 */
export function isActiveHref(href: string, pathname: string, search: string): boolean {
  const [hrefPath, hrefQuery = ''] = href.split('?');
  if (hrefPath !== pathname) return false;
  // No query on the link — the pathname match is the whole answer.
  if (!hrefQuery) return true;
  return hrefQuery === search.replace(/^\?/, '');
}
