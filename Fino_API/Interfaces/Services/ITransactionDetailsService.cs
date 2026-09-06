using FintechApp.Requests.TransactionDetails;
using FintechApp.Responses.TransactionDetails;
namespace FintechApp.Interfaces.Services
{
    public interface ITransactionDetailsService
    {
        Task<List<TransactionDetailsResponse>> GetAll();
        Task<TransactionDetailsResponse?> GetByID(int id);
        Task<List<TransactionDetailsResponse>> GetByUserID(int userid);
        Task<string> Create(CreateTransactionDetailsRequest request);
        Task<string> Update(UpdateTransactionDetailsRequest request);
        Task<string> Delete(int id);
        Task<AccountStatementWrapper> SelectForFilter(AccountStatementRequest request, int currentUserID, int currentUserTypeID);
        Task<AccountStatementWrapper> SelectAdminAccounting(AccountStatementRequest request, int currentUserID, int currentUserTypeID);
    }
}
