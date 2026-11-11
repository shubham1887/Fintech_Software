/**
 * The two pending queues. Both are the same workbench (`PendingTxnScreen`) with a different
 * family description — see `pendingFamilies.ts`.
 */
import { PendingTxnScreen } from './PendingTxnScreen';
import { RECHARGE_FAMILY, DMT_FAMILY } from './pendingFamilies';
import type { CustomScreenProps } from './registry';

export function PendingRechargeScreen(props: CustomScreenProps) {
  return <PendingTxnScreen {...props} family={RECHARGE_FAMILY} />;
}

export function PendingDmtScreen(props: CustomScreenProps) {
  return <PendingTxnScreen {...props} family={DMT_FAMILY} />;
}
