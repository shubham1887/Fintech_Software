import { useEffect, useState } from 'react';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useUISettings } from '@/theme/UISettingsProvider';
import { useIsMobileApp } from '@/lib/native';
import { usePushBridge } from '@/lib/push';
import { useAuth } from '@/features/auth/AuthContext';
import { cn } from '@/lib/cn';
import { MpinGate } from '@/features/security/MpinGate';
import { MobileShell } from './MobileShell';
import { NewsBanner } from './NewsBanner';
import { Sidebar } from './Sidebar';
import { SystemDownBanner } from './SystemDownBanner';
import { Topbar } from './Topbar';
import { WorkQueueStrip } from './WorkQueueStrip';

export function AppShell() {
  const [mobileOpen, setMobileOpen] = useState(false);
  const { orientation } = useUISettings();
  const isMobile = useIsMobileApp();
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();
  // News belongs on the way in, not on top of every screen — an announcement repeated above a
  // transaction form is an interruption, not information.
  const onDashboard = useLocation().pathname === '/dashboard';

  // Hand the Android shell's FCM token to the API, and refresh the bell the moment a push
  // lands. Mounted here (above the desktop/phone split) so both layouts get it exactly once.
  usePushBridge(isAuthenticated, navigate);

  // The console owns its own scroll region (<main>). Lock the document while the shell is mounted
  // so the window can never add a second, outer scrollbar next to it. Restored on unmount so the
  // login/public pages keep their normal document scroll.
  useEffect(() => {
    const html = document.documentElement;
    const prevHtml = html.style.overflow;
    const prevBody = document.body.style.overflow;
    html.style.overflow = 'hidden';
    document.body.style.overflow = 'hidden';
    return () => {
      html.style.overflow = prevHtml;
      document.body.style.overflow = prevBody;
    };
  }, []);

  // Phone / Android shell get their own layout (bottom tabs + app home), not a squeezed
  // desktop console. Hooks above stay above this early return so the order never changes.
  if (isMobile) return <MobileShell />;

  // Vertical keeps a centred, capped column (there is already a sidebar eating the left).
  // Horizontal has no sidebar, so the top-nav spans the whole width and the canvas must match it —
  // a centred max-w column there just leaves big empty gutters on a wide monitor.
  const main = (
    <main className="min-h-0 flex-1 overflow-y-auto">
      <div className={cn('px-4 py-6 sm:px-6 lg:px-8', orientation === 'horizontal' ? 'w-full' : 'mx-auto max-w-7xl')}>
        {/* What is waiting for you, above whatever page you are on. Inside the scroll region
            rather than pinned to the header: it is a starting point, not a permanent fixture,
            and pinning it would cost a line of vertical space on every screen forever. */}
        {/* Above the work queue and on EVERY screen, unlike the news banner: it explains why the
            thing the reader is about to try will be refused, which is only useful before they
            try it. */}
        <SystemDownBanner className="mb-4" />
        <WorkQueueStrip className="mb-4" />
        {onDashboard && <NewsBanner className="mb-4" />}
        <Outlet />
      </div>
    </main>
  );

  // Mobile slide-over sidebar (shared by both orientations).
  const mobileSidebar = mobileOpen && (
    <div className="fixed inset-0 z-40 lg:hidden">
      <div className="absolute inset-0 bg-ink-950/50 backdrop-blur-sm" onClick={() => setMobileOpen(false)} />
      <div className="absolute inset-y-0 left-0 animate-rise">
        <Sidebar onNavigate={() => setMobileOpen(false)} />
      </div>
    </div>
  );

  // Horizontal: top nav bar (brand + menu + controls) above a full-width canvas.
  if (orientation === 'horizontal') {
    return (
      <MpinGate>
        <div className="bg-app-canvas flex h-screen flex-col overflow-hidden">
          <Topbar onMenuClick={() => setMobileOpen(true)} />
          {mobileSidebar}
          {main}
        </div>
      </MpinGate>
    );
  }

  // Vertical (default): left sidebar + topbar.
  return (
    <MpinGate>
      <div className="bg-app-canvas flex h-screen overflow-hidden">
        <div className="hidden shrink-0 lg:block">
          <Sidebar />
        </div>
        {mobileSidebar}
        <div className="flex min-h-0 min-w-0 flex-1 flex-col">
          <Topbar onMenuClick={() => setMobileOpen(true)} />
          {main}
        </div>
      </div>
    </MpinGate>
  );
}
