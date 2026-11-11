import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '@/features/auth/AuthContext';
import { UserTypeCode } from '@/lib/roles';

/**
 * Routes only the platform operator may open.
 *
 * Appearance / Theme Studio is the whole network's branding — colours, logo, login copy, the
 * mobile app's layout — not a per-user preference. It sat in every role's account menu, so any
 * retailer could open it, and what they changed there was not theirs to change. There is no
 * sidebar entry to hide it by (these pages live outside the role-filtered menu), so the gate is
 * stated here instead.
 *
 * Anyone else is sent to their dashboard rather than shown a refusal: they did not ask for a
 * page they were not meant to have, the menu offered it to them.
 */
export function AdminRoute() {
  const { user } = useAuth();
  if ((user?.UserTypeID ?? 0) !== UserTypeCode.Admin) {
    return <Navigate to="/dashboard" replace />;
  }
  return <Outlet />;
}
