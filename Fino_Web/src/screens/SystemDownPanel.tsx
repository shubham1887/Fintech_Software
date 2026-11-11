import { useEffect, useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AlertTriangle, Clock, Save } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { Card, CardBody } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';

/** One service as the window's exception list lists it. */
interface PolicyService {
  ServiceID: number;
  Name: string;
  ServiceCategory: string;
  IsActive: boolean;
  IsAllowedWhenSystemDown: boolean;
}

interface Policy {
  IsSystemDown: boolean;
  From: string;
  To: string;
  Message: string;
  IsDownNow: boolean;
  Services: PolicyService[];
}

/** Accessible On/Off pill switch — same control as the feature toggles above it. */
function Switch({ on, onChange, disabled }: { on: boolean; onChange: (v: boolean) => void; disabled?: boolean }) {
  return (
    <button
      type="button"
      role="switch"
      aria-checked={on}
      disabled={disabled}
      onClick={() => onChange(!on)}
      className={cn(
        'relative inline-flex h-6 w-11 shrink-0 items-center rounded-full transition-colors disabled:opacity-50',
        on ? 'bg-danger-600' : 'bg-slate-300',
      )}
    >
      <span
        className={cn(
          'inline-block size-5 transform rounded-full bg-white shadow transition-transform',
          on ? 'translate-x-5' : 'translate-x-0.5',
        )}
      />
    </button>
  );
}

/**
 * System Down — a daily maintenance window during which the platform refuses financial
 * transactions, with a named set of services allowed to keep running.
 *
 * The switch, the two clock times and the exception list are ONE decision and are saved together.
 * Saving them separately would mean a live window with no hours, or hours with no exceptions, and
 * both of those states stop the whole network trading.
 *
 * The window is read as a wall clock, so a From later than a To crosses midnight (22:00 → 02:00),
 * which is when maintenance is actually run. The server reads it the same way.
 */
export function SystemDownPanel() {
  const toast = useToast();
  const qc = useQueryClient();

  const { data: policy, isFetching } = useQuery({
    queryKey: ['system-down-policy'],
    queryFn: async () => {
      const res = await apiPost<Policy>('SystemDown/SelectPolicy', {});
      if (!res.ok) throw new Error(res.message || 'Could not load the system-down window.');
      return res.data as Policy;
    },
  });

  const [on, setOn] = useState(false);
  const [from, setFrom] = useState('');
  const [to, setTo] = useState('');
  const [message, setMessage] = useState('');
  const [allowed, setAllowed] = useState<Set<number>>(new Set());
  const [dirty, setDirty] = useState(false);

  // Server rows are the source of truth until the operator touches something.
  useEffect(() => {
    if (!policy || dirty) return;
    setOn(Boolean(policy.IsSystemDown));
    setFrom(policy.From ?? '');
    setTo(policy.To ?? '');
    setMessage(policy.Message ?? '');
    setAllowed(new Set((policy.Services ?? []).filter((s) => s.IsAllowedWhenSystemDown).map((s) => s.ServiceID)));
  }, [policy, dirty]);

  const grouped = useMemo(() => {
    const byCategory = new Map<string, PolicyService[]>();
    for (const s of policy?.Services ?? []) {
      const key = s.ServiceCategory || 'Other Services';
      if (!byCategory.has(key)) byCategory.set(key, []);
      byCategory.get(key)!.push(s);
    }
    return [...byCategory.entries()].sort((a, b) => a[0].localeCompare(b[0]));
  }, [policy]);

  const toggleService = (id: number) => {
    setDirty(true);
    setAllowed((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const saveMut = useMutation({
    mutationFn: async () => {
      const res = await apiPost('SystemDown/SavePolicy', {
        IsSystemDown: on,
        From: from.trim(),
        To: to.trim(),
        Message: message.trim(),
        AllowedServiceIDs: [...allowed],
      });
      if (!res.ok) throw new Error(res.message || 'Save failed.');
    },
    onSuccess: () => {
      toast.success('System-down window saved.');
      setDirty(false);
      qc.invalidateQueries({ queryKey: ['system-down-policy'] });
      // The banner every screen shows reads its own endpoint; drop it so the change is visible
      // without a reload.
      qc.invalidateQueries({ queryKey: ['system-down-status'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  const upd = <T,>(set: (v: T) => void) => (v: T) => { setDirty(true); set(v); };

  return (
    <Card className="mt-6">
      <CardBody className="p-0">
        <div className="flex flex-wrap items-start justify-between gap-4 border-b border-slate-100 px-5 py-4">
          <div className="min-w-[18rem] flex-1">
            <div className="flex items-center gap-2">
              <AlertTriangle className="size-4 text-danger-500" />
              <span className="text-sm font-semibold text-slate-800">System Down</span>
              {policy?.IsDownNow && (
                <span className="rounded-full bg-danger-50 px-2 py-0.5 text-[11px] font-semibold text-danger-700">
                  down right now
                </span>
              )}
            </div>
            <p className="mt-0.5 pl-6 text-xs text-slate-500">
              Between these times every financial transaction is refused — recharge, bill payment,
              money transfer, AEPS, Micro ATM, and fund transfers and requests — except on the
              services ticked below. Admin and support are not blocked, so the platform can still
              be settled and repaired while it is down.
            </p>
          </div>
          <div className="flex items-center gap-4">
            <Switch on={on} onChange={upd(setOn)} disabled={isFetching} />
            <span className={cn('w-8 text-xs font-medium', on ? 'text-danger-700' : 'text-slate-400')}>
              {on ? 'On' : 'Off'}
            </span>
            <Button size="sm" variant="outline" disabled={!dirty} loading={saveMut.isPending} onClick={() => saveMut.mutate()}>
              <Save className="size-4" /> Save
            </Button>
          </div>
        </div>

        <div className="grid grid-cols-1 gap-4 px-5 py-4 sm:grid-cols-2 lg:grid-cols-4">
          <Input
            label="From"
            type="time"
            value={from}
            onChange={(e) => upd(setFrom)(e.target.value)}
            leftIcon={<Clock className="size-4 text-slate-400" />}
            hint="24-hour clock"
          />
          <Input
            label="To"
            type="time"
            value={to}
            onChange={(e) => upd(setTo)(e.target.value)}
            leftIcon={<Clock className="size-4 text-slate-400" />}
            hint={from && to && from > to ? 'Crosses midnight' : 'Same day'}
          />
          <div className="sm:col-span-2">
            <Input
              label="Message shown to users"
              value={message}
              placeholder="System is down for maintenance until 02:00. DMT is still available."
              onChange={(e) => upd(setMessage)(e.target.value)}
              hint="Left blank, a standard wording is used."
            />
          </div>
        </div>

        <div className="border-t border-slate-100 px-5 py-4">
          <p className="text-sm font-medium text-slate-700">Services that keep working while the system is down</p>
          <p className="mt-0.5 text-xs text-slate-500">
            Nothing is ticked by default, so switching the window on stops everything until you
            choose the exceptions.
          </p>

          {isFetching && !policy ? (
            <p className="mt-3 text-sm text-slate-400">Loading…</p>
          ) : grouped.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No services configured yet.</p>
          ) : (
            <div className="mt-3 space-y-4">
              {grouped.map(([category, services]) => (
                <div key={category}>
                  <p className="mb-1.5 text-xs font-semibold uppercase tracking-wide text-slate-400">{category}</p>
                  <div className="grid grid-cols-1 gap-x-6 gap-y-1.5 sm:grid-cols-2 lg:grid-cols-3">
                    {services.map((s) => (
                      <label
                        key={s.ServiceID}
                        className={cn(
                          'flex cursor-pointer items-center gap-2 rounded-md px-2 py-1.5 text-sm transition-colors hover:bg-slate-50',
                          !s.IsActive && 'opacity-60',
                        )}
                      >
                        <input
                          type="checkbox"
                          className="size-4 rounded border-slate-300 text-brand-600 focus:ring-brand-500/30"
                          checked={allowed.has(s.ServiceID)}
                          onChange={() => toggleService(s.ServiceID)}
                        />
                        <span className="truncate text-slate-700">{s.Name}</span>
                        {!s.IsActive && <span className="shrink-0 text-[11px] text-slate-400">(off)</span>}
                      </label>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </CardBody>
    </Card>
  );
}
