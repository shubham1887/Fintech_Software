/**
 * The two pending queues — Recharge and DMT — described as data.
 *
 * They are the same job: find transactions the provider has not given a final answer for, re-ask,
 * re-send, or settle them by hand. Only the endpoints, the identifying columns and a couple of
 * capabilities differ, so both are rendered by one screen (`PendingTxnScreen`) driven by the entry
 * below. Adding a third queue (BBPS, payout) is a new entry here, not another copy of the screen.
 */
export interface PendingColumn {
  /** Field on the row; alternates are tried in order (older reports use different names). */
  keys: string[];
  label: string;
  /** 'money' right-aligns and formats; 'bold' emphasises the identifying value. */
  type?: 'money' | 'bold';
  /** Admin / support / upline only. */
  adminOnly?: boolean;
}

export interface PendingFamily {
  /**
   * Cache key for this queue. The two hand-built queues keep their names; every other queue is
   * keyed by its ServiceKinds constant, which is what the generic builder below passes.
   */
  key: string;
  /** Used in prose: "3 recharges settled", "no transfers are pending". */
  noun: string;
  nounPlural: string;
  endpoints: {
    list: string;
    statusCheckAll: string;
    successAll: string;
    refundAll: string;
    resendAll: string;
    statusCheckOne: string;
    resendOne: string;
    shiftOne?: string;
    shiftAll?: string;
    /** Send transfers the global hold parked — takes a list of { TransactionID }. */
    release?: string;
  };
  /** Columns between "Pending since" and the actions, in order. */
  columns: PendingColumn[];
  /** Re-dispatching through a chosen provider — recharge switching only. */
  supportsShift: boolean;
  /** The global "hold new transfers" switch — DMT only. */
  supportsHold: boolean;
  /** Search box label for the free-text filter. */
  searchLabel: string;
  searchPlaceholder: string;
  /**
   * Extra fields pinned onto every list request. The generic queues use it to name their service
   * kind (`ServiceType`), which is what lets one endpoint serve all of them.
   */
  listPayload?: Record<string, unknown>;
}

export const RECHARGE_FAMILY: PendingFamily = {
  key: 'recharge',
  noun: 'recharge',
  nounPlural: 'recharges',
  endpoints: {
    list: 'TransactionReport/SelectPendingTransactionWithFilter',
    statusCheckAll: 'TransactionProcessing/StatusCheckAll',
    successAll: 'TransactionProcessing/SuccessAll',
    refundAll: 'TransactionProcessing/RefundAll',
    resendAll: 'TransactionProcessing/ResendAll',
    statusCheckOne: 'TransactionProcessing/GetRechargeTransctionStatus',
    resendOne: 'TransactionProcessing/Resend',
    shiftOne: 'TransactionProcessing/ShiftRecharge',
    shiftAll: 'TransactionProcessing/ShiftAll',
  },
  columns: [
    { keys: ['FirmName', 'Name'], label: 'Name' },
    { keys: ['ConsumerNumber'], label: 'Number', type: 'bold' },
    { keys: ['OperatorName'], label: 'Operator' },
    { keys: ['Amount'], label: 'Amount', type: 'money' },
  ],
  supportsShift: true,
  supportsHold: false,
  searchLabel: 'Consumer number',
  searchPlaceholder: 'Mobile / account no.',
};

export const DMT_FAMILY: PendingFamily = {
  key: 'dmt',
  noun: 'transfer',
  nounPlural: 'transfers',
  endpoints: {
    list: 'TransactionReport/SelectDMTPendingTransactionWithFilter',
    statusCheckAll: 'TransactionProcessing/GetDMTTransctionStatusCheckAll',
    // Success / refund settle through the generic account pipeline, which is service-agnostic.
    successAll: 'TransactionProcessing/SuccessAll',
    refundAll: 'TransactionProcessing/RefundAll',
    resendAll: 'TransactionProcessing/DMTTransactionResendAll',
    statusCheckOne: 'TransactionProcessing/GetDMTTransctionStatus',
    resendOne: 'TransactionProcessing/DMTTransactionResend',
    // Releasing goes through the bank-transfer client (the gateway configured in DMT Settings),
    // not the recharge resend path — a held transfer has never been sent at all.
    release: 'TransactionHold/ReleaseTransactions',
    // Provider switching is a recharge concept; a transfer's gateway is chosen by the DMT settings.
  },
  columns: [
    // The DMT_* names are what the report actually returns; the bare ones are older aliases.
    { keys: ['DMT_SenderName', 'SenderName', 'FirmName'], label: 'Sender' },
    { keys: ['DMT_SenderNumber', 'SenderNo'], label: 'Sender mobile' },
    { keys: ['ConsumerNumber'], label: 'Account no.', type: 'bold' },
    { keys: ['DMT_Bank', 'Bank'], label: 'Bank' },
    { keys: ['DMT_BeneficiaryName', 'BeneficiaryName'], label: 'Beneficiary' },
    { keys: ['DMT_PaymentType', 'Type'], label: 'Mode' },
    { keys: ['Amount'], label: 'Amount', type: 'money' },
  ],
  supportsShift: false,
  supportsHold: true,
  searchLabel: 'Account / mobile',
  searchPlaceholder: 'Account no. / sender mobile',
};

/**
 * Marker the API writes into a transfer's description while the global hold is parking it. A held
 * transfer has been debited and commissioned but never sent upstream, which is a different thing
 * from "the provider has not answered yet" — and the queue has to say so.
 */
export const HELD_MARKER = '[HELD]';

/**
 * A pending queue for any service kind the install happens to sell.
 *
 * Recharge and DMT have hand-written descriptions above because each has something of its own —
 * provider switching for recharge, the global hold for DMT — and because their identifying
 * columns differ. Everything else (bill payment, electricity, payout, AEPS, Micro ATM, cash
 * deposit, and whatever the operator adds next) is the same job with the same columns, so it gets
 * one description built at run time from the kind.
 *
 * Adding the next service is now a ServiceMaster row, which the admin creates themselves — not an
 * endpoint, a screen and a menu entry in a release.
 *
 * The settle actions are the service-agnostic ones: StatusCheckAll / SuccessAll / RefundAll go
 * through the generic account pipeline, which is what the recharge and DMT queues already use for
 * the same three buttons.
 */
/**
 * Kinds whose stuck transactions can be RE-DISPATCHED through a provider of the admin's choosing.
 *
 * A shift re-sends the transaction down the operator -> provider route table, reusing the original
 * request id so the provider cannot charge twice. That only means anything where the transaction
 * was dispatched from an endpoint template in the first place, which is the recharge family and
 * the BBPS family (bill payment, electricity, gas, DTH, FASTag - everything the catalogue groups
 * under BBPS).
 *
 * Deliberately NOT offered for the rest, because the button would lie:
 *
 *  - DMT and PAYOUT go out on the bank-transfer rail, not the template path. Re-dispatching one
 *    here would push a bank transfer through the recharge endpoints; DMT has its own resend for
 *    exactly this reason, and payout belongs with it.
 *  - AEPS and MICRO ATM are device sessions. The customer's fingerprint or card is long gone, so
 *    there is nothing left to send to anybody.
 *  - CASH DEPOSIT is money handed over a counter; no provider dispatched it.
 *
 * Offering Shift on those would produce "the selected provider is not configured for this
 * operator" at best, and a transfer sent down the wrong rail at worst.
 */
const REDISPATCHABLE_KINDS = new Set(['RECHARGE', 'BBPS']);

export function genericFamily(kind: string, label: string): PendingFamily {
  const noun = label.toLowerCase();
  const canRedispatch = REDISPATCHABLE_KINDS.has(kind.toUpperCase());
  return {
    key: kind,
    noun,
    // Good enough for every label this is used in ("3 bill payments settled"). A kind whose
    // plural is irregular gets its own entry above rather than a pluralisation engine here.
    nounPlural: `${noun}s`,
    endpoints: {
      // One endpoint for every kind; ServiceType below says which.
      list: 'TransactionReport/SelectPendingByKind',
      statusCheckAll: 'TransactionProcessing/StatusCheckAll',
      successAll: 'TransactionProcessing/SuccessAll',
      refundAll: 'TransactionProcessing/RefundAll',
      resendAll: 'TransactionProcessing/ResendAll',
      statusCheckOne: 'TransactionProcessing/GetRechargeTransctionStatus',
      resendOne: 'TransactionProcessing/Resend',
      // Shift goes through the recharge endpoints whatever the service is: the controller's
      // "RECHARGE" label only means "work the rail out from the transaction", and the dispatch
      // itself reads the operator, service and amount off the transaction, so a bill payment
      // shifts through its own BBPS route table. Offered only where that is meaningful.
      ...(canRedispatch
        ? {
            shiftOne: 'TransactionProcessing/ShiftRecharge',
            shiftAll: 'TransactionProcessing/ShiftAll',
          }
        : {}),
    },
    columns: [
      { keys: ['FirmName', 'Name'], label: 'Name' },
      { keys: ['ConsumerNumber'], label: 'Number', type: 'bold' },
      { keys: ['OperatorName'], label: 'Operator' },
      { keys: ['Amount'], label: 'Amount', type: 'money' },
    ],
    supportsShift: canRedispatch,
    // The global transfer hold is a DMT concept and has its own screen; nothing else parks
    // transactions that way.
    supportsHold: false,
    searchLabel: 'Consumer number',
    searchPlaceholder: 'Mobile / account / consumer no.',
    listPayload: { ServiceType: kind },
  };
}
