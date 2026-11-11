import type { ComponentType } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { getMenu } from '@/api/menu.api';
import { collectMenuUrls, isScreenAllowed } from '@/lib/access';
import { useAuth } from '@/features/auth/AuthContext';
import { Spinner } from '@/components/ui/Spinner';
import { findScreen, type CustomScreenProps } from './registry';
import { useSubscriptionStatus } from '@/features/subscription/useSubscriptionStatus';
import { ReportScreen } from './ReportScreen';
import { ListScreen } from './ListScreen';
import { CrudScreen } from './CrudScreen';
import { FormScreen } from './FormScreen';
import { TxnReportScreen } from './TxnReportScreen';
import { UsersScreen } from './UsersScreen';
import { UserFormScreen } from './UserFormScreen';
import { UserProfileScreen } from './UserProfileScreen';
import { ApiAccessScreen } from './ApiAccessScreen';
import { ApiDocsScreen } from './ApiDocsScreen';
import { OperatorsScreen } from './OperatorsScreen';
import { SlabCommissionScreen } from './SlabCommissionScreen';
import { FundRequestsScreen } from './FundRequestsScreen';
import { OutstandingScreen } from './OutstandingScreen';
import { OutstandingReportScreen } from './OutstandingReportScreen';
import { MenuVisibilityScreen } from './MenuVisibilityScreen';
import { AepsScreen } from './AepsScreen';
import { ProviderPerformanceScreen } from './ProviderPerformanceScreen';
import { UISettingsScreen } from './UISettingsScreen';
import { WebsiteBuilderScreen } from './WebsiteBuilderScreen';
import { VirtualBalanceScreen } from './VirtualBalanceScreen';
import { SlabAssignScreen } from './SlabAssignScreen';
import { SlabDelegationScreen } from './SlabDelegationScreen';
import { SlabAuditScreen } from './SlabAuditScreen';
import { ProviderHubScreen } from './ProviderHubScreen';
import { ProviderSettingScreen } from './ProviderSettingScreen';
import { ProviderSequenceScreen } from './ProviderSequenceScreen';
import { UserwiseSequenceScreen } from './UserwiseSequenceScreen';
import { ServiceCatalogScreen } from './ServiceCatalogScreen';
import { ModeMasterScreen } from './ModeMasterScreen';
import { PendingRechargeScreen, PendingDmtScreen } from './PendingQueueScreens';
import { PendingQueueHubScreen } from './PendingQueueHubScreen';
import { BulkActionScreen } from './BulkActionScreen';
import { MessageSettingScreen } from './MessageSettingScreen';
import { MessagingSetupScreen } from './MessagingSetupScreen';
import { ComplaintsScreen } from './ComplaintsScreen';
import { SecurityScreen } from './SecurityScreen';
import { TicketsScreen } from './TicketsScreen';
import { NotificationsScreen } from './NotificationsScreen';
import { AlertComposerScreen } from './AlertComposerScreen';
import { ProviderLogScreen } from './ProviderLogScreen';
import { MessageRulesScreen } from './MessageRulesScreen';
import { SmsLogScreen } from './SmsLogScreen';
import { ApiUserLogScreen } from './ApiUserLogScreen';
import { FundReverseScreen } from './FundReverseScreen';
import { ProviderDaybookScreen } from './ProviderDaybookScreen';
import { AdminAccountingScreen } from './AdminAccountingScreen';
import { TaxSimulatorScreen } from './TaxSimulatorScreen';
import { AdminLedgerScreen } from './AdminLedgerScreen';
import { PurchaseReportScreen } from './PurchaseReportScreen';
import { DMTSettingScreen } from './DMTSettingScreen';
import { KeySettingScreen } from './KeySettingScreen';
import { InvoiceSettingsScreen } from './InvoiceSettingsScreen';
import { TaxInvoicesScreen } from './TaxInvoicesScreen';
import { ExchangeRegisterScreen } from './ExchangeRegisterScreen';
import { EInvoiceQueueScreen } from './EInvoiceQueueScreen';
import { CreditNoteAlertsScreen } from './CreditNoteAlertsScreen';
import { PurchaseRegisterScreen } from './PurchaseRegisterScreen';
import { InvoicesScreen } from './InvoicesScreen';
import { GstMonthScreen } from './GstMonthScreen';
import { Gstr2bReconScreen } from './Gstr2bReconScreen';
import { ItcRegisterScreen } from './ItcRegisterScreen';
import { ChartOfAccountsScreen } from './ChartOfAccountsScreen';
import { LedgerVouchersScreen } from './LedgerVouchersScreen';
import { TrialBalanceScreen } from './TrialBalanceScreen';
import { BankReconScreen } from './BankReconScreen';
import { PartyStateScreen } from './PartyStateScreen';
import { CommissionTaxScreen } from './CommissionTaxScreen';
import { TdsSettingScreen } from './TdsSettingScreen';
import { TdsReportScreen } from './TdsReportScreen';
import { GstCollectionScreen } from './GstCollectionScreen';
import { BankSetupScreen } from './BankSetupScreen';
import { RegistrationApprovalsScreen } from './RegistrationApprovalsScreen';
import { InactiveUsersScreen } from './InactiveUsersScreen';
import { DeletedUsersScreen } from './DeletedUsersScreen';
import { KycApprovalsScreen } from './KycApprovalsScreen';
import { VerificationDocumentsScreen } from './VerificationDocumentsScreen';
import { UserAliasScreen } from './UserAliasScreen';
import { LoginHistoryScreen } from './LoginHistoryScreen';
import { SubscriptionPlansScreen } from './SubscriptionPlansScreen';
import { ServiceSubscriptionScreen } from './ServiceSubscriptionScreen';
import DmtPage from '@/pages/DmtPage';

/** Bespoke screens keyed by ScreenDef.component (kind 'custom'). */
const CUSTOM_SCREENS: Record<string, ComponentType<CustomScreenProps>> = {
  modeMaster: ModeMasterScreen,
  users: UsersScreen,
  userForm: UserFormScreen,
  userProfile: UserProfileScreen,
  apiAccess: ApiAccessScreen,
  apiDocs: ApiDocsScreen,
  operators: OperatorsScreen,
  slabCommission: SlabCommissionScreen,
  fundRequests: FundRequestsScreen,
  fundReverse: FundReverseScreen,
  apiUserLog: ApiUserLogScreen,
  outstanding: OutstandingScreen,
  outstandingReport: OutstandingReportScreen,
  menuVisibility: MenuVisibilityScreen,
  aeps: AepsScreen,
  providerPerformance: ProviderPerformanceScreen,
  uiSettings: UISettingsScreen,
  websiteBuilder: WebsiteBuilderScreen,
  virtualBalance: VirtualBalanceScreen,
  slabAssign: SlabAssignScreen,
  slabDelegation: SlabDelegationScreen,
  slabAudit: SlabAuditScreen,
  providerHub: ProviderHubScreen,
  providerSetting: ProviderSettingScreen,
  providerSequence: ProviderSequenceScreen,
  userwiseSequence: UserwiseSequenceScreen,
  serviceCatalog: ServiceCatalogScreen,
  pendingRecharge: PendingRechargeScreen,
  pendingDmt: PendingDmtScreen,
  // Every queue the install actually has, discovered from the service catalogue rather than
  // written one screen at a time. Recharge and DMT keep their own entries above because each has
  // a capability of its own, and both are reachable from here too.
  pendingQueues: PendingQueueHubScreen,
  bulkAction: BulkActionScreen,
  messageSetting: MessageSettingScreen,
  messagingSetup: MessagingSetupScreen,
  complaints: ComplaintsScreen,
  security: SecurityScreen,
  tickets: TicketsScreen,
  notifications: NotificationsScreen,
  alertComposer: AlertComposerScreen,
  providerLog: ProviderLogScreen,
  messageRules: MessageRulesScreen,
  smsLog: SmsLogScreen,
  providerDaybook: ProviderDaybookScreen,
  adminAccounting: AdminAccountingScreen,
  taxSimulator: TaxSimulatorScreen,
  adminLedger: AdminLedgerScreen,
  purchaseReport: PurchaseReportScreen,
  dmtSetting: DMTSettingScreen,
  keySettings: KeySettingScreen,
  invoiceSettings: InvoiceSettingsScreen,
  taxInvoices: TaxInvoicesScreen,
  exchangeRegister: ExchangeRegisterScreen,
  einvoiceQueue: EInvoiceQueueScreen,
  creditNoteAlerts: CreditNoteAlertsScreen,
  purchaseRegister: PurchaseRegisterScreen,
  invoices: InvoicesScreen,
  gstr2bRecon: Gstr2bReconScreen,
  itcRegister: ItcRegisterScreen,
  chartOfAccounts: ChartOfAccountsScreen,
  ledgerVouchers: LedgerVouchersScreen,
  trialBalance: TrialBalanceScreen,
  bankRecon: BankReconScreen,
  gstReturns: GstMonthScreen,
  partyState: PartyStateScreen,
  commissionTax: CommissionTaxScreen,
  tdsSetting: TdsSettingScreen,
  tdsMonthlyReport: TdsReportScreen,
  gstCollection: GstCollectionScreen,
  bankSetup: BankSetupScreen,
  registrationApprovals: RegistrationApprovalsScreen,
  inactiveUsers: InactiveUsersScreen,
  deletedUsers: DeletedUsersScreen,
  kycApprovals: KycApprovalsScreen,
  verificationDocuments: VerificationDocumentsScreen,
  userAlias: UserAliasScreen,
  loginHistory: LoginHistoryScreen,
  subscriptionPlans: SubscriptionPlansScreen,
  serviceSubscription: ServiceSubscriptionScreen,
  dmt: DmtPage,
};

// Subscription screens — reachable only while the feature (Key Setting) is on.
const SUBSCRIPTION_GATED = new Set(['/createplanindex', '/servicesubscriptioncharge']);

/** Shown when the signed-in role has no access to the requested screen. */
function NoAccess({ path }: { path: string }) {
  return (
    <div className="mx-auto max-w-md py-20 text-center">
      <h1 className="text-lg font-semibold text-slate-900">No access</h1>
      <p className="mt-2 text-sm text-slate-500">
        Your account does not have permission to open <span className="font-medium text-slate-700">{path}</span>.
      </p>
      <Link to="/dashboard" className="mt-6 inline-block text-sm font-medium text-brand-600 hover:underline">
        Back to dashboard
      </Link>
    </div>
  );
}

/**
 * Renders the registry screen for the current /s/<backend-url> path. Unregistered paths
 * fall back to the titled "coming soon" placeholder.
 *
 * Access is decided by `lib/access.ts` against the role-filtered menu the backend returns —
 * so typing the URL of a page your role cannot see in the sidebar (another tier's user list,
 * an admin setting) lands on "No access" instead of the screen. The API enforces the same
 * rules server-side; this only keeps the UI honest.
 */
export default function ScreenHost() {
  const location = useLocation();
  const sub = location.pathname.replace(/^\/s/, '') || '/';

  // Same query key + staleTime as the Sidebar, so this is a cache read, not a new call.
  const { data: menu, isPending: menuPending } = useQuery({ queryKey: ['menu'], queryFn: getMenu, staleTime: 5 * 60_000 });
  const subStatus = useSubscriptionStatus();
  const { user } = useAuth();

  const def = findScreen(sub);
  const subLower = sub.toLowerCase();

  // Wait for the menu before rendering anything — the gate must never fail open, so a screen
  // is not shown until we know the role is allowed to see it.
  if (menuPending) {
    return (
      <div className="flex min-h-[50vh] items-center justify-center">
        <Spinner className="size-6 text-brand-500" />
      </div>
    );
  }

  if (!isScreenAllowed(sub, collectMenuUrls(menu ?? []), def, user?.UserTypeID)) {
    return <NoAccess path={sub} />;
  }

  // Subscription feature off (Key Setting) → its pages are unreachable, even by direct URL.
  if (SUBSCRIPTION_GATED.has(subLower) && subStatus && subStatus.Enabled === false) {
    return <Navigate to="/dashboard" replace />;
  }

  if (def?.kind === 'custom' && def.component) {
    const Custom = CUSTOM_SCREENS[def.component];
    if (Custom) return <Custom key={def.path} def={def} />;
  }
  if (def?.kind === 'report' && def.endpoint) {
    return <ReportScreen key={def.path} title={def.title} endpoint={def.endpoint} subtitle={def.subtitle} />;
  }
  if (def?.kind === 'list' && def.endpoint) {
    return (
      <ListScreen key={def.path} title={def.title} endpoint={def.endpoint} payload={def.payload} subtitle={def.subtitle} />
    );
  }
  if (def?.kind === 'crud' && def.crud) {
    return <CrudScreen key={def.path} title={def.title} subtitle={def.subtitle} crud={def.crud} />;
  }
  if (def?.kind === 'form' && def.form) {
    return <FormScreen key={def.path} title={def.title} subtitle={def.subtitle} form={def.form} />;
  }
  if (def?.kind === 'txn' && def.txn) {
    return <TxnReportScreen key={def.path} title={def.title} subtitle={def.subtitle} config={def.txn} />;
  }

  const title = sub.replace(/^\//, '').replace(/([a-z0-9])([A-Z])/g, '$1 $2') || 'Screen';
  return <Navigate to={`/coming-soon?title=${encodeURIComponent(title)}`} replace />;
}
