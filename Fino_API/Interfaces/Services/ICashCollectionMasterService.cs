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
        Task<(string failureMessage, string result)> ReverseCashCollection(ReverseCashCollectionRequest request, int receiverID);
        Task<(string failureMessage, SelectByUserIDResponse? data)> SelectUserOutstanding(int userID);
        Task<string> SendCollectionMessage(int targetUserID, int adminUserID);
        Task<(string failureMessage, OutstandingWrapper? data)> OutstandingReport(SelectCashCollectionRequest request);
    }
}
