/** Mirrors Fino_API `Responses/Dashboard/DashboardViewModel.cs`. */
export interface DashboardData {
  TotalSuccessAmount: number;
  TotalFailedAmount: number;
  TransferAmount: number;
  TransferCount: number;
  ComplaintAmount: number;
  ComplaintCount: number;
  KycCount: number;
  TotalTarget: number;
  RemainsTarget: number;
  PrePaidSuccess: number;
  PrePaidFail: number;
  PostPaidSuccess: number;
  PostPaidFail: number;
  DthSuccess: number;
  DthFail: number;
  DMTSuccess: number;
  DMTFail: number;
  AEPSSuccess: number;
  AEPSFail: number;
  PurchaseAmount: number;
  PurchaseCount: number;
  TotalCommission: number;
  Surcharge: number;
  TotalAdminCommission: number;
  TotalAdminSurcharge: number;
  /** Commission PAID DOWN to the network (retailer + distributor + super-distributor) on the day. */
  TotalUserCommission: number;
  TotalUserCommissionCount: number;
  AdminCommissionProfit: number;
  AdminSurchargeProfit: number;
  RofferComm: number;
  RofferTotal: number;
  RofferPer: string;
  // Admin balances
  AdminWalletBalance: number;
  TotalUserWalletBalance: number;
  ProviderStockTotal: number;
  FundRequestPendingAmount: number;
  FundRequestPendingCount: number;
  // Admin breakdowns (admin Select only)
  ProviderStocks?: ProviderStockItem[];
  UserTypeBalances?: UserTypeBalanceItem[];
  AdminWallets?: WalletBalanceItem[];
  ServiceSplit?: ServiceSplitItem[];
  MonthlyPerformance?: DayPerfItem[];
  // Distributor / Super-Distributor dashboard
  Downline?: DownlineSummaryItem[];
}

/** One direct downline user with wallet float + the day's transaction volume. */
export interface DownlineSummaryItem {
  UserID: number;
  FirmName: string;
  PersonName: string;
  UserTypeID: number;
  UserType: string;
  MobileNumber: string;
  IsActive: boolean;
  WalletBalance: number;
  SuccessAmount: number;
  FailedAmount: number;
  SuccessCount: number;
}

export interface DayPerfItem {
  Day: number;
  CurrentMonth: number;
  LastMonth: number;
}

export interface ProviderStockItem {
  ProviderID: number;
  ProviderName: string;
  RechargeBalance: number;
  DMTBalance: number;
  Total: number;
  IsActive: boolean;
}

export interface UserTypeBalanceItem {
  UserTypeID: number;
  UserType: string;
  UserCount: number;
  Balance: number;
}

export interface WalletBalanceItem {
  WalletID: number;
  WalletName: string;
  Balance: number;
}

export interface ServiceSplitItem {
  ServiceID: number;
  ServiceName: string;
  SuccessAmount: number;
  FailedAmount: number;
  SuccessCount: number;
  FailedCount: number;
}

/** One row of TodaySummary/Select. */
export interface TodaySummaryItem {
  Type: string;
  TotalAmount: number;
}
