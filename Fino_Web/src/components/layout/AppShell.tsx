import { useEffect, useState } from 'react';
import { Outlet } from 'react-router-dom';
import { useUISettings } from '@/theme/UISettingsProvider';
import { useIsMobileApp } from '@/lib/native';
import { MobileShell } from './MobileShell';
import { Sidebar } from './Sidebar';
import { Topbar } from './Topbar';

export function AppShell() {
  const [mobileOpen, setMobileOpen] = useState(false);
  const { orientation } = useUISettings();
  const isMobile = useIsMobileApp();

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

  const main = (
    <main className="min-h-0 flex-1 overflow-y-auto">
      <div className="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
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
      <div className="bg-app-canvas flex h-screen flex-col overflow-hidden">
        <Topbar onMenuClick={() => setMobileOpen(true)} />
        {mobileSidebar}
        {main}
      </div>
    );
  }

  // Vertical (default): left sidebar + topbar.
  return (
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
  );
}
