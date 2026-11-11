import {
  Activity,
  ArrowLeftRight,
  Banknote,
  Bell,
  BookOpen,
  Boxes,
  Building2,
  CalendarClock,
  CircleDot,
  ClipboardCheck,
  ClipboardList,
  Clock,
  Coins,
  CreditCard,
  Database,
  FileBarChart,
  FileText,
  Gauge,
  GitBranch,
  Globe,
  HandCoins,
  Hash,
  History,
  Inbox,
  KeyRound,
  Landmark,
  Layers,
  LayoutDashboard,
  LayoutGrid,
  LifeBuoy,
  Megaphone,
  Menu,
  MessageSquare,
  Network,
  Newspaper,
  PencilLine,
  Percent,
  PiggyBank,
  Plug,
  Receipt,
  Scale,
  ScrollText,
  Search,
  Settings,
  Shield,
  ShieldCheck,
  SlidersHorizontal,
  Smartphone,
  Ticket,
  Tv,
  Undo2,
  Upload,
  UserCheck,
  UserCog,
  UserPlus,
  Users,
  Wallet,
  type LucideIcon,
} from 'lucide-react';

// Keyword → icon. Matched against the menu item's name + icon string (best effort,
// since backend icon strings come from the legacy menu definition).
//
// ORDER MATTERS: the first keyword found anywhere in "<icon> <name>" wins, so the more
// specific term has to come first. Two cases worth knowing about:
//
//   * 'service' sits ABOVE 'fund' and 'wallet'. A retailer's "Services" group carries the legacy
//     icon "AccountBalanceWalletIcon", so left lower it would match 'wallet' — and a retailer
//     sees BOTH "Funds" and "Services", which would then be two identical wallets in the bar.
//   * 'ledger' and 'accounting' sit ABOVE 'admin', so the Administration group's "Platform
//     Ledger" and "Accounting Summary" rows keep their own icons rather than collapsing into the
//     group's shield.
//
// Every TOP-LEVEL group must resolve to something meaningful here. When the top bar runs out of
// width it drops the labels and shows these icons alone — a group that falls through to the
// CircleDot default becomes an anonymous dot the operator cannot identify.
const KEYWORDS: Array<[string, LucideIcon]> = [
  ['dashboard', LayoutDashboard],
  ['search', Search],
  ['pending', Clock],
  // Before 'recharge' and 'upload', so "Reconciliation Upload" reads as reconciliation.
  ['recon', Scale],
  // Provider / user / circle routing rows, before 'user' and 'dmt' so they all share one icon.
  ['routing', GitBranch],
  ['recharge', Smartphone],
  ['dth', Tv],
  ['report', FileBarChart],
  ['statement', ScrollText],
  ['daybook', BookOpen],
  ['history', History],
  ['transaction', Receipt],
  ['service', LayoutGrid],
  // Before 'request', so a reverse request is not just another request.
  ['reverse', Undo2],
  ['request', Inbox],
  ['fund', Wallet],
  ['wallet', Wallet],
  ['virtual', Coins],
  ['transfer', ArrowLeftRight],
  ['payout', Banknote],
  ['cash', HandCoins],
  ['dmt', Banknote],
  ['aeps', Landmark],
  ['bank', Landmark],
  ['beneficiary', UserCheck],
  ['slab', Layers],
  // Kept for menus whose slab rows are still named "General Add" / "General Assign"; the shipped
  // menu says "Manage Slabs" / "Assign Slab", which the 'slab' keyword above already covers.
  ['general', Layers],
  ['commission', Percent],
  ['card', CreditCard],
  ['denomination', Hash],
  ['downline', GitBranch],
  ['user', Users],
  ['retailer', Users],
  ['distributor', Network],
  ['onboard', UserPlus],
  ['kyc', ShieldCheck],
  // After 'kyc', so "KYC Approvals" keeps the KYC shield.
  ['approval', ClipboardCheck],
  ['registration', ClipboardCheck],
  ['role', UserCog],
  ['subscription', CalendarClock],
  ['plan', ClipboardList],
  ['setting', Settings],
  ['config', Settings],
  ['provider', Boxes],
  ['operator', Boxes],
  ['api', Plug],
  ['ticket', Ticket],
  ['notification', Bell],
  ['alert', Megaphone],
  ['news', Newspaper],
  ['sms', MessageSquare],
  ['message', Megaphone],
  ['invoice', FileText],
  ['tax', FileText],
  ['document', FileText],
  ['ledger', Database],
  ['accounting', PiggyBank],
  ['admin', Shield],
  ['support', LifeBuoy],
  ['complaint', LifeBuoy],
  ['key', KeyRound],
  ['company', Building2],
  ['perform', Gauge],
  ['website', Globe],
  ['menu', Menu],
  ['mode', SlidersHorizontal],
  ['status', Activity],
  ['manual', PencilLine],
  ['upload', Upload],
];

export function pickIcon(name?: string, icon?: string): LucideIcon {
  const hay = `${icon ?? ''} ${name ?? ''}`.toLowerCase();
  for (const [kw, Icon] of KEYWORDS) {
    if (hay.includes(kw)) return Icon;
  }
  return CircleDot;
}
