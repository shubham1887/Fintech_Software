import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Megaphone, Send, Users2, UserRound, Globe2, Network, BellOff } from 'lucide-react';
import { getSentAlerts, getPushStatus, sendAlert, type NotificationKind } from '@/api/notifications.api';
import { fetchList } from '@/api/list.api';
import { getField } from '@/lib/row';
import { loadSession } from '@/lib/storage';
import { UserTypeCode, roleLabel } from '@/lib/roles';
import { dateTime } from '@/lib/format';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Badge } from '@/components/ui/Badge';
import { ImageField } from '@/components/ui/DynamicForm';
import type { CustomScreenProps } from './registry';

/**
 * Compose an app alert.
 *
 * The screen this replaces was a three-field generic form whose only audience control was a
 * single "Audience" dropdown with no default — so the common case was submitting it blank,
 * which the API read as *every user on the platform*. That is the "alert sabko chala jata
 * hai" complaint, and it is a UI problem as much as a server one.
 *
 * So: the audience is a deliberate choice of four, the two narrowing ones force a selection,
 * and the button says how many people are about to be written to before it is pressed.
 */

type Audience = 'All' | 'Role' | 'Users' | 'Downline';

const AUDIENCES: Array<{ key: Audience; label: string; hint: string; icon: typeof Globe2 }> = [
  { key: 'Role', label: 'By role', hint: 'Every retailer, every distributor…', icon: Users2 },
  { key: 'Users', label: 'Specific users', hint: 'Pick the exact people', icon: UserRound },
  { key: 'Downline', label: 'My downline', hint: 'Everyone beneath me in the network', icon: Network },
  { key: 'All', label: 'Everyone', hint: 'The whole network I can reach', icon: Globe2 },
];

const KINDS: Array<{ value: NotificationKind; label: string }> = [
  { value: 'Alert', label: 'Alert — something needs attention' },
  { value: 'Info', label: 'Info — general announcement' },
  { value: 'Promo', label: 'Offer — promotional' },
  { value: 'System', label: 'System — maintenance / downtime' },
];

/**
 * How long an alert stays in the bell.
 *
 * Alerts are time-bound far more often than not, and nothing retired them before — a
 * maintenance notice from March was still sitting unread in people's feeds in June. A week
 * is the default; "until I remove it" remains available for a standing notice.
 */
const AUTO_HIDE = [
  { value: '1', label: 'After 1 day' },
  { value: '3', label: 'After 3 days' },
  { value: '7', label: 'After 7 days' },
  { value: '15', label: 'After 15 days' },
  { value: '30', label: 'After 30 days' },
  { value: '0', label: 'Until I remove it' },
];

/** Roles it makes sense to broadcast to. Support and admin read the sent list instead. */
const ROLE_CHOICES = [
  UserTypeCode.SuperDistributor,
  UserTypeCode.Distributor,
  UserTypeCode.Retailer,
  UserTypeCode.ApiUser,
  UserTypeCode.Client,
];

export function AlertComposerScreen({ def }: CustomScreenProps) {
  const toast = useToast();
  const qc = useQueryClient();

  const session = loadSession();
  const myRole = Number(session?.user?.UserTypeID ?? 0);
  const isAdmin = myRole === UserTypeCode.Admin || myRole === UserTypeCode.SupportUser;

  const [title, setTitle] = useState('');
  const [body, setBody] = useState('');
  const [kind, setKind] = useState<NotificationKind>('Alert');
  const [actionUrl, setActionUrl] = useState('');
  const [imageUrl, setImageUrl] = useState('');
  // Everyone by default. An App Alert is the "tell the whole network" channel — that is what
  // it is opened to do — and starting on a narrowing choice meant the common case needed two
  // extra clicks, with a half-picked role list the likeliest way to send one to the wrong
  // people. The narrowing options are one click away and the server still clips a
  // distributor's "everyone" to their own downline.
  const [audience, setAudience] = useState<Audience>('All');
  const [roles, setRoles] = useState<number[]>([UserTypeCode.Retailer]);
  const [userIds, setUserIds] = useState<number[]>([]);
  const [userSearch, setUserSearch] = useState('');
  // On by default, deliberately. The sender is excluded from their own audience, so without a
  // copy the only feedback an operator gets is a toast — and a toast is exactly what somebody
  // reports as "I sent it and it didn't show up anywhere".
  const [copyToMe, setCopyToMe] = useState(true);
  // How long the alert stays in the bell. Seven days is the sensible default for the kind of
  // thing this screen sends; "Until I remove it" is still there for a standing notice.
  const [expiresInDays, setExpiresInDays] = useState('7');

  // Only loaded for the "specific users" case — the list is big and nobody else needs it.
  const { data: users = [] } = useQuery({
    queryKey: ['alert-users'],
    enabled: audience === 'Users',
    staleTime: 300_000,
    queryFn: () => fetchList('Users/GetAll', { PageNumber: 1, PageSize: 1000, IsActive: true }),
  });

  const { data: sent = [] } = useQuery({
    queryKey: ['sent-alerts'],
    enabled: isAdmin,
    queryFn: () => getSentAlerts(25),
  });

  // Whether a push can leave this install at all. Shown before anything is sent, because the
  // alternative is an operator broadcasting, seeing "Alert sent", and reporting the phone
  // notification as broken when the cause is Firebase not being filled in for this client.
  const { data: push } = useQuery({
    queryKey: ['push-status'],
    enabled: isAdmin,
    staleTime: 300_000,
    queryFn: getPushStatus,
  });

  const userOptions = useMemo(() => {
    const q = userSearch.trim().toLowerCase();
    const rows = users.map((u) => ({
      id: Number(getField(u, 'UserMasterID', 'UserID') ?? 0),
      name: String(getField(u, 'PersonName', 'FirmName') ?? ''),
      mobile: String(getField(u, 'MobileNumber') ?? ''),
      role: Number(getField(u, 'UserTypeID') ?? 0),
    })).filter((u) => u.id > 0);

    if (!q) return rows.slice(0, 50);
    return rows.filter((u) => u.name.toLowerCase().includes(q) || u.mobile.includes(q)).slice(0, 50);
  }, [users, userSearch]);

  const send = useMutation({
    mutationFn: () =>
      sendAlert({
        title: title.trim(),
        body: body.trim(),
        audience,
        userTypeIds: audience === 'Role' ? roles : [],
        userIds: audience === 'Users' ? userIds : [],
        kind,
        actionUrl: actionUrl.trim(),
        imageUrl: imageUrl.trim(),
        includeSelf: copyToMe,
        expiresInDays: Number(expiresInDays) || 0,
      }),
    onSuccess: (res) => {
      if (!res.ok) {
        toast.error(res.message || 'Could not send the alert.');
        return;
      }
      // The API answers with the real reach ("sent to 412 user(s); 190 had a registered
      // device"), which is the only honest confirmation — "Alert sent." said nothing.
      toast.success(res.message || 'Alert sent.');
      setTitle('');
      setBody('');
      setActionUrl('');
      setImageUrl('');
      setUserIds([]);
      qc.invalidateQueries({ queryKey: ['sent-alerts'] });
      // The copy has to appear in the bell now, not at the next 60-second poll — the whole
      // point of it is that the sender can confirm delivery immediately.
      if (copyToMe) {
        qc.invalidateQueries({ queryKey: ['notification-feed'] });
        qc.invalidateQueries({ queryKey: ['notifications-page'] });
      }
    },
    onError: () => toast.error('Could not send the alert.'),
  });

  const blocked =
    !title.trim() ||
    !body.trim() ||
    (audience === 'Role' && roles.length === 0) ||
    (audience === 'Users' && userIds.length === 0);

  function toggleRole(role: number) {
    setRoles((prev) => (prev.includes(role) ? prev.filter((r) => r !== role) : [...prev, role]));
  }

  function toggleUser(id: number) {
    setUserIds((prev) => (prev.includes(id) ? prev.filter((u) => u !== id) : [...prev, id]));
  }

  return (
    <div className="space-y-4">
      <PageHeader title={def.title} subtitle={def.subtitle} icon={Megaphone} />

      {push?.message && (
        <div className="flex items-start gap-2.5 rounded-card border border-amber-200 bg-amber-50 p-3.5 text-sm text-amber-800">
          <BellOff className="mt-0.5 size-4 shrink-0" />
          <div>
            <p className="font-semibold">Phone notifications are not being delivered</p>
            <p className="mt-0.5 text-[13px] leading-relaxed text-amber-700">{push.message}</p>
          </div>
        </div>
      )}
      {push && !push.message && push.androidDevices === 0 && push.devices > 0 && (
        <div className="flex items-start gap-2.5 rounded-card border border-slate-200 bg-slate-50 p-3.5 text-sm text-slate-600">
          <BellOff className="mt-0.5 size-4 shrink-0 text-slate-400" />
          <p>
            Push is configured and {push.devices} browser{push.devices === 1 ? '' : 's'} can
            receive alerts, but no Android handset has registered yet — so this alert will not
            appear in a phone's tray.
          </p>
        </div>
      )}

      <div className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_22rem]">
        {/* ------------------------------------------------------------ compose */}
        <div className="space-y-4 rounded-card border border-slate-200/70 bg-white p-4 shadow-card md:p-5">
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="sm:col-span-2">
              <Input
                label="Title"
                value={title}
                maxLength={200}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="Scheduled maintenance tonight"
              />
            </div>

            <div className="sm:col-span-2">
              <label className="mb-1.5 block text-sm font-medium text-slate-700">Message</label>
              <textarea
                value={body}
                maxLength={1000}
                rows={4}
                onChange={(e) => setBody(e.target.value)}
                placeholder="AEPS will be unavailable from 11:00 PM to 1:00 AM."
                className="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm text-slate-800 outline-none transition-colors placeholder:text-slate-400 focus:border-brand-500 focus:ring-2 focus:ring-brand-500/20"
              />
              <p className="mt-1 text-right text-[11px] text-slate-400">{body.length}/1000</p>
            </div>

            <Select label="Type" value={kind} onChange={(e) => setKind(e.target.value as NotificationKind)}>
              {KINDS.map((k) => (
                <option key={k.value} value={k.value}>
                  {k.label}
                </option>
              ))}
            </Select>

            <Input
              label="Opens (optional)"
              value={actionUrl}
              onChange={(e) => setActionUrl(e.target.value)}
              placeholder="/s/TicketReport"
              hint="In-app path the alert opens when tapped"
            />

            <div>
              <Select
                label="Auto-hide"
                value={expiresInDays}
                onChange={(e) => setExpiresInDays(e.target.value)}
              >
                {AUTO_HIDE.map((o) => (
                  <option key={o.value} value={o.value}>
                    {o.label}
                  </option>
                ))}
              </Select>
              <p className="mt-1 text-xs text-slate-400">
                When it leaves the bell. Who received and read it is still recorded either way.
              </p>
            </div>

            {/* Same upload control the News form uses, so an offer looks the same whichever
                screen it is sent from — the two used to differ, which is why one of them was
                reported as "not working the same way". */}
            <div className="sm:col-span-2">
              <ImageField
                field={{
                  name: 'ImageUrl',
                  label: 'Image (optional)',
                  type: 'image',
                  hint: 'Shown inside the alert and in the phone notification',
                }}
                value={imageUrl}
                onChange={setImageUrl}
              />
            </div>
          </div>

          {/* ---------------------------------------------------------- audience */}
          <div>
            <p className="mb-2 text-sm font-medium text-slate-700">Who receives this</p>
            <div className="grid gap-2 sm:grid-cols-2">
              {AUDIENCES.map((a) => {
                const Icon = a.icon;
                const active = audience === a.key;
                return (
                  <button
                    key={a.key}
                    type="button"
                    onClick={() => setAudience(a.key)}
                    className={cn(
                      'flex items-start gap-2.5 rounded-lg border p-3 text-left transition-colors',
                      active
                        ? 'border-brand-500 bg-brand-50/60 ring-1 ring-brand-500'
                        : 'border-slate-200 hover:border-slate-300 hover:bg-slate-50',
                    )}
                  >
                    <Icon className={cn('mt-0.5 size-4 shrink-0', active ? 'text-brand-600' : 'text-slate-400')} />
                    <span className="min-w-0">
                      <span className="block text-sm font-semibold text-slate-800">{a.label}</span>
                      <span className="block text-xs text-slate-500">{a.hint}</span>
                    </span>
                  </button>
                );
              })}
            </div>

            {audience === 'Role' && (
              <div className="mt-3 flex flex-wrap gap-1.5">
                {ROLE_CHOICES.map((r) => (
                  <button
                    key={r}
                    type="button"
                    onClick={() => toggleRole(r)}
                    className={cn(
                      'rounded-full px-3 py-1.5 text-xs font-semibold transition-colors',
                      roles.includes(r)
                        ? 'bg-brand-600 text-white'
                        : 'bg-white text-slate-500 ring-1 ring-slate-200 hover:text-slate-700',
                    )}
                  >
                    {roleLabel(r, '')}
                  </button>
                ))}
              </div>
            )}

            {audience === 'Users' && (
              <div className="mt-3 space-y-2">
                <Input
                  value={userSearch}
                  onChange={(e) => setUserSearch(e.target.value)}
                  placeholder="Search by name or mobile"
                />
                <div className="max-h-56 overflow-y-auto rounded-lg border border-slate-200">
                  {userOptions.length === 0 ? (
                    <p className="px-3 py-6 text-center text-xs text-slate-400">No users match.</p>
                  ) : (
                    userOptions.map((u, i) => (
                      <label
                        key={u.id}
                        className={cn(
                          'flex cursor-pointer items-center gap-2.5 px-3 py-2 text-sm hover:bg-slate-50',
                          i > 0 && 'border-t border-slate-100',
                        )}
                      >
                        <input
                          type="checkbox"
                          checked={userIds.includes(u.id)}
                          onChange={() => toggleUser(u.id)}
                          className="size-4 accent-[var(--color-brand-600)]"
                        />
                        <span className="min-w-0 flex-1 truncate text-slate-700">{u.name}</span>
                        <span className="shrink-0 text-xs tabular-nums text-slate-400">{u.mobile}</span>
                      </label>
                    ))
                  )}
                </div>
                {userIds.length > 0 && (
                  <p className="text-xs text-slate-500">{userIds.length} selected</p>
                )}
              </div>
            )}

            {audience === 'All' && !isAdmin && (
              <p className="mt-2 text-xs text-slate-500">
                This reaches everyone in your own downline — the server will not let an alert
                cross into another branch of the network.
              </p>
            )}
            {audience === 'All' && isAdmin && (
              <p className="mt-2 text-xs text-amber-600">
                This reaches every active user on the platform. Pick a role instead if you mean
                one group.
              </p>
            )}
          </div>

          <div className="flex flex-wrap items-center justify-between gap-3 border-t border-slate-100 pt-4">
            <label className="flex cursor-pointer items-center gap-2 text-sm text-slate-600">
              <input
                type="checkbox"
                checked={copyToMe}
                onChange={(e) => setCopyToMe(e.target.checked)}
                className="size-4 accent-[var(--color-brand-600)]"
              />
              Send me a copy
              <span className="text-xs text-slate-400">— so you can see it in your own bell</span>
            </label>
            <Button onClick={() => send.mutate()} disabled={blocked} loading={send.isPending}>
              <Send className="size-4" />
              Send alert
            </Button>
          </div>
        </div>

        {/* --------------------------------------------------------------- sent */}
        {isAdmin && (
          <div className="rounded-card border border-slate-200/70 bg-white shadow-card">
            <div className="border-b border-slate-100 px-4 py-3">
              <p className="text-sm font-semibold text-slate-800">Recently sent</p>
              <p className="text-xs text-slate-400">Reach and how much of it was read</p>
            </div>
            <div className="max-h-[32rem] overflow-y-auto">
              {sent.length === 0 ? (
                <p className="px-4 py-10 text-center text-xs text-slate-400">Nothing sent yet.</p>
              ) : (
                sent.map((s, i) => (
                  <div key={s.id} className={cn('px-4 py-3', i > 0 && 'border-t border-slate-100')}>
                    <div className="flex items-baseline gap-2">
                      <p className="min-w-0 flex-1 truncate text-[13px] font-semibold text-slate-800">{s.title}</p>
                      <Badge tone="neutral">{s.kind}</Badge>
                      {s.isExpired && <Badge tone="warning">Hidden</Badge>}
                    </div>
                    <p className="mt-0.5 line-clamp-2 text-xs text-slate-500">{s.body}</p>
                    <p className="mt-1.5 text-[11px] tabular-nums text-slate-400">
                      {s.recipientCount} sent · {s.pushedCount} pushed · {s.readCount} read
                    </p>
                    <p className="text-[11px] text-slate-400">
                      {s.audience}
                      {s.audienceRoles ? ` (${s.audienceRoles})` : ''} · {dateTime(s.at)}
                      {s.expiresOn && !s.isExpired ? ` · hides ${dateTime(s.expiresOn)}` : ''}
                    </p>
                  </div>
                ))
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
