import { apiPost } from '@/lib/apiClient';

/**
 * "Quick Services" — the shortcut tiles a user pins for themselves.
 *
 * Deliberately separate from the admin's catalogue order (ServiceMaster.UISequence): the admin
 * decides what the grid contains and the order everyone sees it in, and each user then pins the
 * handful they use all day. Stored server-side, not in browser storage, so the same shortcuts
 * follow the user from the web panel into the Android app and onto a new device.
 */
export interface QuickService {
  ServiceID: number;
  Name: string;
  ServiceCategory: string;
  ServiceImagePath: string;
  UISequence: number;
  /** Pinned but not usable right now (KYC pending / locked by the operator). */
  IsLocked: boolean;
  LockReason: string;
}

/** The most shortcuts one user may pin — mirrors UserQuickServiceService.MaxShortcuts. */
export const MAX_QUICK_SERVICES = 8;

/** The caller's own pinned tiles, in their chosen order. Never throws — an empty list is valid. */
export async function getQuickServices(): Promise<QuickService[]> {
  const res = await apiPost<QuickService[]>('QuickService/Select', {});
  return res.ok && Array.isArray(res.data) ? res.data : [];
}

/** Replace the caller's shortcuts with `serviceIds`, in that order. An empty list clears them. */
export async function saveQuickServices(serviceIds: number[]) {
  return apiPost('QuickService/Save', { ServiceIDs: serviceIds });
}
