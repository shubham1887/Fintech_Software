import {
  ArrowLeftRight, Banknote, Car, CreditCard, Droplet, Fingerprint, Landmark, ReceiptText,
  Smartphone, Tv, Wallet, Wifi, Zap, type LucideIcon,
} from 'lucide-react';

/**
 * How a catalog service is drawn and where its tile goes.
 *
 * The desktop dashboard, the phone home screen and the Quick Services shortcuts all render the
 * same services, so they share one table. Keeping a private copy per screen meant a new service
 * kind got an icon on one surface and the generic wallet glyph on another, and — worse — could
 * route to the recharge flow on the phone while routing to the AEPS screen on the desktop.
 *
 * First match wins. The icon is only the fallback for when the admin has not uploaded a service
 * image; the image, when there is one, always takes precedence.
 */
const SERVICE_LOOKS: Array<{ match: RegExp; icon: LucideIcon; tone: string; to?: string }> = [
  { match: /aeps/i, icon: Fingerprint, tone: 'bg-rose-50 text-rose-600', to: '/s/AEPSIndex' },
  { match: /micro\s*atm|matm/i, icon: Banknote, tone: 'bg-lime-50 text-lime-600', to: '/s/AEPSIndex' },
  { match: /money transfer|dmt/i, icon: ArrowLeftRight, tone: 'bg-emerald-50 text-emerald-600', to: '/s/DMTTransaction' },
  { match: /payout|settlement/i, icon: Landmark, tone: 'bg-indigo-50 text-indigo-600', to: '/s/PayoutV1' },
  { match: /prepaid/i, icon: Smartphone, tone: 'bg-blue-50 text-blue-600' },
  { match: /postpaid/i, icon: ReceiptText, tone: 'bg-cyan-50 text-cyan-600' },
  { match: /dth|cable/i, icon: Tv, tone: 'bg-violet-50 text-violet-600' },
  { match: /electricity/i, icon: Zap, tone: 'bg-amber-50 text-amber-600' },
  { match: /water/i, icon: Droplet, tone: 'bg-sky-50 text-sky-600' },
  { match: /broadband|landline|fiber/i, icon: Wifi, tone: 'bg-fuchsia-50 text-fuchsia-600' },
  { match: /fastag/i, icon: Car, tone: 'bg-orange-50 text-orange-600' },
  { match: /credit card/i, icon: CreditCard, tone: 'bg-purple-50 text-purple-600' },
  { match: /gas|lpg/i, icon: Zap, tone: 'bg-red-50 text-red-600' },
  { match: /insurance|loan|emi/i, icon: Landmark, tone: 'bg-teal-50 text-teal-600' },
];

const DEFAULT_LOOK = { icon: Wallet, tone: 'bg-slate-100 text-slate-600' };

export function serviceLook(name: string): { icon: LucideIcon; tone: string } {
  return SERVICE_LOOKS.find((l) => l.match.test(name)) ?? DEFAULT_LOOK;
}

/**
 * Where a service tile navigates: the flows with their own screens (AEPS / DMT / Payout) go
 * there, everything else lands on the recharge / bill-pay flow pre-filtered to the service.
 */
export function serviceHref(name: string): string {
  const look = SERVICE_LOOKS.find((l) => l.match.test(name));
  return (look && 'to' in look && look.to) || `/recharge?service=${encodeURIComponent(name)}`;
}

/**
 * The tooltip for a service the user may see but cannot open. A KYC hold is something they can
 * clear themselves, so it must not read like an admin lock they can do nothing about.
 */
export function lockMessage(lockReason: string): string {
  return lockReason === 'KYC pending'
    ? 'Your KYC is not approved yet. Complete your KYC from your profile to use this service.'
    : 'This service is locked for your account. Please contact your service provider.';
}
