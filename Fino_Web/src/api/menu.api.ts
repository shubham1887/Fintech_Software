import { apiPost } from '@/lib/apiClient';
import { loadSession } from '@/lib/storage';
import { UserTypeCode } from '@/lib/roles';

/** Mirrors Fino_API `Responses/Menu/MenuItem.cs` (role-filtered server-side). */
export interface MenuNode {
  name: string;
  url?: string;
  icon?: string;
  children?: MenuNode[];
}

/** Menu URLs hidden from the UI regardless of what the backend menu returns. */
const HIDDEN_URLS = new Set(['/agentdetailreport']);

function stripHidden(nodes: MenuNode[]): MenuNode[] {
  return nodes
    .filter((n) => !(n.url && HIDDEN_URLS.has(n.url.toLowerCase())))
    .map((n) => (n.children ? { ...n, children: stripHidden(n.children) } : n));
}

/** True if any node in the tree already points at `url`. */
function hasUrl(nodes: MenuNode[], url: string): boolean {
  return nodes.some((n) => n.url?.toLowerCase() === url.toLowerCase() || (n.children ? hasUrl(n.children, url) : false));
}

/**
 * Add `child` as the last item of the first top-level section whose name matches `matcher`
 * (e.g. the "Settings" group). Returns the new tree and whether it was placed; callers fall
 * back to a top-level item when the section isn't present.
 */
function addUnderSection(nodes: MenuNode[], matcher: RegExp, child: MenuNode): { nodes: MenuNode[]; added: boolean } {
  let added = false;
  const out = nodes.map((n) => {
    if (!added && matcher.test(n.name) && n.children) {
      added = true;
      return { ...n, children: [...n.children, child] };
    }
    return n;
  });
  return { nodes: out, added };
}

/**
 * Client-injected menu items not (yet) served by the backend SidebarMenu.
 * API Access is a self-service console for API users; admins/support get it too so they
 * can reach it. Added only when the backend menu doesn't already include the URL.
 */
function withInjected(nodes: MenuNode[]): MenuNode[] {
  const role = loadSession()?.user?.UserTypeID;
  let tree = nodes;
  const extra: MenuNode[] = [];
  // Registration Approvals — admins review self-service ("Create account") sign-ups.
  if (role === UserTypeCode.Admin && !hasUrl(nodes, '/RegistrationApprovals')) {
    extra.push({ name: 'Registration Approvals', url: '/RegistrationApprovals', icon: 'user' });
  }
  // API Access (token / IP / callbacks) is strictly for API users.
  if (role === UserTypeCode.ApiUser && !hasUrl(nodes, '/ApiAccess')) {
    extra.push({ name: 'API Access', url: '/ApiAccess', icon: 'key' });
  }
  // API Documentation is visible to API users and Admins.
  if ((role === UserTypeCode.ApiUser || role === UserTypeCode.Admin) && !hasUrl(nodes, '/ApiDocs')) {
    extra.push({ name: 'API Documentation', url: '/ApiDocs', icon: 'document' });
  }
  // Verification Settings — admin/support configure the Manual-Verification document master.
  // Placed inside the "Settings" section (falls back to top-level if that section is absent).
  if ((role === UserTypeCode.Admin || role === UserTypeCode.SupportUser) && !hasUrl(tree, '/VerificationDocuments')) {
    const item: MenuNode = { name: 'Verification Settings', url: '/VerificationDocuments', icon: 'kyc' };
    const res = addUnderSection(tree, /setting/i, item);
    tree = res.nodes;
    if (!res.added) extra.push(item);
  }
  // KYC has no menu entry of its own any more — it lives on the user's profile page
  // (/profile), which every role reaches from the topbar, and it surfaces there only while
  // something is actually pending.
  return extra.length ? [...tree, ...extra] : tree;
}

/** POST api/SidebarMenu/Select — returns the menu for the authenticated role. */
export async function getMenu(): Promise<MenuNode[]> {
  const res = await apiPost<{ data: MenuNode[] }>('SidebarMenu/Select', {});
  return withInjected(stripHidden(res.data?.data ?? []));
}
