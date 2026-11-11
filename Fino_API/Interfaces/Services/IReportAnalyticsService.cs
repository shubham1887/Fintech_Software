using FintechApp.Requests.TransactionReport;
using FintechApp.Responses.TransactionReport;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Aggregate ("analytics") reports — the roll-up screens that summarise transactions by
    /// service, user, operator, amount band or day, plus the network / slab reports that sit
    /// beside them. Kept separate from <see cref="ITransactionReportService"/> (which returns
    /// raw transaction rows) because these need a wider set of repositories: wallets, fund
    /// transfers, slabs and onboarding.
    ///
    /// Every method takes the caller's identity and scopes the data itself: Admin / Support see
    /// the whole network, everyone else sees only themselves plus their downline — a requested
    /// UserID outside that branch is ignored rather than honoured.
    /// </summary>
    public interface IReportAnalyticsService
    {
        Task<(string fail, AnalyticsWrapper<ServiceSaleViewModel> data)> ServicewiseSale(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, AnalyticsWrapper<UserSaleViewModel> data)> UserwiseSale(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, AnalyticsWrapper<OperatorSaleViewModel> data)> OperatorSale(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, AnalyticsWrapper<TopPerformerViewModel> data)> TopPerformer(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, AnalyticsWrapper<AmountRangeViewModel> data)> RangewiseRecharge(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, AnalyticsWrapper<UserDMTRangeViewModel> data)> UserwiseDMTRange(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, AnalyticsWrapper<DailyServiceSaleViewModel> data)> DailyServiceSale(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, AnalyticsWrapper<UserServiceSellViewModel> data)> UserwiseServiceSell(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, AnalyticsWrapper<DatewiseUserViewModel> data)> DatewiseUsers(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, AnalyticsWrapper<DownlineReportViewModel> data)> DownlineReport(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, AnalyticsWrapper<UserOnboardingViewModel> data)> OnboardingList(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, AnalyticsWrapper<ServiceChargeDeductionViewModel> data)> ServiceChargeDeduction(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, AnalyticsWrapper<CommissionStructureViewModel> data)> CommissionStructure(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, AnalyticsWrapper<DaybookViewModel> data)> Daybook(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, AnalyticsWrapper<TdsViewModel> data)> Tds(TxnReportRequest request, int currentUserID, int currentUserTypeID);

        /// <summary>
        /// Profit and loss: opening, purchase, sale, closing, each commission tier, the GST on the
        /// downline and provider sides, the margin between them and the net profit. GroupBy picks
        /// the granularity - Date (default), Service, Operator or User.
        /// </summary>
        Task<(string fail, AnalyticsWrapper<ProfitLossViewModel> data)> ProfitLoss(TxnReportRequest request, int currentUserID, int currentUserTypeID);
    }
}
