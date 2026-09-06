import { useQuery } from '@tanstack/react-query';
import { fetchMyStatus, type MySubscriptionStatus } from '@/api/subscription.api';

/**
 * Single source of truth for subscription gating across the app (header counter, renew
 * banner, menu visibility, screen guards). One cached call to SubscriptionPlan/MyStatus,
 * which also lazily locks lapsed paid services server-side.
 */
export function useSubscriptionStatus() {
  const q = useQuery<MySubscriptionStatus>({
    queryKey: ['subscription-status'],
    queryFn: fetchMyStatus,
    staleTime: 60_000,
  });
  return q.data;
}
