import { useState } from 'react';
import { Bell, CheckCheck } from 'lucide-react';

/**
 * Notifications bell. The backend push/alert feed isn't wired yet, so this shows a clean
 * "all caught up" state — present so the console reads as a complete product surface.
 */
export function NotificationsBell() {
  const [open, setOpen] = useState(false);

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => setOpen((v) => !v)}
        className="relative grid size-9 place-items-center rounded-lg text-slate-500 transition-colors hover:bg-slate-100"
        aria-label="Notifications"
      >
        <Bell className="size-5" />
        <span className="absolute right-2 top-2 size-1.5 rounded-full bg-brand-500 ring-2 ring-white" />
      </button>

      {open && (
        <>
          <div className="fixed inset-0 z-10" onClick={() => setOpen(false)} />
          <div className="absolute right-0 z-20 mt-2 w-72 overflow-hidden rounded-xl border border-slate-200 bg-white shadow-pop">
            <div className="flex items-center justify-between border-b border-slate-100 px-4 py-3">
              <p className="text-sm font-semibold text-slate-800">Notifications</p>
              <CheckCheck className="size-4 text-slate-300" />
            </div>
            <div className="flex flex-col items-center gap-1 px-6 py-10 text-center">
              <span className="grid size-11 place-items-center rounded-full bg-slate-50 text-slate-300">
                <Bell className="size-5" />
              </span>
              <p className="mt-2 text-sm font-medium text-slate-600">You're all caught up</p>
              <p className="text-xs text-slate-400">New alerts and approvals will show up here.</p>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
