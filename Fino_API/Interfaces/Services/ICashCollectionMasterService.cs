using FintechApp.Entities;
using FintechApp.Requests.CashCollection;
using FintechApp.Requests.CashCollectionMaster;
using FintechApp.Responses.CashCollection;
using FintechApp.Responses.CashCollectionMaster;

namespace FintechApp.Interfaces.Services
{
    public interface ICashCollectionMasterService
    {
        Task<List<CashCollectionMasterResponse>> GetAll();
        Task<CashCollectionMasterResponse?> GetByID(int id);
        Task<List<CashCollectionMasterResponse>> GetByUserID(int userID);
        Task<string> Create(CreateCashCollectionMasterRequest request);
        Task<string> Update(UpdateCashCollectionMasterRequest request);
        Task<string> Delete(int id);

        Task<(string failureMessage, string result)> AddCashCollection(AddCashCollectionRequest request, int senderID);
        Task<(string failureMessage, string result)> ReverseCashCollection(ReverseCashCollectionRequest request, int actorID);
        Task<(string failureMessage, SelectByUserIDResponse? data)> SelectUserOutstanding(int userID);
        Task<string> SendCollectionMessage(int targetUserID, int actorID);

        /// <summary>
        /// "Who owes how much", scoped to the caller: the whole network for admin and support,
        /// own downline for everyone else. The caller's role comes from the signed token, never
        /// from the request.
        /// </summary>
        Task<(string failureMessage, OutstandingWrapper? data)> OutstandingReport(
            SelectCashCollectionRequest request, int callerID, int callerTypeID);

        /// <summary>One user's statement — the page behind the View button.</summary>
        Task<(string failureMessage, OutstandingStatementWrapper? data)> Statement(
            OutstandingStatementRequest request, int callerID, int callerTypeID);

        /// <summary>
        /// The shared reminder text. Public so the daily sweep sends exactly what the on-demand
        /// "remind now" button sends.
        /// </summary>
        Task SendReminder(UserMaster user, decimal outstanding, string parentName);
    }
}
