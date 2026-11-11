using FintechApp.Requests.TransactionReport;
using FintechApp.Responses.TransactionReport;

namespace FintechApp.Interfaces.Services
{
    public interface ITransactionReportService
    {
        Task<(string fail, ProviderSaleWrapper data)> ProviderSale(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, PurchaseReportWrapper data)> PurchaseReport(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, TxnReportWrapper data)> SelectReport(TxnReportRequest request, int currentUserID);
        Task<(string fail, TxnSummaryViewModel data)> SelectSummary(TxnReportRequest request, int currentUserID);
        Task<(string fail, TxnReportWrapper data)> Recharge(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, TxnReportWrapper data)> Prepaid(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, TxnReportWrapper data)> AEPS(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, TxnReportWrapper data)> MicroATM(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, TxnReportWrapper data)> CashDeposit(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, TxnReportWrapper data)> LeadGeneration(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, TxnReportWrapper data)> LiveTransaction(TxnReportRequest request, int currentUserID);
        Task<(string fail, TxnReportWrapper data)> ManualSuccessFail(TxnReportRequest request, int currentUserID);
        Task<(string fail, TxnReportWrapper data)> LapuwiseReport(TxnReportRequest request, int currentUserID);
        Task<(string fail, List<UserWiseSummaryViewModel> data)> SelectByUserWise(TxnReportRequest request, int currentUserID);
        Task<(string fail, TxnReportWrapper data)> SelectByUserWiseByServiceID(TxnReportByServiceRequest request, int currentUserID, int currentUserTypeID = 0);
        Task<(string fail, TxnReportWrapper data)> SelectByUserWiseByTransactionMasterID(TxnReportByServiceRequest request, int currentUserID);
        Task<(string fail, TxnReportWrapper data)> SpecificUserReport(TxnReportRequest request, int currentUserID, int currentUserTypeID = 0);
        Task<(string fail, List<MonthlySummaryViewModel> data)> MonthlySummary(TxnReportRequest request, int currentUserID);
        Task<(string fail, List<GSTSummaryViewModel> data)> GSTsummary(TxnReportRequest request, int currentUserID);
        Task<(string fail, List<GSTSummaryViewModel> data)> GSTOperatorWise(TxnReportRequest request, int currentUserID);
        Task<(string fail, AnalyticsWrapper<IncomeSummaryViewModel> data)> Income(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, List<AmountInsightViewModel> data)> AmountWiseInsightsSelect(TxnReportRequest request, int currentUserID);
        Task<(string fail, List<StateWiseSummaryViewModel> data)> StateWiseSummary(TxnReportRequest request, int currentUserID);
        // Filter reports (web UI report screens) — service-scoped, with Service/Operator/Status filters.
        Task<(string fail, TxnReportWrapper data)> SelectForRechargeFilter(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        // Pending recharge queue (admin Pending Recharge page) — recharge txns still PENDING/PROCESS.
        Task<(string fail, TxnReportWrapper data)> SelectPendingTransactionWithFilter(TxnReportRequest request, int currentUserID, int currentUserTypeID);

        /// <summary>
        /// The pending queue for any service kind, named by request.ServiceType (a ServiceKinds
        /// constant). Blank or unknown = every service. See the implementation for why this
        /// replaces one endpoint per service.
        /// </summary>
        Task<(string fail, TxnReportWrapper data)> SelectPendingByKind(TxnReportRequest request, int currentUserID, int currentUserTypeID);

        /// <summary>
        /// Which pending queues exist on this install and what is waiting in each. Drives the
        /// queue hub, so the screen discovers the queues instead of being written to know them.
        /// </summary>
        Task<(string fail, PendingQueueSummary data)> SelectPendingQueues(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, TxnReportWrapper data)> SelectForDMTFilter(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, TxnReportWrapper data)> SelectForBBPS(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        Task<(string fail, TxnReportWrapper data)> Payout(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        // Quick-search a transaction by consumer number / reference / provider ref / id.
        Task<(string fail, TxnReportWrapper data)> Search(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        // Manually-settled transactions ("Past Success/Fail"), filtered by settlement date.
        Task<(string fail, TxnReportWrapper data)> SelectPastSuccessFail(TxnReportRequest request, int currentUserID, int currentUserTypeID);
        // DMT transactions still awaiting a final status (DMT pending queue).
        Task<(string fail, TxnReportWrapper data)> SelectDMTPendingTransactionWithFilter(TxnReportRequest request, int currentUserID, int currentUserTypeID);
    }
}
