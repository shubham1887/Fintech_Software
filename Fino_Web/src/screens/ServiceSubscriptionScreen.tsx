import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Package, Star, Check, ShieldCheck, Clock, RefreshCw, Sparkles, Lock } from 'lucide-react';
import {
  fetchAvailablePlans, fetchServices, subscribeToPlan, type SubscriptionPlan,
} from '@/api/subscription.api';
import { useSubscriptionStatus } from '@/features/subscription/useSubscriptionStatus';
import { imageUrl } from '@/api/catalog.api';
import { inr, dateOnly } from '@/lib/format';
import { cn } from '@/lib/cn';
import { useToast } from '@/components/ui/Toast';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Modal } from '@/components/ui/Modal';

export function ServiceSubscriptionScreen() {
  const toast = useToast();
  const qc = useQueryClient();
  const [confirm, setConfirm] = useState<SubscriptionPlan | null>(null);

  const status = useSubscriptionStatus();
  const { data: plans = [], isLoading } = useQuery({ queryKey: ['available-plans'], queryFn: fetchAvailablePlans });
  const { data: services = [] } = useQuery({ queryKey: ['sub-services'], queryFn: fetchServices });

  const serviceName = useMemo(() => {
    const m = new Map<number, string>();
    services.forEach((s) => m.set(s.ServiceID, s.Name));
    return m;
  }, [services]);

  const subscribe = useMutation({
    mutationFn: (plan: SubscriptionPlan) => subscribeToPlan(plan.ServiceSubscriptionCreatePlanMasterID),
    onSuccess: (r) => {
      if (r.ok) {
        toast.success(r.message || 'Subscription activated.');
        setConfirm(null);
        qc.invalidateQueries({ queryKey: ['available-plans'] });
        qc.invalidateQueries({ queryKey: ['subscription-status'] });
        qc.invalidateQueries({ queryKey: ['menu'] });
        qc.invalidateQueries({ queryKey: ['dashboard'] });
      } else {
        toast.error(r.message);
      }
    },
    onError: (e: Error) => toast.error(e.message),
  });

  return (
    <div className="space-y-6">
      <PageHeader title="Service Subscription" subtitle="Choose a plan to unlock its services" icon={Sparkles} />

      {/* Current standing */}
      {status?.Required && (
        status.Active ? (
          <div className="flex flex-wrap items-center gap-3 rounded-card border border-success-500/20 bg-success-50 px-5 py-4">
            <span className="grid size-10 shrink-0 place-items-center rounded-full bg-success-500/10 text-success-600">
              <ShieldCheck className="size-5" />
            </span>
            <div className="min-w-0 flex-1">
              <p className="text-sm font-semibold text-success-800">
                {status.PlanName} is active
              </p>
              <p className="text-xs text-success-700/80">
                {status.DaysRemaining} day{status.DaysRemaining === 1 ? '' : 's'} remaining
                {status.ExpiryDate && ` · renews by ${dateOnly(status.ExpiryDate)}`}
              </p>
            </div>
          </div>
        ) : (
          <div className="flex flex-wrap items-center gap-3 rounded-card border border-warning-500/25 bg-warning-50 px-5 py-4">
            <span className="grid size-10 shrink-0 place-items-center rounded-full bg-warning-500/15 text-warning-600">
              <Lock className="size-5" />
            </span>
            <div className="min-w-0 flex-1">
              <p className="text-sm font-semibold text-warning-800">Your subscription is inactive</p>
              <p className="text-xs text-warning-700/80">Paid services are locked. Pick a plan below to unlock them again.</p>
            </div>
          </div>
        )
      )}

      {isLoading ? (
        <div className="grid grid-cols-1 gap-5 md:grid-cols-2 xl:grid-cols-3">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="h-96 animate-pulse rounded-card border border-slate-200 bg-white" />
          ))}
        </div>
      ) : plans.length === 0 ? (
        <div className="rounded-card border border-dashed border-slate-300 bg-white px-6 py-20 text-center">
          <span className="mx-auto mb-4 grid size-14 place-items-center rounded-2xl bg-slate-100 text-slate-300">
            <Package className="size-7" />
          </span>
          <p className="text-base font-semibold text-slate-700">No plans available</p>
          <p className="mx-auto mt-1 max-w-sm text-sm text-slate-400">There are no subscription plans to show right now. Please check back later.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-5 md:grid-cols-2 xl:grid-cols-3">
          {plans.map((p) => {
            const id = p.ServiceSubscriptionCreatePlanMasterID;
            const isCurrent = status?.PlanID === id && status?.Active;
            const featured = p.IsDefault && !p.IsSubscribed;
            return (
              <div
                key={id}
                className={cn(
                  'group relative flex flex-col overflow-hidden rounded-card bg-white transition-all',
                  p.IsSubscribed
                    ? 'border border-success-500/30 shadow-[0_0_0_1px_rgba(16,185,129,0.12)]'
                    : featured
                      ? 'border border-brand-300 shadow-pop ring-1 ring-brand-500/15'
                      : 'border border-slate-200 shadow-card hover:-translate-y-0.5 hover:border-slate-300 hover:shadow-pop',
                )}
              >
                {/* Accent rail */}
                <span className={cn('absolute inset-x-0 top-0 h-1',
                  p.IsSubscribed ? 'bg-success-500' : featured ? 'bg-gradient-to-r from-brand-500 to-brand-600' : 'bg-transparent')} />

                {/* Ribbon */}
                {featured && (
                  <span className="absolute right-4 top-4 inline-flex items-center gap-1 rounded-full bg-brand-600 px-2.5 py-1 text-[11px] font-semibold uppercase tracking-wide text-white shadow-sm">
                    <Star className="size-3" /> Popular
                  </span>
                )}
                {p.IsSubscribed && (
                  <span className="absolute right-4 top-4 inline-flex items-center gap-1 rounded-full bg-success-500/10 px-2.5 py-1 text-[11px] font-semibold uppercase tracking-wide text-success-700">
                    <Check className="size-3" /> Subscribed
                  </span>
                )}

                <div className="flex flex-col p-6 pt-7">
                  {/* Identity */}
                  <div className="flex items-center gap-3">
                    {p.ImageURL ? (
                      <img src={imageUrl(p.ImageURL)} alt={p.PlanName} className="size-11 rounded-xl border border-slate-200 object-cover" />
                    ) : (
                      <span className={cn('grid size-11 place-items-center rounded-xl',
                        featured ? 'bg-brand-50 text-brand-600' : 'bg-slate-100 text-slate-400')}>
                        <Package className="size-5" />
                      </span>
                    )}
                    <h3 className="font-display text-base font-bold text-slate-900">{p.PlanName}</h3>
                  </div>

                  {/* Price */}
                  <div className="mt-5 flex items-end gap-1.5">
                    <span className="font-display text-4xl font-extrabold leading-none tracking-tight text-slate-900">{inr(p.Amount)}</span>
                    <span className="pb-1 text-sm font-medium text-slate-400">
                      {p.ValidityDays > 0 ? `/ ${p.ValidityDays} days` : '/ lifetime'}
                    </span>
                  </div>
                  {isCurrent && (
                    <p className="mt-2 inline-flex w-fit items-center gap-1.5 rounded-full bg-success-50 px-2.5 py-1 text-xs font-semibold text-success-700">
                      <Clock className="size-3.5" /> {status!.DaysRemaining} day{status!.DaysRemaining === 1 ? '' : 's'} left
                    </p>
                  )}

                  {/* Included */}
                  <div className="mt-6 border-t border-slate-100 pt-5">
                    <p className="mb-3 text-xs font-semibold uppercase tracking-wider text-slate-400">What's included</p>
                    <ul className="space-y-2.5">
                      {p.ServiceIDs.length === 0 && <li className="text-sm text-slate-400">No services in this plan.</li>}
                      {p.ServiceIDs.map((sid) => (
                        <li key={sid} className="flex items-start gap-2.5 text-sm text-slate-700">
                          <span className={cn('mt-0.5 grid size-4 shrink-0 place-items-center rounded-full',
                            p.IsSubscribed ? 'bg-success-500/15 text-success-600' : 'bg-brand-50 text-brand-600')}>
                            <Check className="size-3" strokeWidth={3} />
                          </span>
                          {serviceName.get(sid) ?? `Service #${sid}`}
                        </li>
                      ))}
                    </ul>
                  </div>
                </div>

                {/* CTA */}
                <div className="mt-auto p-6 pt-0">
                  {p.IsSubscribed ? (
                    <Button variant="outline" className="w-full" onClick={() => setConfirm(p)}>
                      <RefreshCw className="size-4" /> Renew plan
                    </Button>
                  ) : (
                    <Button variant={featured ? 'primary' : 'secondary'} className="w-full" onClick={() => setConfirm(p)}>
                      Subscribe
                    </Button>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Confirm */}
      <Modal open={!!confirm} onClose={() => setConfirm(null)} title={confirm?.IsSubscribed ? 'Confirm renewal' : 'Confirm subscription'} size="sm">
        {confirm && (
          <div className="space-y-5">
            <div className="flex items-center gap-3">
              {confirm.ImageURL ? (
                <img src={imageUrl(confirm.ImageURL)} alt={confirm.PlanName} className="size-12 rounded-xl border border-slate-200 object-cover" />
              ) : (
                <span className="grid size-12 place-items-center rounded-xl bg-brand-50 text-brand-600"><Package className="size-6" /></span>
              )}
              <div>
                <p className="font-display text-base font-bold text-slate-900">{confirm.PlanName}</p>
                <p className="text-xs text-slate-400">{confirm.ServiceCount} service{confirm.ServiceCount === 1 ? '' : 's'} · {confirm.ValidityDays > 0 ? `${confirm.ValidityDays} days` : 'lifetime'}</p>
              </div>
            </div>

            <div className="divide-y divide-slate-100 rounded-xl border border-slate-200 bg-slate-50/60 text-sm">
              <div className="flex items-center justify-between px-4 py-3">
                <span className="text-slate-500">{confirm.IsSubscribed ? 'Renewal charge' : 'Charge'}</span>
                <span className="font-display text-lg font-bold tabular-nums text-slate-900">{inr(confirm.Amount)}</span>
              </div>
              <div className="flex items-center justify-between px-4 py-3">
                <span className="text-slate-500">Validity</span>
                <span className="font-medium text-slate-700">{confirm.ValidityDays > 0 ? `${confirm.ValidityDays} days` : 'Lifetime'}</span>
              </div>
            </div>

            {confirm.Amount > 0 && (
              <p className="flex items-center gap-1.5 text-xs text-slate-400">
                <ShieldCheck className="size-3.5" /> {inr(confirm.Amount)} will be deducted from your Recharge wallet.
              </p>
            )}

            <div className="flex justify-end gap-2 border-t border-slate-100 pt-4">
              <Button variant="outline" onClick={() => setConfirm(null)} disabled={subscribe.isPending}>Cancel</Button>
              <Button onClick={() => subscribe.mutate(confirm)} loading={subscribe.isPending}>
                {confirm.IsSubscribed ? 'Confirm renewal' : 'Confirm & subscribe'}
              </Button>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}
