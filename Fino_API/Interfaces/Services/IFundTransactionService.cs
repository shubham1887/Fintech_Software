using FintechApp.Requests.FundTransaction;
using FintechApp.Responses.FundTransaction;

namespace FintechApp.Interfaces.Services
{
    public interface IFundTransactionService
    {
        Task<FundTransactionResponse?> GetFundTransactionById(int fundTransactionId);
        Task<List<FundTransactionResponse>> GetFundTransactions(GetFundTransactionRequest request);
        Task<string> CreateFundTransaction(CreateFundTransactionRequest request);
        Task<string> UpdateFundTransaction(UpdateFundTransactionRequest request);
        Task<string> DeleteFundTransaction(int fundTransactionId);
    }
}