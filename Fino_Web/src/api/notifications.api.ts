import { apiPost } from '@/lib/apiClient';
import { getField } from '@/lib/row';

/**
 * The notification bell's data layer.
 *
 * Everything here is scoped server-side to the signed-in user — no UserID is ever sent,
 * because the API takes the identity from the session envelope. See
 * `Fino_API/Controllers/AppNotificationController.cs`.
 */

export type NotificationKind = 'Info' | 'Alert' | 'Promo' | 'Ticket' | 'Fund' | 'Txn' | 'System';

export interface AppNotification {
  id: number;
  title: string;
  body: string;
  kind: NotificationKind;
  imageUrl: string;
  /** In-app path to open on tap, e.g. "/s/TicketReport". Blank = nothing to open. */
  actionUrl: string;
  isRead: boolean;
  at: string;
}

export interface NotificationFeed {
  unreadCount: number;
  totalCount: number;
  items: AppNotification[];
}

const EMPTY: NotificationFeed = { unreadCount: 0, totalCount: 0, items: [] };

function toNotification(row: Record<string, unknown>): AppNotification {
  return {
    id: Number(getField(row, 'AppNotificationID') ?? 0),
    title: String(getField(row, 'Title') ?? ''),
    body: String(getField(row, 'Body') ?? ''),
    kind: (String(getField(row, 'Kind') ?? 'Info') as NotificationKind),
    imageUrl: String(getField(row, 'ImageUrl') ?? ''),
    actionUrl: String(getField(row, 'ActionUrl') ?? ''),
    isRead: Boolean(getField(row, 'IsRead')),
    at: String(getField(row, 'DateTime') ?? ''),
  };
}

/**
 * One call gives the badge count AND the list, so the bell never renders a count that
 * disagrees with what is under it.
 */
export async function getNotificationFeed(
  opts: { pageNumber?: number; pageSize?: number; unreadOnly?: boolean; kind?: string } = {},
): Promise<NotificationFeed> {
  const res = await apiPost<unknown>('AppNotification/Feed', {
    PageNumber: opts.pageNumber ?? 1,
    PageSize: opts.pageSize ?? 30,
    UnreadOnly: opts.unreadOnly ?? false,
    Kind: opts.kind ?? '',
  });
  // A quiet inbox is not an error — render the empty state, don't blow up the app bar.
  if (!res.ok || !res.data || typeof res.data !== 'object') return EMPTY;

  const data = res.data as Record<string, unknown>;
  const items = getField(data, 'Items');
  return {
    unreadCount: Number(getField(data, 'UnreadCount') ?? 0),
    totalCount: Number(getField(data, 'TotalCount') ?? 0),
    items: Array.isArray(items) ? (items as Record<string, unknown>[]).map(toNotification) : [],
  };
}

/** Mark one alert read, or the whole feed when `id` is omitted. */
export function markNotificationRead(id?: number) {
  return apiPost('AppNotification/MarkRead', {
    AppNotificationID: id ?? 0,
    All: id === undefined,
  });
}

export interface SendAlertInput {
  title: string;
  body: string;
  /** All = everyone the sender may reach; Role/Users/Downline narrow it. */
  audience: 'All' | 'Role' | 'Users' | 'Downline';
  userTypeIds?: number[];
  userIds?: number[];
  kind?: NotificationKind;
  actionUrl?: string;
  imageUrl?: string;
  /**
   * Deliver a copy to the sender too.
   *
   * The sender is normally dropped from their own audience. That is correct, and it is also
   * why App Alert read as broken: an admin would send one, check their own bell, find it
   * empty, and conclude nothing had been delivered. A copy is the only way to see what the
   * network actually received.
   */
  includeSelf?: boolean;
  /**
   * Drop the alert out of everyone's bell this many days after it is sent. 0 = never.
   *
   * Almost every alert is true only for a while ("maintenance tonight", "offer till
   * Sunday") and nothing used to retire one, so a user's feed filled with notices that had
   * stopped applying and the only cure was dismissing each by hand.
   */
  expiresInDays?: number;
}

/** Compose an alert. Admin/support reach the platform; a distributor their own downline. */
export function sendAlert(input: SendAlertInput) {
  return apiPost('AppNotification/Send', {
    Title: input.title,
    Body: input.body,
    Audience: input.audience,
    UserTypeIDs: input.userTypeIds ?? [],
    UserIDs: input.userIds ?? [],
    Kind: input.kind ?? 'Alert',
    ActionUrl: input.actionUrl ?? '',
    ImageUrl: input.imageUrl ?? '',
    IncludeSelf: input.includeSelf ?? false,
    ExpiresInDays: input.expiresInDays ?? 0,
  });
}

export interface PushStatus {
  /** A Firebase service account is configured on the server. */
  configured: boolean;
  /** Active device registrations platform-wide (phones + browsers). */
  devices: number;
  androidDevices: number;
  /** What to fix, in plain language. Empty when push is working. */
  message: string;
}

/**
 * Can this install reach a phone's tray at all?
 *
 * Push needs Firebase filled in per client (a service account on the server, four values in
 * the app's hosted app-config.json). Neither is code, and when they are missing the composer
 * still reports a successful send — so the screens that broadcast ask this and say it plainly
 * instead of leaving "the alert never reached my phone" to be reported as a bug.
 */
export async function getPushStatus(): Promise<PushStatus | null> {
  const res = await apiPost<Record<string, unknown>>('AppNotification/PushStatus', {});
  if (!res.ok || !res.data) return null;
  const d = res.data;
  return {
    configured: Boolean(getField(d, 'PushConfigured') ?? false),
    devices: Number(getField(d, 'RegisteredDevices') ?? 0),
    androidDevices: Number(getField(d, 'AndroidDevices') ?? 0),
    message: String(getField(d, 'Message') ?? ''),
  };
}

export interface SentAlert {
  id: number;
  title: string;
  body: string;
  kind: string;
  audience: string;
  audienceRoles: string;
  recipientCount: number;
  pushedCount: number;
  readCount: number;
  createdByName: string;
  at: string;
  /** When it drops out of recipients' bells. Empty = it stays until deleted. */
  expiresOn: string;
  /** Already retired — nobody can see it any more, which the ledger says out loud. */
  isExpired: boolean;
}

/** Admin's record of what went out, to how many, and how much of it was read. */
export async function getSentAlerts(pageSize = 50): Promise<SentAlert[]> {
  const res = await apiPost<unknown>('AppNotification/Sent', { PageNumber: 1, PageSize: pageSize });
  if (!res.ok || !Array.isArray(res.data)) return [];
  return (res.data as Record<string, unknown>[]).map((r) => ({
    id: Number(getField(r, 'AppNotificationID') ?? 0),
    title: String(getField(r, 'Title') ?? ''),
    body: String(getField(r, 'Body') ?? ''),
    kind: String(getField(r, 'Kind') ?? ''),
    audience: String(getField(r, 'Audience') ?? ''),
    audienceRoles: String(getField(r, 'AudienceRoles') ?? ''),
    recipientCount: Number(getField(r, 'RecipientCount') ?? 0),
    pushedCount: Number(getField(r, 'PushedCount') ?? 0),
    readCount: Number(getField(r, 'ReadCount') ?? 0),
    createdByName: String(getField(r, 'CreatedByName') ?? ''),
    at: String(getField(r, 'DateTime') ?? ''),
    expiresOn: String(getField(r, 'ExpiresOn') ?? ''),
    isExpired: Boolean(getField(r, 'IsExpired')),
  }));
}

/**
 * Hand the platform this browser's / handset's push token.
 *
 * Called by `lib/push.ts` on sign-in and whenever Firebase rotates the token. Until this
 * existed, the server had no address to push to at all.
 */
export function registerDevice(input: {
  fcmToken: string;
  platform: 'android' | 'web' | 'ios';
  deviceName?: string;
  appVersion?: string;
  remove?: boolean;
}) {
  return apiPost('AppNotification/RegisterDevice', {
    FcmToken: input.fcmToken,
    Platform: input.platform,
    DeviceName: input.deviceName ?? '',
    AppVersion: input.appVersion ?? '',
    Remove: input.remove ?? false,
  });
}
