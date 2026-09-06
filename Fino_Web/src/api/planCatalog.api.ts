import { apiPost } from '@/lib/apiClient';

/**
 * The plan provider's (planapi.in / Easytm) own reference codes, served from the API's
 * PlanApiCatalog so there is a single source of truth. These are NOT the app's operator ids.
 */

/** One telecom circle / state. Code is a string — several carry a leading zero ("02" Punjab). */
export interface PlanCircle {
  Id: number;
  Name: string;
  Code: string;
}

/** One operator as the plan provider lists it. */
export interface PlanOperatorCode {
  Id: number;
  Name: string;
  OpCode: string;
  Type: string;
}

/** All circle codes the plan provider publishes (31 rows — cache indefinitely). */
export async function fetchPlanCircles(): Promise<PlanCircle[]> {
  const res = await apiPost<PlanCircle[]>('ROffer/Circles', {});
  return res.ok && Array.isArray(res.data) ? res.data : [];
}

/** All operator codes the plan provider publishes, optionally one type ("DTH", "PREPAID", …). */
export async function fetchPlanOperatorCodes(type = ''): Promise<PlanOperatorCode[]> {
  const res = await apiPost<PlanOperatorCode[]>('ROffer/OperatorCodes', { Type: type });
  return res.ok && Array.isArray(res.data) ? res.data : [];
}
