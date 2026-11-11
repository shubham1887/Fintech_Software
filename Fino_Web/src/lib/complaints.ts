/**
 * Complaint state + who may do what with it. Mirrors `StaticModels/ComplaintStatus.cs` and the
 * authorization the API enforces, so the buttons a screen offers match what the server will accept.
 *
 * Rows written by older builds used SUCCESS / FAILED for the decided states, so nothing compares
 * raw status strings — everything goes through the helpers here.
 */
import { UserTypeCode } from '@/lib/roles';

export type ComplaintBucket = 'pending' | 'accept' | 'reject' | 'cancel';

const norm = (status: unknown) => String(status ?? '').trim().toUpperCase();

export function complaintBucket(status: unknown): ComplaintBucket {
  const s = norm(status);
  if (s === 'ACCEPT' || s === 'ACCEPTED' || s === 'SUCCESS') return 'accept';
  if (s === 'REJECT' || s === 'REJECTED' || s === 'FAILED' || s === 'FAIL') return 'reject';
  if (s === 'CANCEL' || s === 'CANCELLED' || s === 'CANCELED') return 'cancel';
  return 'pending';
}

/** Nobody has decided it yet — the only state from which it can still be actioned. */
export const isOpenComplaint = (status: unknown) => complaintBucket(status) === 'pending';

/**
 * Accept / Reject decide the dispute and Accept moves money (it refunds the transaction and
 * reverses every commission on it), so they belong to Admin and Support alone — the API refuses
 * them for anyone else.
 */
export const canResolveComplaints = (userTypeId: number) =>
  userTypeId === UserTypeCode.Admin || userTypeId === UserTypeCode.SupportUser;

/**
 * Everyone else gets Cancel: withdrawing a complaint they raised themselves. It closes the
 * complaint and moves nothing, which is why it is safe in a retailer's hands. An admin can also
 * cancel on a user's behalf, so this stays true for them too — the row-level check below is what
 * decides whether the button actually shows.
 */
export function canCancelComplaint(row: {
  status: unknown;
  ownerUserId: number;
}, currentUserId: number, userTypeId: number): boolean {
  if (!isOpenComplaint(row.status)) return false;
  return canResolveComplaints(userTypeId) || row.ownerUserId === currentUserId;
}
