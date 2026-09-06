import { apiPost } from '@/lib/apiClient';
import { fetchList } from '@/api/list.api';

/**
 * Service Subscription API — subscription plans (a plan header + the services it bundles)
 * and the user-facing subscribe flow. Backed by the SubscriptionPlan controller, which keeps
 * the plan header (ServiceSubscriptionCreatePlanMaster) and its service links
 * (ServiceSubscriptionPlanDetails) consistent in one call.
 */

export interface SubscriptionPlan {
  ServiceSubscriptionCreatePlanMasterID: number;
  PlanName: string;
  Amount: number;
  ImageURL: string;
  IsDefault: boolean;
  ValidityDays: number;
  ServiceIDs: number[];
  ServiceCount: number;
  /** Only meaningful in the user-facing "available plans" view. */
  IsSubscribed: boolean;
}

/** The caller's subscription standing — drives the header counter, banner and gating. */
export interface MySubscriptionStatus {
  Enabled: boolean;
  Exempt: boolean;
  Required: boolean;
  Active: boolean;
  PlanID: number;
  PlanName: string;
  ExpiryDate: string | null;
  DaysRemaining: number;
}

/** One row of the admin "who is subscribed / who is out" list. */
export interface UserSubscriptionStatus {
  UserID: number;
  PersonName: string;
  FirmName: string;
  MobileNumber: string;
  UserTypeID: number;
  Exempt: boolean;
  Active: boolean;
  PlanName: string;
  ExpiryDate: string | null;
  DaysRemaining: number;
}

export interface ServiceOption {
  ServiceID: number;
  Name: string;
}

/** All services (admin catalog), for the plan builder's service picker. */
export async function fetchServices(): Promise<ServiceOption[]> {
  const rows = await fetchList('Service/GetAll', { IsActive: true, PageNumber: 1, PageSize: 500 });
  return rows
    .map((r) => ({ ServiceID: Number(r.ServiceID ?? r.serviceID ?? 0), Name: String(r.Name ?? r.name ?? '') }))
    .filter((s) => s.ServiceID > 0);
}

/** Admin: every plan with its bundled service ids. */
export async function fetchPlans(): Promise<SubscriptionPlan[]> {
  const res = await apiPost<SubscriptionPlan[]>('SubscriptionPlan/SelectPlans', {});
  return res.ok && Array.isArray(res.data) ? res.data : [];
}

/** User: available plans, each flagged with whether the caller is subscribed. */
export async function fetchAvailablePlans(): Promise<SubscriptionPlan[]> {
  const res = await apiPost<SubscriptionPlan[]>('SubscriptionPlan/AvailablePlans', {});
  return res.ok && Array.isArray(res.data) ? res.data : [];
}

export interface SavePlanInput {
  PlanID: number; // 0 = create
  PlanName: string;
  Amount: number;
  ImageURL: string;
  IsDefault: boolean;
  ValidityDays: number;
  ServiceIDs: number[];
}

export async function savePlan(input: SavePlanInput) {
  return apiPost('SubscriptionPlan/SavePlan', input);
}

export async function deletePlan(planId: number) {
  return apiPost('SubscriptionPlan/DeletePlan', { PlanID: planId });
}

/** Subscribe the current user (or, for admin, a chosen user) to a plan. Renewal uses the same call. */
export async function subscribeToPlan(planId: number, userId = 0) {
  return apiPost('SubscriptionPlan/Subscribe', {
    ServiceSubscriptionCreatePlanMasterID: planId,
    UserID: userId,
  });
}

/** The caller's subscription standing (also lazily locks lapsed paid services server-side). */
export async function fetchMyStatus(): Promise<MySubscriptionStatus> {
  const res = await apiPost<MySubscriptionStatus>('SubscriptionPlan/MyStatus', {});
  return (res.ok && res.data) ? res.data : {
    Enabled: false, Exempt: false, Required: false, Active: false,
    PlanID: 0, PlanName: '', ExpiryDate: null, DaysRemaining: 0,
  };
}

/** Admin: subscription standing for every user that can be required to subscribe. */
export async function fetchUsersStatus(): Promise<UserSubscriptionStatus[]> {
  const res = await apiPost<UserSubscriptionStatus[]>('SubscriptionPlan/UsersStatus', {});
  return res.ok && Array.isArray(res.data) ? res.data : [];
}

/** Admin: mark a user permanently free (cascades to their downline) or move them back to paid. */
export async function setExempt(userId: number, isExempt: boolean) {
  return apiPost('SubscriptionPlan/SetExempt', { UserID: userId, IsExempt: isExempt });
}
