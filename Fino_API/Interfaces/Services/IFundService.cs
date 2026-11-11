using FintechApp.Requests.Fund;
using FintechApp.Responses.Fund;

namespace FintechApp.Interfaces.Services
{
    public interface IFundService
    {
        Task<string> RequestInsert(FundInsertRequest request, int userID);
        /// <summary>
        /// The fund-request queue for one caller.
        ///
        /// <paramref name="userTypeID"/> is what makes this role-aware: admin and support are the
        /// platform's final approver and see the whole queue, while everyone else sees only the
        /// requests they raised and the requests raised TO them - never another upline's queue.
        /// Pass 0 to get the non-admin (strict) scoping.
        /// </summary>
        Task<(string failureMessage, List<FundRequestViewModel> data)> SelectByFilter(FundSelectRequest request, int userID, int userTypeID = 0);
        Task<(string failureMessage, FundRequestWrapper data)> SelectHeaderWithRequestByFilter(FundSelectRequest request, int userID, int userTypeID = 0);
        Task<(string failureMessage, FundRequestViewModel? data)> SelectByID(int userID, int fundTransactionID);
        Task<string> AcceptReject(FundUpdateRequest request, int userID);
        Task<string> Transfer(FundTransferRequest request, int userID, string ipAddress);
        Task<(string failureMessage, FundTransferReportWrapper data)> SelectTransferReport(FundTransferReportRequest request, int userID);
        Task<string> ReverseRequest(FundReverseInsertRequest request, int userID);
        Task<(string failureMessage, FundTransferReportWrapper data)> SelectReverseReport(FundTransferReportRequest request, int userID);
        Task<string> Reverse(FundReverseWithOTPRequest request, int userID, string ipAddress);
        Task<(string failureMessage, GenerateFundReverseOTPResponse? data)> GenerateFundReverseOTP(GenerateFundReverseOTPRequest request, int userID);
        Task<string> ReverseWithOTP(FundReverseWithOTPRequest request, int userID, string ipAddress);
        Task<(string failureMessage, List<FundRequestViewModel> data)> SelectLast10ByReceiverID(int userID, FundSelectLast10ByReceiverRequest request);
        Task<(string failureMessage, List<FundRequestViewModel> data)> SelectLast10Request(int userID);
        Task<(string failureMessage, List<FundRequestViewModel> data)> SelectLast10ForReverseTransaction(FundSelectLast10ForReverseRequest request, int userID);
        Task<(string failureMessage, EcollectionWrapper data)> SelectEcollection(FundSelectEcollectionRequest request);
        Task<string> Delete(int userID, int fundTransactionID);
        Task<(string failureMessage, FundTransferReportWrapper data)> SelectWalletTransferReport(FundTransferReportRequest request, int userID);
    }
}
