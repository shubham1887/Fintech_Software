import { getField } from '@/lib/row';

type Row = Record<string, unknown>;

/**
 * The operators a report's Operator picker should offer for the currently-chosen service.
 *
 * The rule that matters: **no service chosen means every operator**, not none. Every report's
 * service filter defaults to "All services" (id 0), and no operator belongs to service 0 — so a
 * naive `filter(o => o.ServiceID === serviceId)` left the Operator dropdown empty on first open,
 * on every report that has one. It read as a filter that had never been wired up.
 *
 * Shared because this cascade was written out three times (transaction reports, the pending queues
 * and the provider daybook) and was wrong in all three.
 */
export function operatorsForService(allOperators: Row[], serviceId: string | number | undefined): Row[] {
  const id = String(serviceId ?? '0');
  if (!id || id === '0') return allOperators;
  return allOperators.filter((o) => String(getField(o, 'ServiceID') ?? '') === id);
}
