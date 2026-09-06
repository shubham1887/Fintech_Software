using FintechApp.Entities;
using FintechApp.Requests.FundTransaction;

namespace FintechApp.Interfaces.Repositories
{
    public interface IFundTransactionRepository
    {
        Task<List<FundTransaction>> GetAllFundTransactions();
        Task<FundTransaction?> GetFundTransactionById(int fundTransactionId);
        Task<List<FundTransaction>> GetFundTransactions(GetFundTransactionRequest request);
        Task<int> Insert(FundTransaction fundTransaction);
        Task<int> Update(FundTransaction fundTransaction);
        Task<int> Delete(FundTransaction fundTransaction);
        Task<int> GetPendingRequestCount(int requestToId);
        Task<int> GetPendingReverseRequestCount(int requestToId);
    }
}