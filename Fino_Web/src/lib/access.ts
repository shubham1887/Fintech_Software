import type { MenuNode } from '@/api/menu.api';

/**
 * Client-side mirror of the backend authorization.
 *
 * The server already decides what a role may see: `SidebarMenu/Select` returns a menu filtered
 * by user-type (and by which services are switched on). This module reflects that same answer
 * onto routing, so a page a role cannot see in the sidebar cannot be opened by typing its URL
 * either. There is exactly ONE source of truth — the role-filtered menu — instead of a
 * hand-maintained per-screen role list that drifts from the backend.
 *
 * This is a UX guard, not the security boundary: the API enforces the same rules again
 * (ApiAccessPolicy for "which endpoint", HierarchyScopePolicy for "whose rows").
 */

/** Every URL present anywhere in the role-filtered menu tree, lowercased. */
export function collectMenuUrls(nodes: MenuNode[], acc = new Set<string>()): Set<string> {
  for (const n of nodes) {
    if (n.url) acc.add(n.url.toLowerCase());
    if (n.children) collectMenuUrls(n.children, acc);
  }
  return acc;
}

/** "/Users/Profile" → ["/users/profile", "/users"] — a detail page inherits its section's access. */
function pathChain(path: string): string[] {
  const parts = path.toLowerCase().split('/').filter(Boolean);
  const chain: string[] = [];
  for (let i = parts.length; i > 0; i -= 1) chain.push('/' + parts.slice(0, i).join('/'));
  return chain;
}

export interface ScreenAccess {
  /** Menu URL this screen hangs off when it is not itself a menu entry (e.g. a detail page). */
  parent?: string;
  /** Explicit user-type ids, for a screen deliberately outside the menu. Checked instead of the menu. */
  roles?: number[];
}

/**
 * Whether the signed-in role may open `path` (a registry path such as "/Users/Edit").
 * `menuUrls` comes from {@link collectMenuUrls} on the menu the backend returned for this role.
 */
export function isScreenAllowed(
  path: string,
  menuUrls: Set<string>,
  def: ScreenAccess | undefined,
  roleId: number | undefined,
): boolean {
  // An explicit role list wins — used by screens that are intentionally not menu items.
  if (def?.roles && def.roles.length > 0) return roleId !== undefined && def.roles.includes(roleId);

  // The page itself, or any of its parent sections, is in the role's menu.
  for (const candidate of pathChain(path)) if (menuUrls.has(candidate)) return true;

  // A declared parent covers detail pages reached only by navigation.
  if (def?.parent && menuUrls.has(def.parent.toLowerCase())) return true;

  return false;
}
