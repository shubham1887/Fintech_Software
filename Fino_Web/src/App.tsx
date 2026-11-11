import { lazy, Suspense } from 'react';
import { BrowserRouter, Navigate, Route, Routes, useParams } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import { queryClient } from '@/lib/queryClient';
import { BrandProvider } from '@/theme/BrandProvider';
import { AuthProvider } from '@/features/auth/AuthContext';
import { UISettingsProvider } from '@/theme/UISettingsProvider';
import { ToastProvider } from '@/components/ui/Toast';
import { ProtectedRoute } from '@/routes/ProtectedRoute';
import { AdminRoute } from '@/routes/AdminRoute';
import { AppShell } from '@/components/layout/AppShell';
import { BrandLoading } from '@/components/ui/BrandMark';
import LoginPage from '@/pages/LoginPage';
import ForgotPasswordPage from '@/pages/ForgotPasswordPage';
import RegisterPage from '@/pages/RegisterPage';

// Route-level code splitting — the heavy authenticated surfaces (dashboard charts, screen
// host, reports) load on demand so the login/first paint stays lean.
const DashboardPage = lazy(() => import('@/pages/DashboardPage'));
const RechargePage = lazy(() => import('@/pages/RechargePage'));
const DmtPage = lazy(() => import('@/pages/DmtPage'));
const TransactionsPage = lazy(() => import('@/pages/TransactionsPage'));
const AppearancePage = lazy(() => import('@/pages/AppearancePage'));
const ProfilePage = lazy(() => import('@/pages/ProfilePage'));
const PlaceholderPage = lazy(() => import('@/pages/PlaceholderPage'));
const ScreenHost = lazy(() => import('@/screens/ScreenHost'));
const PublicSite = lazy(() => import('@/site/PublicSite'));

/** Legacy /site/<slug> URLs → the new top-level /<slug>. */
function SiteRedirect() {
  const { slug } = useParams();
  return <Navigate to={`/${slug ?? ''}`} replace />;
}

/** Centered fallback while a lazy route chunk loads - branded, because in the phone app this is
 *  the first thing shown after the native splash hands over. */
function RouteFallback() {
  return <BrandLoading />;
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrandProvider>
        <BrowserRouter>
          <AuthProvider>
            <UISettingsProvider>
            <ToastProvider>
              <Suspense fallback={<RouteFallback />}>
                <Routes>
                  <Route path="/login" element={<LoginPage />} />
                  <Route path="/forgot-password" element={<ForgotPasswordPage />} />
                  <Route path="/register" element={<RegisterPage />} />
                  {/* Public website lives at the ROOT — Home at "/", other pages at "/<slug>".
                      The console keeps its own paths below; anything unmatched falls through
                      to the website renderer (which shows its own not-found state). */}
                  <Route path="/" element={<PublicSite />} />
                  <Route path="/site" element={<Navigate to="/" replace />} />
                  <Route path="/site/:slug" element={<SiteRedirect />} />
                  <Route element={<ProtectedRoute />}>
                    <Route element={<AppShell />}>
                      <Route path="/dashboard" element={<DashboardPage />} />
                      <Route path="/recharge" element={<RechargePage />} />
                      <Route path="/dmt" element={<DmtPage />} />
                      <Route path="/transactions" element={<TransactionsPage />} />
                      {/* Network-wide branding, not a personal preference — operator only. */}
                      <Route element={<AdminRoute />}>
                        <Route path="/appearance" element={<AppearancePage />} />
                        <Route path="/theme" element={<AppearancePage />} />
                      </Route>
                      <Route path="/profile" element={<ProfilePage />} />
                      <Route path="/coming-soon" element={<PlaceholderPage />} />
                      <Route path="/s/*" element={<ScreenHost />} />
                    </Route>
                  </Route>
                  <Route path="*" element={<PublicSite />} />
                </Routes>
              </Suspense>
            </ToastProvider>
            </UISettingsProvider>
          </AuthProvider>
        </BrowserRouter>
      </BrandProvider>
    </QueryClientProvider>
  );
}
