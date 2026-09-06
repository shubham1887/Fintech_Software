using FintechApp.Requests.TransactionProcessing;
using FintechApp.Responses.TransactionMaster;
using FintechApp.Responses.TransactionProcessing;

namespace FintechApp.Interfaces.Services
{
    public interface ITransactionProcessingService
    {
        Task<(string fail, TxnListWrapper data)> Select(TxnSelectRequest request, int currentUserID);
        Task<(string fail, TransactionResponse? data)> SelectByID(TxnSelectByIDRequest request);
        Task<string> Update(TxnUpdateStatusRequest request, int currentUserID);
        Task<string> UpdateManualSuccessFail(TxnUpdateStatusRequest request, int currentUserID);
        Task<string> UpdateStatusManually(TxnUpdateStatusRequest request, int currentUserID);
        Task<string> UpdateStatusManuallyPendingProvider(TxnUpdateStatusRequest request, int currentUserID);
        Task<string> RechargeRefundAll(List<TxnBatchIDRequest> list, int currentUserID);
        Task<string> CMSRefundAll(List<TxnBatchIDRequest> list, int currentUserID);
        Task<string> DMTTransactionRefund(TxnBatchIDRequest request, int currentUserID);
        Task<string> RefundAll(List<TxnBatchIDRequest> list, int currentUserID);
        Task<string> RefundAllProviderPending(List<TxnBatchIDRequest> list, int currentUserID);
        Task<string> SuccessAll(List<TxnBatchIDRequest> list, int currentUserID);
        Task<string> SuccessAllProviderPending(List<TxnBatchIDRequest> list, int currentUserID);
        Task<(string fail, List<ProviderWisePerformanceViewModel> data)> AllProviderWisePerformance(TxnProviderWiseRequest request);
        Task<List<HourlyPerformanceViewModel>> HourlyPerformance();
        Task<List<Last12hrPerformanceViewModel>> Last12hrPerformance();
        Task<List<Last12hrPerformanceViewModel>> ProviderWisePerformance(TxnSingleProviderPerfRequest request);
        Task<string> TransactionResend(TxnSelectByIDRequest request, int currentUserID);
        Task<string> TransactionResendForSingleProvider(TxnDMTResendRequest request, int currentUserID, string ipAddress);
        Task<string> DMTTransactionResend(TxnDMTResendRequest request, int currentUserID, string ipAddress);
        Task<string> DMTTransactionResendAll(List<TxnDMTResendRequest> list, int currentUserID, string ipAddress);
        Task<string> DMTCMSTransactionResendAll(List<TxnDMTResendRequest> list, int currentUserID, string ipAddress);
        Task<string> GetAEPSTransactionStatus(TxnSelectByIDRequest request, int currentUserID, string ipAddress);
        Task<string> GetBBPSTransactionStatus(TxnSelectByIDRequest request, int currentUserID);
        Task<string> GetTransactionStatus(TxnSelectByIDRequest request, int currentUserID);
        // Generic provider status-check / resend for any service type (DMT/Payout/UPI/MicroATM/etc.)
        Task<string> CheckProviderStatus(int transactionID, string serviceType, int currentUserID, string ipAddress);
        // Provider push callback: upstream notifies us when a recharge moves PROCESS -> SUCCESS/FAILURE.
        Task<string> ApplyRechargeCallback(RechargeCallbackRequest request, string ipAddress);
        Task<string> ResendToProvider(int transactionID, string serviceType, int currentUserID, string ipAddress);
        Task<string> ShiftToProvider(int transactionID, int providerID, string serviceType, int currentUserID, string ipAddress);
        Task<string> ShiftToProviderBatch(List<TxnShiftRequest> list, string serviceType, int currentUserID, string ipAddress);
        Task<string> CheckProviderStatusBatch(List<TxnBatchIDRequest> list, string serviceType, int currentUserID, string ipAddress);
        Task<string> ResendToProviderBatch(List<TxnBatchIDRequest> list, string serviceType, int currentUserID, string ipAddress);
    }
}
