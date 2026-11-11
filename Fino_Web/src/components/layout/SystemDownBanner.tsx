import { useQuery } from '@tanstack/react-query';
import { AlertTriangle } from 'lucide-react';
import { apiPost } from '@/lib/apiClient';
import { useAuth } from '@/features/auth/AuthContext';
import { cn } from '@/lib/cn';

interface Status {
  IsDownNow: boolean;
  From: string;
  To: string;
  Message: string;
  AllowedServices: string[];
}

/**
 * Shown on every screen while a scheduled system-down window is open.
 *
 * Not dismissible, and deliberately so: the other banners on this shell carry information the
 * reader can choose to act on, and this one explains why the thing they are about to try is going
 * to be refused. A retailer who dismisses it and then fails three recharges calls support.
 *
 * Polled rather than read once at sign-in, because a window that starts at 22:00 has to reach a
 * page that was opened at 21:50 — and reach it before the first refused transaction does.
 */
export function SystemDownBanner({ className }: { className?: string }) {
  const { isAuthenticated } = useAuth();

  const { data } = useQuery({
    queryKey: ['system-down-status'],
    enabled: isAuthenticated,
    // Two minutes: close enough that the banner lands near the start of the window, far enough
    // apart that it is not a per-screen cost. The SERVER is what actually refuses transactions,
    // so nothing depends on this being instant.
    refetchInterval: 120_000,
    staleTime: 60_000,
    queryFn: async () => {
      const res = await apiPost<Status>('SystemDown/Status', {});
      return res.ok ? (res.data as Status) : null;
    },
  });

  if (!data?.IsDownNow) return null;

  const window = data.From && data.To ? `${data.From} – ${data.To}` : '';

  return (
    <div
      role="status"
      className={cn(
        'flex flex-wrap items-start gap-3 rounded-card border border-danger-200 bg-danger-50 px-4 py-3',
        className,
      )}
    >
      <AlertTriangle className="mt-0.5 size-5 shrink-0 text-danger-600" />
      <div className="min-w-0 flex-1 text-sm">
        <p className="font-semibold text-danger-800">
          System is down for maintenance{window && <span className="font-normal"> ({window})</span>}
        </p>
        <p className="mt-0.5 text-danger-700">{data.Message}</p>
        {data.AllowedServices.length > 0 && (
          <p className="mt-1 text-xs text-danger-700">
            Still working: {data.AllowedServices.join(', ')}
          </p>
        )}
      </div>
    </div>
  );
}
