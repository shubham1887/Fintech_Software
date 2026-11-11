import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Check, Lock, Pencil, Plus, Star } from 'lucide-react';
import {
  getQuickServices, saveQuickServices, MAX_QUICK_SERVICES, type QuickService,
} from '@/api/quickService.api';
import { fetchCatalog, imageUrl, type CatalogService } from '@/api/catalog.api';
import { lockMessage, serviceHref, serviceLook } from '@/lib/serviceTiles';
import { cn } from '@/lib/cn';
import { haptic } from '@/lib/native';
import { useToast } from '@/components/ui/Toast';
import { Button } from '@/components/ui/Button';
import { Modal } from '@/components/ui/Modal';
import { Skeleton } from '@/components/ui/Skeleton';

/**
 * "Quick Services" — the shortcuts a user pins for themselves.
 *
 * The admin owns the catalogue and the order everyone sees it in; this is the row each user
 * arranges for their own day. Both surfaces render the same component so the phone and the
 * desktop can never drift apart, and the Android shell — which runs this same web app in a
 * WebView — gets it for free.
 *
 * The selection lives on the server, not in browser storage: a retailer who signs in on the
 * phone after setting it up on the counter PC should find their own shortcuts there, and
 * browser storage is per-device, per-browser and silently empty in a private window.
 */
export function QuickServices({ compact = false }: { compact?: boolean }) {
  const toast = useToast();
  const qc = useQueryClient();
  const [picking, setPicking] = useState(false);

  const pinnedQ = useQuery({ queryKey: ['quick-services'], queryFn: getQuickServices, staleTime: 60_000 });
  // The picker needs the catalogue; the row itself does not, so it is only fetched once the
  // user opens the picker (or when it is already cached from the dashboard's own grid).
  const catalogQ = useQuery({
    queryKey: ['service-catalog'], queryFn: fetchCatalog, staleTime: 300_000, enabled: picking,
  });

  const pinned = pinnedQ.data ?? [];
  const available = useMemo(
    () => (catalogQ.data ?? []).flatMap((c) => c.Services),
    [catalogQ.data],
  );

  const saveMut = useMutation({
    mutationFn: (ids: number[]) => saveQuickServices(ids),
    onSuccess: (res) => {
      if (!res.ok) { toast.error(res.message); return; }
      toast.success('Quick services updated.');
      setPicking(false);
      qc.invalidateQueries({ queryKey: ['quick-services'] });
    },
    onError: (e: Error) => toast.error(e.message),
  });

  // Nothing pinned and nothing loading → a single invitation rather than an empty card, so the
  // feature is discoverable instead of being a row that only appears once you already know.
  const empty = !pinnedQ.isLoading && pinned.length === 0;

  return (
    <>
      <section className={cn(!compact && 'rounded-card border border-slate-200/70 bg-white shadow-card')}>
        <div className={cn('flex items-center justify-between', compact ? 'mb-2 px-1' : 'px-5 pt-4')}>
          <h2
            className={cn(
              'flex items-center gap-1.5 font-semibold',
              compact
                ? 'text-[11px] uppercase tracking-wider text-slate-400'
                : 'font-display text-base text-slate-800',
            )}
          >
            <Star className={cn(compact ? 'size-3' : 'size-4', 'text-amber-500')} />
            Quick services
          </h2>
          <button
            type="button"
            onClick={() => { haptic(); setPicking(true); }}
            className="flex items-center gap-1 text-xs font-medium text-brand-600 hover:text-brand-700"
          >
            <Pencil className="size-3.5" />
            {empty ? 'Choose' : 'Edit'}
          </button>
        </div>

        <div className={cn(compact ? 'rounded-card border border-slate-200/70 bg-white p-3 shadow-card' : 'px-5 pb-4 pt-3')}>
          {pinnedQ.isLoading ? (
            <div className="grid grid-cols-4 gap-y-3 sm:grid-cols-6">
              {Array.from({ length: 4 }).map((_, i) => (
                <div key={i} className="flex flex-col items-center gap-2">
                  <Skeleton className="size-13 rounded-2xl" />
                  <Skeleton className="h-2.5 w-12" />
                </div>
              ))}
            </div>
          ) : empty ? (
            <button
              type="button"
              onClick={() => { haptic(); setPicking(true); }}
              className="flex w-full items-center justify-center gap-2 rounded-xl border border-dashed border-slate-300 py-5 text-sm text-slate-500 transition-colors hover:border-brand-300 hover:text-brand-600"
            >
              <Plus className="size-4" />
              Pin the services you use most
            </button>
          ) : (
            <div className="grid grid-cols-4 gap-y-3 sm:grid-cols-6 md:grid-cols-8">
              {pinned.map((svc) => <PinnedTile key={svc.ServiceID} svc={svc} />)}
            </div>
          )}
        </div>
      </section>

      <QuickServicePicker
        open={picking}
        onClose={() => setPicking(false)}
        loading={catalogQ.isLoading}
        available={available}
        pinned={pinned}
        saving={saveMut.isPending}
        onSave={(ids) => saveMut.mutate(ids)}
      />
    </>
  );
}

function PinnedTile({ svc }: { svc: QuickService }) {
  const look = serviceLook(svc.Name);
  const img = imageUrl(svc.ServiceImagePath);

  const inner = (
    <>
      <span
        className={cn(
          'relative grid size-13 place-items-center overflow-hidden rounded-2xl shadow-sm ring-1 ring-black/[0.04] transition-transform',
          svc.IsLocked ? 'bg-slate-100' : 'group-hover:scale-105',
          img && !svc.IsLocked ? 'bg-white' : !img && !svc.IsLocked ? look.tone : '',
        )}
      >
        {img
          ? <img src={img} alt="" className={cn('size-full object-contain p-2', svc.IsLocked && 'opacity-40 grayscale')} />
          : <look.icon className={cn('size-6', svc.IsLocked && 'text-slate-400')} />}
        {svc.IsLocked && (
          <span className="absolute inset-0 grid place-items-center bg-slate-900/5">
            <Lock className="size-4 text-slate-500" />
          </span>
        )}
      </span>
      <span className={cn('line-clamp-2 text-xs font-medium leading-tight', svc.IsLocked ? 'text-slate-400' : 'text-slate-600')}>
        {svc.Name}
      </span>
    </>
  );

  // A locked shortcut stays visible with its reason rather than silently disappearing — the user
  // pinned it deliberately, so it losing its link needs an explanation.
  if (svc.IsLocked) {
    return (
      <div title={lockMessage(svc.LockReason)} className="flex cursor-not-allowed flex-col items-center gap-2 p-2 text-center">
        {inner}
      </div>
    );
  }

  return (
    <Link
      to={serviceHref(svc.Name)}
      onClick={() => haptic()}
      className="group flex flex-col items-center gap-2 rounded-2xl p-2 text-center transition-all hover:-translate-y-0.5 hover:bg-brand-50/50"
    >
      {inner}
    </Link>
  );
}

/**
 * The picker. Order matters — tapping adds to the end, so the sequence a user taps in is the
 * sequence they get — and each chosen tile shows its position so that is visible rather than
 * implied.
 */
function QuickServicePicker({
  open, onClose, loading, available, pinned, saving, onSave,
}: {
  open: boolean;
  onClose: () => void;
  loading: boolean;
  available: CatalogService[];
  pinned: QuickService[];
  saving: boolean;
  onSave: (ids: number[]) => void;
}) {
  const [selected, setSelected] = useState<number[]>([]);

  // Re-seed from the saved list every time it opens, so closing without saving discards cleanly.
  useEffect(() => {
    if (open) setSelected(pinned.map((p) => p.ServiceID));
  }, [open, pinned]);

  const toggle = (id: number) => {
    haptic();
    setSelected((prev) => {
      if (prev.includes(id)) return prev.filter((x) => x !== id);
      if (prev.length >= MAX_QUICK_SERVICES) return prev;
      return [...prev, id];
    });
  };

  const full = selected.length >= MAX_QUICK_SERVICES;

  return (
    <Modal open={open} onClose={onClose} title="Choose your quick services" size="lg">
      <p className="mb-3 text-sm text-slate-500">
        Pick up to {MAX_QUICK_SERVICES} services to pin to the top of your dashboard. They appear in
        the order you tap them, and follow you to the mobile app.
      </p>

      {loading ? (
        <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
          {Array.from({ length: 9 }).map((_, i) => <Skeleton key={i} className="h-14 rounded-xl" />)}
        </div>
      ) : available.length === 0 ? (
        <p className="py-8 text-center text-sm text-slate-400">No services are enabled for your account yet.</p>
      ) : (
        <div className="max-h-[50vh] overflow-y-auto pr-1">
          <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
            {available.map((svc) => {
              const position = selected.indexOf(svc.ServiceID);
              const isOn = position >= 0;
              const look = serviceLook(svc.Name);
              const img = imageUrl(svc.ServiceImagePath);
              return (
                <button
                  key={svc.ServiceID}
                  type="button"
                  // A full list must not look clickable for the items it cannot accept.
                  disabled={!isOn && full}
                  onClick={() => toggle(svc.ServiceID)}
                  className={cn(
                    'flex items-center gap-2.5 rounded-xl border p-2.5 text-left transition-colors',
                    isOn ? 'border-brand-400 bg-brand-50/60' : 'border-slate-200 hover:border-slate-300',
                    !isOn && full && 'cursor-not-allowed opacity-40',
                  )}
                >
                  <span className={cn('grid size-9 shrink-0 place-items-center overflow-hidden rounded-lg', img ? 'bg-white ring-1 ring-black/[0.04]' : look.tone)}>
                    {img ? <img src={img} alt="" className="size-full object-contain p-1" /> : <look.icon className="size-4" />}
                  </span>
                  <span className="min-w-0 flex-1 truncate text-sm font-medium text-slate-700">{svc.Name}</span>
                  {isOn && (
                    <span className="grid size-5 shrink-0 place-items-center rounded-full bg-brand-600 text-[10px] font-bold text-white">
                      {position + 1}
                    </span>
                  )}
                </button>
              );
            })}
          </div>
        </div>
      )}

      <div className="mt-4 flex items-center justify-between gap-3 border-t border-slate-100 pt-3">
        <span className="text-xs text-slate-500">{selected.length} of {MAX_QUICK_SERVICES} selected</span>
        <div className="flex gap-2">
          <Button variant="ghost" onClick={onClose} disabled={saving}>Cancel</Button>
          <Button onClick={() => onSave(selected)} disabled={saving}>
            <Check className="size-4" />
            {saving ? 'Saving…' : 'Save'}
          </Button>
        </div>
      </div>
    </Modal>
  );
}
